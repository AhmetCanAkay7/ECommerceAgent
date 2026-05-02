using ECommerceAgent.Application.Common;
using ECommerceAgent.Application.Models;

namespace ECommerceAgent.Application.Interfaces;

public interface ICustomerService
{
    Result<CustomerProfileDto> GetCustomerProfile(string customerId);
    Result<CustomerOrderHistoryDto> GetOrderHistory(string customerId);
}
