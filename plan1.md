# Implementasyon Planı — Adım Adım

## Phase 1 — Çalışan İlk Chat Loop

**1. `KernelConfiguration.CreateKernel()`**
- Dosya: `src/ECommerceAgent.ConsoleApp/Configuration/KernelConfiguration.cs`
- `Kernel.CreateBuilder()` ile builder oluştur
- `.AddOpenAIChatCompletion(modelId, apiKey)` ile OpenAI bağla
- `.Build()` ile kernel döndür
- Öğrenilen: Semantic Kernel'e LLM nasıl bağlanır
- Kaynak: https://learn.microsoft.com/en-us/semantic-kernel/get-started/quick-start-guide

> Bu adım tamamlandığında uygulama ayağa kalkar, tool olmadan düz sohbet çalışır.

---

## Phase 2 — Veri Katmanı

**2. `MockStore.Products`**
- Dosya: `src/ECommerceAgent.Infrastructure/Data/MockStore.cs`
- ~15 ürün ekle: süt, peynir, ekmek, deterjan vb.
- Her ürünlerde `Id`, `Name`, `Category`, `Price`, `Stock` dolu olmalı
- Öğrenilen: Agent'ın kullanacağı veri modeli

**3. `InMemoryProductRepository.Search()`**
- Dosya: `src/ECommerceAgent.Infrastructure/Repositories/InMemoryProductRepository.cs`
- `MockStore.Products` üzerinde `query` ile ad veya kategori filtrele

**4. `InMemoryProductRepository.GetById()`**
- Aynı dosya
- `MockStore.Products` içinden ID ile bul

**5. `InMemoryCartRepository.AddItem()`**
- Dosya: `src/ECommerceAgent.Infrastructure/Repositories/InMemoryCartRepository.cs`
- `MockStore.Cart` dictionary'sine ekle veya mevcut miktarı artır

**6. `InMemoryCartRepository.RemoveItem()`**
- Aynı dosya
- Dictionary'den sil, yoksa `false` döndür

**7. `InMemoryCartRepository.GetAllItems()`**
- Aynı dosya
- `MockStore.Cart` + `MockStore.Products` birleştirerek `CartItem` listesi oluştur

**8. `InMemoryCartRepository.GetItemQuantity()` ve `Clear()`**
- Aynı dosya

---

## Phase 3 — İş Kuralları (Application Katmanı)

**9. `ProductService.SearchProducts()`**
- Dosya: `src/ECommerceAgent.Application/Services/ProductService.cs`
- Repository'yi çağır, sonucu döndür

**10. `ProductService.GetProductById()`**
- Aynı dosya

**11. `CartService.AddToCart()`**
- Dosya: `src/ECommerceAgent.Application/Services/CartService.cs`
- Ürün var mı? → `ProductRepository.GetById()`
- Stok yeterli mi? → kısmi ekleme veya hata mesajı döndür
- Ekle → string özet döndür
- Öğrenilen: Tool'dan dönen hata mesajlarını LLM nasıl yorumlar

**12. `CartService.RemoveFromCart()`**
- Aynı dosya
- Sepette var mı? → yoksa anlamlı hata mesajı döndür

**13. `CartService.GetCart()`**
- Aynı dosya
- Tüm item'ları getir, toplam tutarı hesapla, string formatla

---

## Phase 4 — Semantic Kernel Plugin'leri

**14. `ProductPlugin.SearchProducts()`**
- Dosya: `src/ECommerceAgent.ConsoleApp/Plugins/ProductPlugin.cs`
- `_productService.SearchProducts(query)` çağır
- Sonucu JSON string'e dönüştür (LLM JSON okur)
- Öğrenilen: `[KernelFunction]` + `[Description]` — LLM tool'u nasıl "görür"
- Kaynak: https://learn.microsoft.com/en-us/semantic-kernel/concepts/plugins

**15. `CartPlugin.AddToCart()`, `RemoveFromCart()`, `GetCart()`**
- Dosya: `src/ECommerceAgent.ConsoleApp/Plugins/CartPlugin.cs`
- Servisleri çağır, sonucu string döndür
- Öğrenilen: Parametreli vs parametresiz tool farkı, tool description önemi

**16. Plugin'leri Kernel'e register et**
- `KernelConfiguration.CreateKernel()`'e geri dön
- `kernel.Plugins.AddFromObject(new ProductPlugin(...))` vb. ekle
- Öğrenilen: DI olmadan plugin registration, servis bağımlılıklarını nasıl geçersin

---

## Phase 5 — Observability

**17. `ToolLoggingFilter.OnAutoFunctionInvocationAsync()`**
- Dosya: `src/ECommerceAgent.ConsoleApp/Filters/ToolLoggingFilter.cs`
- Tool adı ve parametrelerini logla
- `await next(context)` ile akışı devam ettir
- Dönüş sonucunu ve süreyi logla
- Öğrenilen: `IAutoFunctionInvocationFilter` — her tool çağrısını intercept etmek
- Kaynak: https://learn.microsoft.com/en-us/semantic-kernel/concepts/filters

---

## Sıra Özeti

```
1     → Çalışan chat loop (tool yok)
2-8   → Veri katmanı (MockStore + Repository'ler)
9-13  → İş kuralları (Service'ler)
14-16 → LLM tool'ları görür, agentic döngü başlar  ← Kritik nokta
17    → Her tool çağrısını izle
```
