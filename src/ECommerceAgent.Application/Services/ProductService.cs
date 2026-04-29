using ECommerceAgent.Application.Interfaces;
using ECommerceAgent.Domain.Entities;
using ECommerceAgent.Domain.Interfaces;

namespace ECommerceAgent.Application.Services;

/// <summary>
/// Ürün iş mantığı servisi.
/// 
/// Şu an basit bir delegation (delege etme) yapıyor — repository'ye iletip sonucu dönüyor.
/// Gerçek projede burada cache, validation, business rule'lar olabilir.
/// 
/// Neden doğrudan repository'yi Plugin'den çağırmıyoruz?
/// → Separation of Concerns: Plugin sadece "Semantic Kernel ile nasıl konuşulur" bilir.
/// → Service "iş kuralları" bilir.
/// → Repository "veri erişimi" bilir.
/// → Her katmanın tek sorumluluğu var (Single Responsibility).
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public IEnumerable<Product> SearchProducts(string query)
    {
        return _productRepository.Search(query);
    }

    public Product? GetProductById(string productId)
    {
        return _productRepository.GetById(productId);
    }
}
