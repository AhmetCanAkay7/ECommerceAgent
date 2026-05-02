using ECommerceAgent.Domain.Entities;

namespace ECommerceAgent.Domain.Interfaces;

public interface IOrderRepository
{
    Order? GetById(string orderId);
    IEnumerable<Order> GetByCustomerId(string customerId);
    bool Cancel(string orderId, string reason);
}
