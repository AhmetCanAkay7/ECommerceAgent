using System.ComponentModel;
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
    [Description("Ürün kataloğunda anahtar kelimeye göre arama yapar. Ürün adı veya kategoriye göre arama yapılabilir.")]
    public string SearchProducts(
        [Description("Aranacak ürün adı veya kategori (örn: süt, peynir, temizlik)")] string query)
    {
        // TODO: Pair programming ile implemente edilecek
        // _productService.SearchProducts çağrılacak, sonuç JSON formatında dönecek
        throw new NotImplementedException();
    }
}
