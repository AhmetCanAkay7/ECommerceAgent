using ECommerceAgent.Domain.Entities;

namespace ECommerceAgent.Infrastructure.Data;

/// <summary>
/// In-memory veri deposu. Gerçek projede bu katman veritabanı olur,
/// ama agentic yapıyı öğrenirken basit tutuyoruz.
/// 
/// Neden static? → Uygulama boyunca tek bir "mağaza" ve tek bir "sepet" olsun.
/// Agent her tool çağrısında aynı veriye erişsin.
/// </summary>
public static class MockStore
{
    /// <summary>
    /// Ürün kataloğu — bir marketin reyonlarını düşün.
    /// 5 kategori, toplamda 15 ürün.
    /// Her ürünün stok bilgisi var — agent stok kontrolü yapacak.
    /// </summary>
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

    /// <summary>
    /// Sepet state'i — Dictionary<productId, quantity>
    /// 
    /// Neden Dictionary? → productId ile hızlı erişim.
    /// Agent "2 süt ekle" dediğinde: Cart["sut-001"] = 2
    /// Agent "1 süt daha ekle" dediğinde: Cart["sut-001"] = 3 (mevcut üzerine ekle)
    /// </summary>
    public static Dictionary<string, int> Cart { get; } = new();
}
