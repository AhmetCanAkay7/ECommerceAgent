# Agentic E-Commerce Sepet Asistanı — Learning Project Plan

## Bağlam
- Migros Ticaret, Software Developer, Agentic AI öğrenme projesi
- Kişisel OpenAI API key (Azure OpenAI değil)
- Semantic Kernel + .NET 8 + Console App
- Basit e-ticaret sepet domain'i — ürün arama, sepete ekleme/çıkarma, sepet özeti

---

## Senaryo — Kullanıcı Ne Yapar?

```
"Süt var mı?"                          → Agent: SearchProducts çağırır, sonuçları sunar
"İlk sıradakinden 2 tane ekle"         → Agent: Context'ten productId çözer, AddToCart çağırır
"Bir de peynir bak"                    → Agent: SearchProducts çağırır
"En ucuzunu sepete at"                 → Agent: Reasoning — en düşük fiyatlıyı bulur, AddToCart çağırır
"Sepetimde ne var?"                    → Agent: GetCart çağırır, özet sunar
"Sütü çıkar"                          → Agent: RemoveFromCart çağırır
"Toplam ne kadar?"                     → Agent: GetCart çağırır, tutar hesaplar
```

**Öğretici olan kısım:** Agent hardcoded bir akış izlemiyor — her mesajda **hangi tool'u çağıracağına (veya hiçbirini çağırmayacağına) LLM reasoning ile karar veriyor.**

---

## Tool Tanımları (4 adet)

| Tool | Input | Output | Öğrettiği |
|---|---|---|---|
| `SearchProducts` | `query: string` | products[] (id, name, price, stock) | Temel tool calling |
| `AddToCart` | `productId: string`, `quantity: int` | cart summary (items, total) | Parametreli tool + iş kuralı (stok kontrolü) |
| `RemoveFromCart` | `productId: string` | cart summary | Tool seçimi (Add vs Remove ayrımı) |
| `GetCart` | *(parametresiz)* | cart items[], totalPrice | Parametresiz tool, context-based reasoning |

