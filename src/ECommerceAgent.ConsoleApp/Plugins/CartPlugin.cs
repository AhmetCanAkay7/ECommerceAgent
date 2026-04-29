using System.ComponentModel;
using Microsoft.SemanticKernel;
using ECommerceAgent.Application.Interfaces;

namespace ECommerceAgent.ConsoleApp.Plugins;

/// <summary>
/// Sepet yönetimi plugin'i — LLM'in "sepete ekleme/çıkarma/görüntüleme" yetenekleri.
/// 
/// Bu plugin'de 3 KernelFunction var. LLM her mesajda hangisini çağıracağına
/// (veya hiçbirini çağırmayacağına) KENDI BAŞINA karar verir.
/// 
/// Agentic öğrenme noktaları:
/// - AddToCart vs RemoveFromCart → LLM "ekle" mi "çıkar" mı dediğini anlamalı
/// - GetCart → Parametresiz tool — bazen bilgi almak için tool çağırmak yeterli
/// - quantity default 1 → LLM miktar belirtmezse varsayılan kullanılır
/// </summary>
[Description("Sepet yönetimi işlemlerini gerçekleştiren plugin")]
public class CartPlugin
{
    private readonly ICartService _cartService;

    public CartPlugin(ICartService cartService)
    {
        _cartService = cartService;
    }

    [KernelFunction("AddToCart")]
    [Description("Belirtilen ürünü sepete ekler. Stok kontrolü yapar. Stokta yoksa veya yetersizse bilgi mesajı döner.")]
    public string AddToCart(
        [Description("Eklenecek ürünün ID'si (örn: sut-001, gida-002)")] string productId,
        [Description("Eklenecek adet (varsayılan: 1)")] int quantity = 1)
    {
        // Service'e delege et — iş kuralları (stok kontrolü vb.) orada.
        // Dönen string doğrudan LLM'e gider.
        return _cartService.AddToCart(productId, quantity);
    }

    [KernelFunction("RemoveFromCart")]
    [Description("Belirtilen ürünü sepetten tamamen çıkarır.")]
    public string RemoveFromCart(
        [Description("Çıkarılacak ürünün ID'si (örn: sut-001, gida-002)")] string productId)
    {
        return _cartService.RemoveFromCart(productId);
    }

    [KernelFunction("GetCart")]
    [Description("Sepetteki tüm ürünleri, adetlerini, birim fiyatlarını ve toplam tutarı gösterir.")]
    public string GetCart()
    {
        // Parametresiz tool — LLM "sepetimde ne var?" deyince çağırır.
        // Sepet boşsa "Sepetiniz boş." döner.
        return _cartService.GetCart();
    }
}
