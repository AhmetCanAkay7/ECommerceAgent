using ECommerceAgent.Application.Common;
using ECommerceAgent.Application.Interfaces;
using ECommerceAgent.Application.Models;
using ECommerceAgent.Domain.Entities;
using ECommerceAgent.Domain.Interfaces;

namespace ECommerceAgent.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;

    public CustomerService(
        ICustomerRepository customerRepository,
        IOrderRepository orderRepository)
    {
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
    }

    public Result<CustomerProfileDto> GetCustomerProfile(string customerId)
    {
        var customer = _customerRepository.GetById(customerId);
        if (customer == null)
        {
            return Result<CustomerProfileDto>.Fail(
                $"'{customerId}' ID'li musteri bulunamadi. Lutfen gecerli bir musteri ID'si kullanin.",
                errorCode: "CUSTOMER_NOT_FOUND");
        }

        return Result<CustomerProfileDto>.Ok(
            MapProfile(customer),
            "Musteri profili getirildi.");
    }

    public Result<CustomerOrderHistoryDto> GetOrderHistory(string customerId)
    {
        var customer = _customerRepository.GetById(customerId);
        if (customer == null)
        {
            return Result<CustomerOrderHistoryDto>.Fail(
                $"'{customerId}' ID'li musteri bulunamadi. Siparis gecmisi getirilemedi.",
                errorCode: "CUSTOMER_NOT_FOUND");
        }

        var orders = _orderRepository
            .GetByCustomerId(customerId)
            .OrderByDescending(order => order.CreatedAt)
            .Select(MapOrderSummary)
            .ToList();

        var data = new CustomerOrderHistoryDto(
            MapProfile(customer),
            orders);

        var message = orders.Any()
            ? "Musteri siparis gecmisi getirildi."
            : $"'{customer.FullName}' icin siparis gecmisi bulunamadi.";

        return Result<CustomerOrderHistoryDto>.Ok(data, message);
    }

    private static CustomerProfileDto MapProfile(Customer customer)
    {
        return new CustomerProfileDto(
            customer.Id,
            customer.FullName,
            customer.Email,
            customer.PhoneNumber,
            customer.LoyaltyLevel);
    }

    private static CustomerOrderSummaryDto MapOrderSummary(Order order)
    {
        return new CustomerOrderSummaryDto(
            order.Id,
            order.Status.ToString(),
            order.CreatedAt,
            order.Items.Sum(item => item.Quantity),
            order.TotalAmount);
    }
}
