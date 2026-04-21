using ECommerceAgent.Domain.Entities;

namespace ECommerceAgent.Application.Interfaces;

public interface IProductService
{
    IEnumerable<Product> SearchProducts(string query);
    Product? GetProductById(string productId);
}
