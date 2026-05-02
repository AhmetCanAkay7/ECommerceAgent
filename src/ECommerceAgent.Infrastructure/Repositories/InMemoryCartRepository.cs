using ECommerceAgent.Domain.Entities;
using ECommerceAgent.Domain.Interfaces;
using ECommerceAgent.Infrastructure.Data;

namespace ECommerceAgent.Infrastructure.Repositories;

public class InMemoryCartRepository : ICartRepository
{
    public void AddItem(string productId, int quantity)
    {
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
        return MockStore.Cart.Remove(productId);
    }

    public IEnumerable<CartItem> GetAllItems()
    {
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
