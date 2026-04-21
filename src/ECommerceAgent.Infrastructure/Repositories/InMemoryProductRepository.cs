using ECommerceAgent.Domain.Entities;
using ECommerceAgent.Domain.Interfaces;
using ECommerceAgent.Infrastructure.Data;

namespace ECommerceAgent.Infrastructure.Repositories;

public class InMemoryProductRepository : IProductRepository
{
    public IEnumerable<Product> Search(string query)
    {
        // TODO: Pair programming ile implemente edilecek
        // MockStore.Products üzerinden arama yapacak
        throw new NotImplementedException();
    }

    public Product? GetById(string productId)
    {
        // TODO: Pair programming ile implemente edilecek
        throw new NotImplementedException();
    }
}
