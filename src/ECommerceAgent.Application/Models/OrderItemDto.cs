namespace ECommerceAgent.Application.Models;

public sealed record OrderItemDto(
    string ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity)
{
    public decimal TotalPrice => UnitPrice * Quantity;
}
