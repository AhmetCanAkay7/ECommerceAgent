using ECommerceAgent.Domain.Entities;

namespace ECommerceAgent.Infrastructure.Data;

public static class MockStore
{
    // TODO: Pair programming ile ~15 ürünlük katalog doldurulacak
    // Kategoriler: Süt Ürünleri, Temel Gıda, Temizlik, İçecek, Atıştırmalık
    public static List<Product> Products { get; } = new();

    // Sepet state — Dictionary<productId, quantity>
    public static Dictionary<string, int> Cart { get; } = new();
}
