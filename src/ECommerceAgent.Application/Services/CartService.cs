using System.Text;
using System.Text.Json;
using ECommerceAgent.Application.Interfaces;
using ECommerceAgent.Domain.Interfaces;

namespace ECommerceAgent.Application.Services;

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
            return $"Hata: '{productId}' ID'li urun bulunamadi. Lutfen once urun arayin.";

        if (quantity <= 0)
            return "Hata: Miktar 0'dan buyuk olmalidir.";

        var currentInCart = _cartRepository.GetItemQuantity(productId);
        var availableStock = product.Stock - currentInCart;

        if (availableStock <= 0)
            return $"Hata: '{product.Name}' stokta kalmadi.";

        var actualQuantity = Math.Min(quantity, availableStock);
        _cartRepository.AddItem(productId, actualQuantity);

        var message = new StringBuilder();
        message.Append($"OK: '{product.Name}' x{actualQuantity} sepete eklendi.");

        if (actualQuantity < quantity)
            message.Append($" Uyari: Stokta yalnizca {availableStock} adet vardi, {actualQuantity} adet eklendi.");

        message.Append($" Birim fiyat: {FormatPrice(product.Price)}.");

        return message.ToString();
    }

    public string RemoveFromCart(string productId)
    {
        var product = _productRepository.GetById(productId);
        var productName = product?.Name ?? productId;

        var removed = _cartRepository.RemoveItem(productId);
        if (!removed)
            return $"Hata: '{productName}' sepetinizde bulunamadi.";

        return $"OK: '{productName}' sepetten cikarildi.";
    }

    public string GetCart()
    {
        var items = _cartRepository.GetAllItems().ToList();

        if (!items.Any())
            return "Sepetiniz bos.";

        var cartSummary = new
        {
            Items = items.Select(i => new
            {
                i.ProductId,
                i.ProductName,
                i.Quantity,
                UnitPrice = FormatPrice(i.UnitPrice),
                TotalPrice = FormatPrice(i.TotalPrice)
            }),
            TotalAmount = FormatPrice(items.Sum(i => i.TotalPrice)),
            ItemCount = items.Count
        };

        return JsonSerializer.Serialize(cartSummary, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    private static string FormatPrice(decimal price)
    {
        return $"{price:F2} TL";
    }
}
