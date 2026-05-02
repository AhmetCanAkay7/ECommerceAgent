namespace ECommerceAgent.Application.Models;

public sealed record OrderDetailsDto(
    string Id,
    string CustomerId,
    string Status,
    DateTime CreatedAt,
    IReadOnlyList<OrderItemDto> Items,
    decimal TotalAmount,
    DateTime? CancelledAt,
    string? CancellationReason);
