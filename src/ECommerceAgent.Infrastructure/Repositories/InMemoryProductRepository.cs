using ECommerceAgent.Domain.Entities;
using ECommerceAgent.Domain.Interfaces;
using ECommerceAgent.Infrastructure.Data;

namespace ECommerceAgent.Infrastructure.Repositories;

public class InMemoryProductRepository : IProductRepository
{
    public IEnumerable<Product> Search(string query)
    {
        var keywords = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return MockStore.Products
            .Where(p =>
                keywords.Any(keyword =>
                    p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    p.Category.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public Product? GetById(string productId)
    {
        return MockStore.Products.FirstOrDefault(p =>
            p.Id.Equals(productId, StringComparison.OrdinalIgnoreCase));
    }
}
