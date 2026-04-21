using System.ComponentModel;
using Microsoft.SemanticKernel;
using ECommerceAgent.Application.Interfaces;

namespace ECommerceAgent.ConsoleApp.Plugins;

[Description("Sepet yönetimi işlemlerini gerçekleştiren plugin")]
public class CartPlugin
{
    private readonly ICartService _cartService;

    public CartPlugin(ICartService cartService)
    {
        _cartService = cartService;
    }

    [KernelFunction("AddToCart")]
    [Description("Belirtilen ürünü sepete ekler. Stok kontrolü yapar.")]
    public string AddToCart(
        [Description("Eklenecek ürünün ID'si")] string productId,
        [Description("Eklenecek adet (varsayılan: 1)")] int quantity = 1)
    {
        // TODO: Pair programming ile implemente edilecek
        throw new NotImplementedException();
    }

    [KernelFunction("RemoveFromCart")]
    [Description("Belirtilen ürünü sepetten çıkarır.")]
    public string RemoveFromCart(
        [Description("Çıkarılacak ürünün ID'si")] string productId)
    {
        // TODO: Pair programming ile implemente edilecek
        throw new NotImplementedException();
    }

    [KernelFunction("GetCart")]
    [Description("Sepetteki tüm ürünleri ve toplam tutarı gösterir.")]
    public string GetCart()
    {
        // TODO: Pair programming ile implemente edilecek
        throw new NotImplementedException();
    }
}
