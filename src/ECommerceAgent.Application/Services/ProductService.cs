using ECommerceAgent.Application.Interfaces;
using ECommerceAgent.Domain.Entities;
using ECommerceAgent.Domain.Interfaces;

namespace ECommerceAgent.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public IEnumerable<Product> SearchProducts(string query)
    {
        // TODO: Pair programming ile implemente edilecek
        throw new NotImplementedException();
    }

    public Product? GetProductById(string productId)
    {
        // TODO: Pair programming ile implemente edilecek
        throw new NotImplementedException();
    }
}
