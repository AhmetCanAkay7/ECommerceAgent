using ECommerceAgent.Domain.Entities;
using ECommerceAgent.Domain.Interfaces;
using ECommerceAgent.Infrastructure.Data;

namespace ECommerceAgent.Infrastructure.Repositories;

public class InMemoryCustomerRepository : ICustomerRepository
{
    public Customer? GetById(string customerId)
    {
        return MockStore.Customers.FirstOrDefault(customer =>
            customer.Id.Equals(customerId, StringComparison.OrdinalIgnoreCase));
    }

    public Customer? FindByName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return null;

        return MockStore.Customers.FirstOrDefault(customer =>
            customer.FullName.Contains(fullName, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<Customer> GetAll()
    {
        return MockStore.Customers.ToList();
    }
}
