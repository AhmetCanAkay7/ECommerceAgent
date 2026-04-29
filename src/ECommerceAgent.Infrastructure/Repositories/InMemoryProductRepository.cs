using ECommerceAgent.Domain.Entities;
using ECommerceAgent.Domain.Interfaces;
using ECommerceAgent.Infrastructure.Data;

namespace ECommerceAgent.Infrastructure.Repositories;

/// <summary>
/// In-memory ürün repository'si. MockStore üzerinde arama yapar.
/// 
/// Agentic perspektiften neden önemli?
/// → Agent'ın SearchProducts tool'u bu repository'yi kullanır.
/// → Arama sonuçları LLM'e JSON olarak döner.
/// → LLM bu sonuçlardan reasoning yaparak kullanıcıya cevap verir.
///   Örnek: "En ucuz peyniri bul" → LLM fiyatları karşılaştırır.
/// </summary>
public class InMemoryProductRepository : IProductRepository
{
    public IEnumerable<Product> Search(string query)
    {
        // Sorguyu kelimelere böl — LLM "litrelik süt" gibi doğal dil ifadesi gönderebilir.
        // Her kelimeyi ayrı ayrı ara, herhangi biri eşleşirse ürünü dahil et.
        //
        // Örnek:
        //   query = "litrelik süt"
        //   → keywords = ["litrelik", "süt"]
        //   → "Günlük Süt 1L".Contains("süt") → ✅ eşleşir!
        //
        // Neden bu gerekli?
        // → LLM, tool'un arama mantığını bilmez. Kullanıcının sözünü
        //   aynen veya kendi yorumuyla parametre olarak gönderebilir.
        // → Arama mantığımızı LLM'in olası davranışlarına karşı dayanıklı
        //   (robust) hale getirmek bizim sorumluluğumuz.
        var keywords = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return MockStore.Products
            .Where(p =>
                keywords.Any(keyword =>
                    p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    p.Category.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public Product? GetById(string productId)
    {
        // Agent AddToCart çağırırken productId kullanır.
        // Eğer geçersiz/uydurma bir ID gelirse null döner → üst katman handle eder.
        return MockStore.Products.FirstOrDefault(p =>
            p.Id.Equals(productId, StringComparison.OrdinalIgnoreCase));
    }
}
