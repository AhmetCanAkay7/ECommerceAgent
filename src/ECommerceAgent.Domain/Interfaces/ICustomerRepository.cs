using ECommerceAgent.Domain.Entities;

namespace ECommerceAgent.Domain.Interfaces;

public interface ICustomerRepository
{
    Customer? GetById(string customerId);
    Customer? FindByName(string fullName);
    IEnumerable<Customer> GetAll();
}
