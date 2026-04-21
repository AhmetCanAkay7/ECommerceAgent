using ECommerceAgent.Domain.Entities;
using ECommerceAgent.Domain.Interfaces;
using ECommerceAgent.Infrastructure.Data;

namespace ECommerceAgent.Infrastructure.Repositories;

public class InMemoryCartRepository : ICartRepository
{
    public void AddItem(string productId, int quantity)
    {
        // TODO: Pair programming ile implemente edilecek
        throw new NotImplementedException();
    }

    public bool RemoveItem(string productId)
    {
        // TODO: Pair programming ile implemente edilecek
        throw new NotImplementedException();
    }

    public IEnumerable<CartItem> GetAllItems()
    {
        // TODO: Pair programming ile implemente edilecek
        // MockStore.Cart + MockStore.Products birleştirilerek CartItem listesi dönecek
        throw new NotImplementedException();
    }

    public void Clear()
    {
        // TODO: Pair programming ile implemente edilecek
        throw new NotImplementedException();
    }

    public int GetItemQuantity(string productId)
    {
        // TODO: Pair programming ile implemente edilecek
        throw new NotImplementedException();
    }
}
