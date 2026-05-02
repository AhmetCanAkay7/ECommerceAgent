using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;
using ECommerceAgent.Application.Interfaces;

namespace ECommerceAgent.ConsoleApp.Plugins;

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
