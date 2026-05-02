using ECommerceAgent.Application.Common;
using ECommerceAgent.Application.Interfaces;
using ECommerceAgent.Application.Models;
using ECommerceAgent.Domain.Entities;
using ECommerceAgent.Domain.Interfaces;

namespace ECommerceAgent.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public Result<OrderStatusDto> GetOrderStatus(string orderId)
    {
        var order = _orderRepository.GetById(orderId);
        if (order == null)
        {
            return Result<OrderStatusDto>.Fail(
                $"'{orderId}' ID'li siparis bulunamadi. Lutfen gecerli bir siparis ID'si ile tekrar deneyin.",
                errorCode: "ORDER_NOT_FOUND");
        }

        return Result<OrderStatusDto>.Ok(
            MapStatus(order),
            "Siparis durumu getirildi.");
    }

    public Result<OrderDetailsDto> GetOrderDetails(string orderId)
    {
        var order = _orderRepository.GetById(orderId);
        if (order == null)
        {
            return Result<OrderDetailsDto>.Fail(
                $"'{orderId}' ID'li siparis bulunamadi. Siparis detaylarini getirmek icin once dogru ID'yi kullanin.",
                errorCode: "ORDER_NOT_FOUND");
        }

        return Result<OrderDetailsDto>.Ok(
            MapDetails(order),
            "Siparis detaylari getirildi.");
    }

    public Result<OrderCancellationDto> CancelOrder(string orderId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result<OrderCancellationDto>.Fail(
                "Siparis iptali icin iptal nedeni gereklidir.",
                errorCode: "CANCELLATION_REASON_REQUIRED");
        }

        var order = _orderRepository.GetById(orderId);
        if (order == null)
        {
            return Result<OrderCancellationDto>.Fail(
                $"'{orderId}' ID'li siparis bulunamadi. Iptal islemi yapilmadi.",
                errorCode: "ORDER_NOT_FOUND");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            return Result<OrderCancellationDto>.Fail(
                $"'{orderId}' ID'li siparis zaten iptal edilmis. Onceki neden: {order.CancellationReason ?? "belirtilmemis"}.",
                errorCode: "ORDER_ALREADY_CANCELLED");
        }

        if (order.Status == OrderStatus.Delivered)
        {
            return Result<OrderCancellationDto>.Fail(
                $"'{orderId}' ID'li siparis teslim edildigi icin otomatik iptal edilemez. Musteri temsilcisine eskalasyon gerekir.",
                errorCode: "ORDER_ALREADY_DELIVERED",
                requiresEscalation: true);
        }

        if (order.Status == OrderStatus.OutForDelivery)
        {
            return Result<OrderCancellationDto>.Fail(
                $"'{orderId}' ID'li siparis teslimata ciktigi icin otomatik iptal edilemez. Musteri temsilcisi onayi gerekir.",
                errorCode: "ORDER_OUT_FOR_DELIVERY",
                requiresEscalation: true);
        }

        var previousStatus = order.Status.ToString();
        var cancelled = _orderRepository.Cancel(orderId, reason);
        if (!cancelled)
        {
            return Result<OrderCancellationDto>.Fail(
                $"'{orderId}' ID'li siparis iptal edilemedi. Lutfen daha sonra tekrar deneyin.",
                errorCode: "ORDER_CANCEL_FAILED");
        }

        var updatedOrder = _orderRepository.GetById(orderId)!;
        var data = new OrderCancellationDto(
            updatedOrder.Id,
            previousStatus,
            updatedOrder.Status.ToString(),
            updatedOrder.CancelledAt,
            updatedOrder.CancellationReason ?? reason);

        return Result<OrderCancellationDto>.Ok(
            data,
            $"'{orderId}' ID'li siparis iptal edildi.");
    }

    private static OrderStatusDto MapStatus(Order order)
    {
        return new OrderStatusDto(
            order.Id,
            order.CustomerId,
            order.Status.ToString(),
            order.CreatedAt,
            order.CancelledAt,
            order.CancellationReason);
    }

    private static OrderDetailsDto MapDetails(Order order)
    {
        return new OrderDetailsDto(
            order.Id,
            order.CustomerId,
            order.Status.ToString(),
            order.CreatedAt,
            order.Items.Select(MapItem).ToList(),
            order.TotalAmount,
            order.CancelledAt,
            order.CancellationReason);
    }

    private static OrderItemDto MapItem(OrderItem item)
    {
        return new OrderItemDto(
            item.ProductId,
            item.ProductName,
            item.UnitPrice,
            item.Quantity);
    }
}
