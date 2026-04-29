using ECommerceAgent.Domain.Entities;
using ECommerceAgent.Domain.Interfaces;
using ECommerceAgent.Infrastructure.Data;

namespace ECommerceAgent.Infrastructure.Repositories;

/// <summary>
/// In-memory sepet repository'si. MockStore.Cart üzerinde CRUD işlemleri yapar.
/// 
/// Agentic perspektiften neden önemli?
/// → Sepet, agent'ın "state" kavramını öğrendiği yerdir.
/// → Agent bir tool çağırdığında state değişir (side effect).
/// → Sonraki tool çağrıları bu state'ten etkilenir.
/// → Bu, IBM sunumundaki "session / state yönetimi" konusunun karşılığıdır.
/// </summary>
public class InMemoryCartRepository : ICartRepository
{
    public void AddItem(string productId, int quantity)
    {
        // Sepette zaten varsa → mevcut miktar üzerine ekle
        // Yoksa → yeni kayıt oluştur
        // Bu basit bir Dictionary işlemi:
        //   Cart["sut-001"] = Cart.GetValueOrDefault("sut-001", 0) + quantity
        if (MockStore.Cart.ContainsKey(productId))
        {
            MockStore.Cart[productId] += quantity;
        }
        else
        {
            MockStore.Cart[productId] = quantity;
        }
    }

    public bool RemoveItem(string productId)
    {
        // Sepette yoksa false dön → üst katman "bu ürün sepetinizde yok" diyecek
        // Varsa tamamen sil (kısmi silme yok — basit tutuyoruz)
        return MockStore.Cart.Remove(productId);
    }

    public IEnumerable<CartItem> GetAllItems()
    {
        // Cart Dictionary'si sadece productId → quantity tutuyor.
        // Ama agent'a ürün adı ve fiyatı da lazım.
        // Bu yüzden Cart + Products'ı birleştiriyoruz (join).
        //
        // Neden burada yapıyoruz, service'de değil?
        // → Repository'nin işi veriyi şekillendirmek.
        // → Service'in işi iş kuralları uygulamak.
        return MockStore.Cart
            .Select(cartEntry =>
            {
                var product = MockStore.Products.FirstOrDefault(p => p.Id == cartEntry.Key);
                return new CartItem
                {
                    ProductId = cartEntry.Key,
                    ProductName = product?.Name ?? "Bilinmeyen Ürün",
                    UnitPrice = product?.Price ?? 0,
                    Quantity = cartEntry.Value
                    // TotalPrice otomatik hesaplanır (CartItem.TotalPrice => UnitPrice * Quantity)
                };
            })
            .ToList();
    }

    public void Clear()
    {
        MockStore.Cart.Clear();
    }

    public int GetItemQuantity(string productId)
    {
        // Sepette yoksa 0 döner
        return MockStore.Cart.GetValueOrDefault(productId, 0);
    }
}
