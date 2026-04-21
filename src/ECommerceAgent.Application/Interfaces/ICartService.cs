using ECommerceAgent.Domain.Entities;

namespace ECommerceAgent.Application.Interfaces;

public interface ICartService
{
    string AddToCart(string productId, int quantity);
    string RemoveFromCart(string productId);
    string GetCart();
}
