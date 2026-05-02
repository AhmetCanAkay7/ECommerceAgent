namespace ECommerceAgent.Domain.Entities;
public class Customer
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string LoyaltyLevel { get; set; } = string.Empty;

}
