using ECommerceAgent.Domain.Entities;
using ECommerceAgent.Domain.Interfaces;
using ECommerceAgent.Infrastructure.Data;

namespace ECommerceAgent.Infrastructure.Repositories;

public class InMemoryOrderRepository : IOrderRepository
{
    public Order? GetById(string orderId)
    {
        return MockStore.Orders.FirstOrDefault(order =>
            order.Id.Equals(orderId, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<Order> GetByCustomerId(string customerId)
    {
        return MockStore.Orders
            .Where(order => order.CustomerId.Equals(customerId, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public bool Cancel(string orderId, string reason)
    {
        var order = GetById(orderId);
        if (order == null)
            return false;

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        order.CancellationReason = reason;

        return true;
    }
}
