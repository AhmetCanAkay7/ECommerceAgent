using ECommerceAgent.Domain.Entities;

namespace ECommerceAgent.Infrastructure.Data;

public static class MockStore
{
    public static List<Product> Products { get; } = new()
    {
        // Cart and product-search scenarios
        Product("sut-001", "Günlük Süt 1L", "Süt Ürünleri", 45.90m, 20),
        Product("sut-002", "Laktozsuz Süt 1L", "Süt Ürünleri", 52.90m, 8),
        Product("gida-001", "Tam Buğday Ekmek", "Temel Gıda", 22.50m, 30),
        Product("gida-002", "Makarna 500g", "Temel Gıda", 34.90m, 25),

        // Edge-case scenarios
        Product("icecek-001", "Portakal Suyu 1L", "İçecek", 44.90m, 3),
        Product("atis-001", "Bisküvi 200g", "Atıştırmalık", 24.90m, 0),

        // Order-history variety
        Product("temiz-001", "Bulaşık Deterjanı 1L", "Temizlik", 79.90m, 12),
        Product("temiz-002", "Yüzey Temizleyici 750ml", "Temizlik", 59.90m, 15)
    };

    public static List<Customer> Customers { get; } = new()
    {
        Customer(
            id: "cust-001",
            fullName: "Ahmet Yilmaz",
            email: "ahmet.yilmaz@example.com",
            phoneNumber: "+90 555 100 10 01",
            loyaltyLevel: "Gold"),

        Customer(
            id: "cust-002",
            fullName: "Ayse Demir",
            email: "ayse.demir@example.com",
            phoneNumber: "+90 555 100 10 02",
            loyaltyLevel: "Silver"),

        Customer(
            id: "cust-003",
            fullName: "Mehmet Kaya",
            email: "mehmet.kaya@example.com",
            phoneNumber: "+90 555 100 10 03",
            loyaltyLevel: "Standard")
    };

    public static List<Order> Orders { get; } = new()
    {
        // Delivered order: should not be auto-cancellable.
        Order(
            id: "ord-1001",
            customerId: "cust-001",
            status: OrderStatus.Delivered,
            createdAtUtc: new DateTime(2026, 5, 1, 10, 30, 0, DateTimeKind.Utc),
            items:
            [
                OrderItem("sut-001", 2),
                OrderItem("gida-001", 1)
            ]),

        // Preparing order: safe cancellation path for Section 2/3 tests.
        Order(
            id: "ord-1002",
            customerId: "cust-001",
            status: OrderStatus.Preparing,
            createdAtUtc: new DateTime(2026, 5, 8, 18, 15, 0, DateTimeKind.Utc),
            items:
            [
                OrderItem("sut-002", 1),
                OrderItem("gida-002", 3),
                OrderItem("icecek-001", 1)
            ]),

        // Out-for-delivery order: should require escalation.
        Order(
            id: "ord-1003",
            customerId: "cust-002",
            status: OrderStatus.OutForDelivery,
            createdAtUtc: new DateTime(2026, 5, 9, 9, 45, 0, DateTimeKind.Utc),
            items:
            [
                OrderItem("temiz-001", 1),
                OrderItem("temiz-002", 2)
            ]),

        // Already cancelled order: idempotency and error-code test.
        Order(
            id: "ord-1004",
            customerId: "cust-003",
            status: OrderStatus.Cancelled,
            createdAtUtc: new DateTime(2026, 5, 4, 16, 0, 0, DateTimeKind.Utc),
            items:
            [
                OrderItem("atis-001", 4)
            ],
            cancelledAtUtc: new DateTime(2026, 5, 4, 16, 20, 0, DateTimeKind.Utc),
            cancellationReason: "Musteri talebi")
    };

    // The active console session cart. It is intentionally not customer-specific.
    public static Dictionary<string, int> Cart { get; } = new();

    private static Product Product(
        string id,
        string name,
        string category,
        decimal price,
        int stock)
    {
        return new Product
        {
            Id = id,
            Name = name,
            Category = category,
            Price = price,
            Stock = stock
        };
    }

    private static Customer Customer(
        string id,
        string fullName,
        string email,
        string phoneNumber,
        string loyaltyLevel)
    {
        return new Customer
        {
            Id = id,
            FullName = fullName,
            Email = email,
            PhoneNumber = phoneNumber,
            LoyaltyLevel = loyaltyLevel
        };
    }

    private static Order Order(
        string id,
        string customerId,
        OrderStatus status,
        DateTime createdAtUtc,
        IEnumerable<OrderItem> items,
        DateTime? cancelledAtUtc = null,
        string? cancellationReason = null)
    {
        return new Order
        {
            Id = id,
            CustomerId = customerId,
            Status = status,
            CreatedAt = createdAtUtc,
            Items = items.ToList(),
            CancelledAt = cancelledAtUtc,
            CancellationReason = cancellationReason
        };
    }

    private static OrderItem OrderItem(string productId, int quantity)
    {
        var product = Products.First(p => p.Id == productId);

        return new OrderItem
        {
            ProductId = product.Id,
            ProductName = product.Name,
            UnitPrice = product.Price,
            Quantity = quantity
        };
    }
}
