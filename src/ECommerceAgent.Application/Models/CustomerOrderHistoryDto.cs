namespace ECommerceAgent.Application.Models;

public sealed record CustomerOrderHistoryDto(
    CustomerProfileDto Customer,
    IReadOnlyList<CustomerOrderSummaryDto> Orders);
