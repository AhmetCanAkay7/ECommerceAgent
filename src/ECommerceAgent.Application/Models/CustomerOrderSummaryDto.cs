namespace ECommerceAgent.Application.Models;

public sealed record CustomerOrderSummaryDto(
    string Id,
    string Status,
    DateTime CreatedAt,
    int ItemCount,
    decimal TotalAmount);