**İş kuralları (agent'ın öğrenmesi gereken):**
- Stokta yoksa ekleme → tool hata döner, agent kullanıcıya açıklar
- Stoktan fazla isteme → tool kısmi ekleme yapar, agent bilgilendirir
- Sepette olmayan ürünü çıkarma → tool hata döner, agent graceful handle eder

---

## Mimari

```
┌──────────────────────────────────────────────┐
│              Console App (Chat Loop)          │
│         Kullanıcı doğal dilde konuşur         │
└─────────────────────┬────────────────────────┘
                      │
                      ▼
┌──────────────────────────────────────────────┐
│            Orchestrator Agent                 │
│        (Semantic Kernel + OpenAI GPT-4o)      │
│                                               │
│  System Prompt: "Alışveriş asistanısın"       │
│  ChatHistory: Konuşma bağlamı (state)         │
│  FunctionChoiceBehavior.Auto()                │
│                                               │
│  LLM reasoning örnekleri:                     │
│  - "en ucuzunu ekle" → fiyat karşılaştır      │
│  - "ilk sıradaki" → önceki arama sonucundan   │
│  - "sütü çıkar" → sepetten ürün adıyla bul    │
└───────┬──────────┬──────────┬──────────┬─────┘
        │          │          │          │
        ▼          ▼          ▼          ▼
   Search      AddToCart  RemoveFrom   GetCart
   Products    (Plugin)   Cart         (Plugin)
   (Plugin)               (Plugin)
        │          │          │          │
        ▼          ▼          ▼          ▼
┌──────────────────────────────────────────────┐
│          In-Memory Data Store                 │
│  - Ürün kataloğu (~15 ürün, kategorili)       │
│  - Sepet (Dictionary<productId, quantity>)     │
└──────────────────────────────────────────────┘
```

---

## Tech Stack
- **.NET 8** / C#
- **Microsoft.SemanticKernel** NuGet
- **OpenAI GPT-4o** (kişisel API key)
- Console App
- In-memory mock data

## Proje Yapısı
```
ECommerceAgent/
├── Program.cs                  # Kernel setup + chat loop
├── Plugins/
│   ├── ProductPlugin.cs        # SearchProducts
│   └── CartPlugin.cs           # AddToCart, RemoveFromCart, GetCart
├── Models/
│   ├── Product.cs
│   └── CartItem.cs
├── Data/
│   └── MockStore.cs            # Ürün kataloğu + sepet state
├── Filters/
│   └── ToolLoggingFilter.cs    # IAutoFunctionInvocationFilter
├── appsettings.json            # OpenAI API key
└── ECommerceAgent.csproj
```

---

## Adım Adım Uygulama

### Phase 1: Temel Kurulum
1. `dotnet new console -n ECommerceAgent` + `dotnet add package Microsoft.SemanticKernel`
2. OpenAI bağlantısı: `builder.AddOpenAIChatCompletion("gpt-4o", apiKey)`
3. Chat loop: `while(true)` — kullanıcı yazar, LLM cevaplar (tool yok henüz)
4. System prompt: *"Sen bir alışveriş asistanısın. Kullanıcıya ürün aramada, sepet yönetiminde yardımcı olursun. Sadece bu konularda yardım edersin."*

### Phase 2: Mock Data + Plugin'ler *(depends on Phase 1)*
5. `MockStore`: ~15 ürün (süt, peynir, ekmek, deterjan vb. — kategorili, fiyatlı, stoklu) + in-memory sepet (`Dictionary<string, int>`)
6. `ProductPlugin.SearchProducts` → `[KernelFunction][Description("Ürün kataloğunda arama yapar")]`
7. `CartPlugin.AddToCart`, `RemoveFromCart`, `GetCart` → her biri `[KernelFunction]` ile
8. Register: `kernel.Plugins.AddFromType<ProductPlugin>()`, `kernel.Plugins.AddFromType<CartPlugin>()`

### Phase 3: Agentic Döngü *(depends on Phase 2)*
9. `FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()` aktif et
10. Test senaryoları:
    - **Tek tool**: "Sepetimde ne var?" → `GetCart`
    - **Arama + ekleme zinciri**: "Süt var mı? Varsa 2 tane ekle" → `SearchProducts` → `AddToCart`
    - **Context reasoning**: "En ucuz peyniri sepete at" → `SearchProducts` → LLM fiyat karşılaştırır → `AddToCart`
    - **Edge case**: "Stokta olmayan üründen 100 tane ekle" → hata yönetimi
11. Multi-turn test: "Süt ara" → "İlkini ekle" → "Sütü çıkar" → "Toplam kaç TL?"

### Phase 4: Observability & Kontrol *(depends on Phase 3)*
12. `ToolLoggingFilter` (`IAutoFunctionInvocationFilter`) → tool adı, parametreler, sonuç, süre logla
13. Off-topic test: "Hava durumu?" → agent reddeder
14. Stok aşımı, negatif miktar, boş sepetten silme → graceful handling

### Phase 5: Genişletme *(opsiyonel)*
15. Yeni tool: `Checkout` → sepeti onayla, sipariş oluştur (human-in-the-loop onay)
16. Yeni tool: `ApplyDiscount` → kupon kodu uygula
17. Streaming response
18. ASP.NET Minimal API + basit chat UI

---

## IBM Projesiyle Paralel Öğrenmeler

| IBM Projesi Dersi | Bu Projede Karşılığı |
|---|---|
| Yanlış tool seçimi | `AddToCart` vs `RemoveFromCart` — benzer description'lar, agent doğru seçiyor mu? |
| Parametre halüsinasyonu | Var olmayan productId → agent uyduruyor mu yoksa aramadan mı buluyor? |
| State yönetimi | ChatHistory: "süt ara" → "ilkini ekle" — önceki sonucu hatırlıyor mu? |
| İş kuralı uygulama | Stok kontrolü — tool hata dönünce agent nasıl tepki veriyor? |
| Observability | `IAutoFunctionInvocationFilter` ile her tool çağrısını izle |
| Human-in-the-loop | Phase 5'te `Checkout` öncesi onay mekanizması |

---

## Genişletilebilirlik
- Mock data → gerçek API (aynı plugin interface korunur)
- Yeni tool'lar eklemek = yeni `[KernelFunction]` yazmak, Kernel'e register etmek — mimari değişmez
- Müşteri 360 use-case'ine evrilme: `CreateComplaint`, `SearchComplaints` tool'ları aynı pattern'la eklenir
- Multi-agent (gelecek): Product Agent + Cart Agent + Checkout Agent

---

## Verification
1. "Süt ara → en ucuzunu ekle → sepeti göster → sütü çıkar" tam akışını test et
2. Loglardan doğru tool seçimini doğrula
3. Olmayan ürün ID → graceful error
4. Multi-turn context korunma testi
5. Off-topic → agent sınırları koruyor mu?
6. Stokta 3 olan üründen 10 isteme → agent ne yapıyor?

---

## Kararlar
- Domain olarak e-ticaret sepet seçildi (Müşteri 360 çok kapsamlı bulundu)
- 4 tool yeterli — tüm agentic konseptleri (tool selection, chaining, reasoning, state) kapsıyor
- OpenAI doğrudan (kişisel key) → ileride Azure OpenAI'ya geçiş tek satır
