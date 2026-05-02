namespace ECommerceAgent.Application.Models;

public sealed record OrderStatusDto(
    string Id,
    string CustomerId,
    string Status,
    DateTime CreatedAt,
    DateTime? CancelledAt,
    string? CancellationReason);
