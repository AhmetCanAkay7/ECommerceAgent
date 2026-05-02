namespace ECommerceAgent.Application.Models;

public sealed record OrderCancellationDto(
    string Id,
    string PreviousStatus,
    string CurrentStatus,
    DateTime? CancelledAt,
    string CancellationReason);
