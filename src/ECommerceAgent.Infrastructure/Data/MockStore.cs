using ECommerceAgent.Domain.Entities;

namespace ECommerceAgent.Infrastructure.Data;

public static class MockStore
{
    public static List<Product> Products { get; } = new()
    {
        // 🥛 Süt Ürünleri
        new Product { Id = "sut-001",    Name = "Günlük Süt 1L",         Category = "Süt Ürünleri",  Price = 45.90m,  Stock = 20 },
        new Product { Id = "sut-002",    Name = "Kaşar Peyniri 500g",    Category = "Süt Ürünleri",  Price = 129.90m, Stock = 15 },
        new Product { Id = "sut-003",    Name = "Beyaz Peynir 400g",     Category = "Süt Ürünleri",  Price = 89.90m,  Stock = 10 },

        // 🍞 Temel Gıda
        new Product { Id = "gida-001",   Name = "Tam Buğday Ekmek",      Category = "Temel Gıda",    Price = 22.50m,  Stock = 30 },
        new Product { Id = "gida-002",   Name = "Makarna 500g",          Category = "Temel Gıda",    Price = 34.90m,  Stock = 25 },
        new Product { Id = "gida-003",   Name = "Pirinç 1kg",           Category = "Temel Gıda",    Price = 64.90m,  Stock = 18 },

        // 🧹 Temizlik
        new Product { Id = "temiz-001",  Name = "Bulaşık Deterjanı 1L",  Category = "Temizlik",      Price = 79.90m,  Stock = 12 },
        new Product { Id = "temiz-002",  Name = "Çamaşır Deterjanı 2kg", Category = "Temizlik",      Price = 149.90m, Stock = 8 },
        new Product { Id = "temiz-003",  Name = "Yüzey Temizleyici 750ml",Category = "Temizlik",     Price = 59.90m,  Stock = 15 },

        // 🥤 İçecek
        new Product { Id = "icecek-001", Name = "Ayran 1L",              Category = "İçecek",        Price = 29.90m,  Stock = 25 },
        new Product { Id = "icecek-002", Name = "Maden Suyu 6'lı Paket", Category = "İçecek",        Price = 54.90m,  Stock = 20 },
        new Product { Id = "icecek-003", Name = "Portakal Suyu 1L",      Category = "İçecek",        Price = 44.90m,  Stock = 3 },  // ⚠️ Düşük stok — test için

        // 🍫 Atıştırmalık
        new Product { Id = "atis-001",   Name = "Çikolata 80g",          Category = "Atıştırmalık",  Price = 39.90m,  Stock = 30 },
        new Product { Id = "atis-002",   Name = "Bisküvi 200g",          Category = "Atıştırmalık",  Price = 24.90m,  Stock = 0 },  // ⚠️ Stokta yok — test için!
        new Product { Id = "atis-003",   Name = "Kuruyemiş 250g",        Category = "Atıştırmalık",  Price = 74.90m,  Stock = 10 },
    };

    public static List<Customer> Customers { get; } = new()
    {
        new Customer
        {
            Id = "cust-001",
            FullName = "Ahmet Yilmaz",
            Email = "ahmet.yilmaz@example.com",
            PhoneNumber = "+90 555 100 10 01",
            LoyaltyLevel = "Gold"
        },
        new Customer
        {
            Id = "cust-002",
            FullName = "Ayse Demir",
            Email = "ayse.demir@example.com",
            PhoneNumber = "+90 555 100 10 02",
            LoyaltyLevel = "Silver"
        },
        new Customer
        {
            Id = "cust-003",
            FullName = "Mehmet Kaya",
            Email = "mehmet.kaya@example.com",
            PhoneNumber = "+90 555 100 10 03",
            LoyaltyLevel = "Standard"
        }
    };

    public static List<Order> Orders { get; } = new()
    {
        new Order
        {
            Id = "ord-1001",
            CustomerId = "cust-001",
            Status = OrderStatus.Delivered,
            CreatedAt = new DateTime(2026, 4, 25, 11, 30, 0, DateTimeKind.Utc),
            Items = new List<OrderItem>
            {
                CreateOrderItem("sut-001", 2),
                CreateOrderItem("gida-001", 1),
                CreateOrderItem("icecek-001", 1)
            }
        },
        new Order
        {
            Id = "ord-1002",
            CustomerId = "cust-001",
            Status = OrderStatus.Preparing,
            CreatedAt = new DateTime(2026, 5, 1, 18, 15, 0, DateTimeKind.Utc),
            Items = new List<OrderItem>
            {
                CreateOrderItem("sut-002", 1),
                CreateOrderItem("gida-002", 3),
                CreateOrderItem("atis-003", 1)
            }
        },
        new Order
        {
            Id = "ord-1003",
            CustomerId = "cust-002",
            Status = OrderStatus.OutForDelivery,
            CreatedAt = new DateTime(2026, 5, 2, 9, 45, 0, DateTimeKind.Utc),
            Items = new List<OrderItem>
            {
                CreateOrderItem("temiz-001", 1),
                CreateOrderItem("temiz-003", 2)
            }
        },
        new Order
        {
            Id = "ord-1004",
            CustomerId = "cust-003",
            Status = OrderStatus.Cancelled,
            CreatedAt = new DateTime(2026, 4, 28, 16, 0, 0, DateTimeKind.Utc),
            CancelledAt = new DateTime(2026, 4, 28, 16, 20, 0, DateTimeKind.Utc),
            CancellationReason = "Musteri talebi",
            Items = new List<OrderItem>
            {
                CreateOrderItem("icecek-003", 2),
                CreateOrderItem("atis-001", 4)
            }
        }
    };

    public static Dictionary<string, int> Cart { get; } = new();

    private static OrderItem CreateOrderItem(string productId, int quantity)
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
