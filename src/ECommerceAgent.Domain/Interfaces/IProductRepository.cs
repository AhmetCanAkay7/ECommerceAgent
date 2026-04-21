using ECommerceAgent.Domain.Entities;

namespace ECommerceAgent.Domain.Interfaces;

public interface IProductRepository
{
    IEnumerable<Product> Search(string query);
    Product? GetById(string productId);
}
