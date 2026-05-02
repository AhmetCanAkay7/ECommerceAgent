using ECommerceAgent.Application.Common;
using ECommerceAgent.Application.Models;

namespace ECommerceAgent.Application.Interfaces;

public interface IOrderService
{
    Result<OrderStatusDto> GetOrderStatus(string orderId);
    Result<OrderDetailsDto> GetOrderDetails(string orderId);
    Result<OrderCancellationDto> CancelOrder(string orderId, string reason);
}
