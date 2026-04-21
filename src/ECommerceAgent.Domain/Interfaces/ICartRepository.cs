using ECommerceAgent.Domain.Entities;

namespace ECommerceAgent.Domain.Interfaces;

public interface ICartRepository
{
    void AddItem(string productId, int quantity);
    bool RemoveItem(string productId);
    IEnumerable<CartItem> GetAllItems();
    void Clear();
    int GetItemQuantity(string productId);
}
