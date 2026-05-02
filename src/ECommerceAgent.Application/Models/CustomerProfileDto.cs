namespace ECommerceAgent.Application.Models;

public sealed record CustomerProfileDto(
    string Id,
    string FullName,
    string Email,
    string PhoneNumber,
    string LoyaltyLevel);
