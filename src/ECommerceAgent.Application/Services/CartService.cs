using ECommerceAgent.Application.Interfaces;
using ECommerceAgent.Domain.Entities;
using ECommerceAgent.Domain.Interfaces;

namespace ECommerceAgent.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public CartService(ICartRepository cartRepository, IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public string AddToCart(string productId, int quantity)
    {
        // TODO: Pair programming ile implemente edilecek
        // Stok kontrolü, iş kuralları burada olacak
        throw new NotImplementedException();
    }

    public string RemoveFromCart(string productId)
    {
        // TODO: Pair programming ile implemente edilecek
        throw new NotImplementedException();
    }

    public string GetCart()
    {
        // TODO: Pair programming ile implemente edilecek
        throw new NotImplementedException();
    }
}
