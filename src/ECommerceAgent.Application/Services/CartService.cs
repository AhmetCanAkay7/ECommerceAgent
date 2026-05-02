using System.Text;
using System.Text.Json;
using ECommerceAgent.Application.Interfaces;
using ECommerceAgent.Domain.Entities;
using ECommerceAgent.Domain.Interfaces;

namespace ECommerceAgent.Application.Services;

/// 1. Her metot string döner — çünkü bu string doğrudan LLM'e gider.
///    LLM bu mesajı okuyup kullanıcıya doğal dilde aktarır.

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public CartService(ICartRepository cartRepository, IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public string AddToCart(string productId, int quantity)
    {
        var product = _productRepository.GetById(productId);
        if (product == null)
            return $"Hata: '{productId}' ID'li ürün bulunamadı. Lütfen önce ürün arayın.";

        if (quantity <= 0)
            return "Hata: Miktar 0'dan büyük olmalıdır.";

        var currentInCart = _cartRepository.GetItemQuantity(productId);
        var availableStock = product.Stock - currentInCart;

        if (availableStock <= 0)
            return $"Hata: '{product.Name}' stokta kalmadı.";

        var actualQuantity = Math.Min(quantity, availableStock);
        _cartRepository.AddItem(productId, actualQuantity);

        var message = new StringBuilder();
        message.Append($"✅ '{product.Name}' x{actualQuantity} sepete eklendi.");

        if (actualQuantity < quantity)
            message.Append($" ⚠️ Stokta yalnızca {availableStock} adet vardı, {actualQuantity} adet eklendi.");

        message.Append($" (Birim fiyat: {product.Price:C})");

        return message.ToString();
    }

    public string RemoveFromCart(string productId)
    {
        // Ürün bilgisini al (mesajda adını göstermek için)
        var product = _productRepository.GetById(productId);
        var productName = product?.Name ?? productId;

        var removed = _cartRepository.RemoveItem(productId);
        if (!removed)
            return $"Hata: '{productName}' sepetinizde bulunamadı.";

        return $"✅ '{productName}' sepetten çıkarıldı.";
    }

    public string GetCart()
    {
        var items = _cartRepository.GetAllItems().ToList();

        if (!items.Any())
            return "Sepetiniz boş.";

        // JSON formatında dön — LLM yapılandırılmış veriyi daha iyi okur
        var cartSummary = new
        {
            Items = items.Select(i => new
            {
                i.ProductId,
                i.ProductName,
                i.Quantity,
                UnitPrice = $"{i.UnitPrice:F2} TL",
                TotalPrice = $"{i.TotalPrice:F2} TL"
            }),
            TotalAmount = $"{items.Sum(i => i.TotalPrice):F2} TL",
            ItemCount = items.Count
        };

        return JsonSerializer.Serialize(cartSummary, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }
}
