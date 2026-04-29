using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;
using ECommerceAgent.Application.Interfaces;

namespace ECommerceAgent.ConsoleApp.Plugins;

/// <summary>
/// Ürün arama plugin'i — LLM'in "ürün kataloğunda arama yapma" yeteneği.
/// 
/// [KernelFunction] → Bu metodu Semantic Kernel'ın tool olarak görmesini sağlar.
/// [Description]    → LLM bu açıklamayı okuyarak "bu tool'u ne zaman çağırmalıyım?" kararını verir.
/// 
/// ⚠️ Description yazarken IBM sunumundan öğrendiğimiz kurallar:
/// 1. Net ve spesifik ol — "Ürün arar" yerine "Ürün kataloğunda anahtar kelimeye göre arama yapar"
/// 2. Neyi aradığını belirt — "ad veya kategoriye göre"
/// 3. Başka tool'larla karışmayacak şekilde yaz
/// </summary>
[Description("Ürün kataloğunda arama yapan plugin")]
public class ProductPlugin
{
    private readonly IProductService _productService;

    public ProductPlugin(IProductService productService)
    {
        _productService = productService;
    }

    [KernelFunction("SearchProducts")]
    [Description("Ürün kataloğunda anahtar kelimeye göre arama yapar. Ürün adı veya kategoriye göre arama yapılabilir. Ürünlerin ID, ad, fiyat ve stok bilgilerini döner. Arama basit anahtar kelime eşleşmesi yapar, bu yüzden kısa ve temel kelimeler kullanın.")]
    public string SearchProducts(
        [Description("Aranacak kısa anahtar kelime. Tek kelime tercih edin (örn: süt, peynir, temizlik, ekmek). Birden fazla kelime kullanmayın.")] string query)
    {
        var products = _productService.SearchProducts(query);
        var productList = products.ToList();

        if (!productList.Any())
            return $"'{query}' ile eşleşen ürün bulunamadı.";

        // LLM'e JSON döndürüyoruz — yapılandırılmış veriyi daha iyi anlar.
        // LLM bu veriyi okuyup "en ucuzunu bul", "ilk sıradakini ekle" gibi
        // reasoning yapabilir.
        var result = productList.Select(p => new
        {
            p.Id,
            p.Name,
            p.Category,
            Price = $"{p.Price:F2} TL",
            p.Stock,
            StockStatus = p.Stock > 0 ? "Stokta" : "Tükendi"
        });

        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }
}
