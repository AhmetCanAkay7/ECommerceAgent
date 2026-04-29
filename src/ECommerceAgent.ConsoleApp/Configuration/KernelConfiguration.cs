using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using ECommerceAgent.Application.Interfaces;
using ECommerceAgent.Application.Services;
using ECommerceAgent.Domain.Interfaces;
using ECommerceAgent.Infrastructure.Repositories;
using ECommerceAgent.ConsoleApp.Plugins;
using ECommerceAgent.ConsoleApp.Filters;
using ECommerceAgent.ConsoleApp.Orchestration;

namespace ECommerceAgent.ConsoleApp.Configuration;

/// <summary>
/// DI (Dependency Injection) yapılandırması — tüm bağımlılıkları burada kayıt ediyoruz.
/// 
/// 🎓 Manuel DI'dan Proper DI'ya Geçiş:
/// 
/// ÖNCE (Manuel):
///   IProductRepository repo = new InMemoryProductRepository();
///   IProductService service = new ProductService(repo);
///   var plugin = new ProductPlugin(service);
///   → Her şeyi biz "new"liyoruz, bağımlılık zincirini biz kuruyoruz.
/// 
/// SONRA (DI Container):
///   services.AddSingleton&lt;IProductRepository, InMemoryProductRepository&gt;();
///   services.AddSingleton&lt;IProductService, ProductService&gt;();
///   → Container otomatik çözer: "ProductService'in constructor'ında IProductRepository var,
///     onu da ben kayıt ettim, o zaman InMemoryProductRepository veririm."
/// 
/// Agentic açıdan neden önemli?
/// → Tool (Plugin) sayısı arttıkça, her tool'un bağımlılıkları artar.
/// → Manuel yönetim sürdürülemez hale gelir.
/// → DI ile yeni bir tool eklemek = 2 satır ekleme (register + plugin factory).
/// 
/// Extension Method Pattern:
/// → IServiceCollection'a "AddECommerceAgent" metodu ekliyoruz.
/// → Program.cs'de tek satırla tüm konfigürasyon yapılır.
/// → Bu pattern .NET ekosisteminin standart yaklaşımıdır (AddDbContext, AddAuthentication gibi).
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddECommerceAgent(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ═══════════════════════════════════════════════════════════
        // ADIM 1: Configuration'dan ayarları oku
        // ═══════════════════════════════════════════════════════════
        // API key gibi hassas bilgiler kod içinde hardcoded olmamalı.
        // IConfiguration sayesinde şu kaynaklardan okunabilir:
        //   - appsettings.json (geliştirme)
        //   - Environment variables (production) → OPENAI__APIKEY
        //   - User secrets (geliştirme, git-safe)
        //
        // Öncelik: Environment Variable > appsettings.json
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException(
                "OpenAI:ApiKey yapılandırılmamış! appsettings.json'a veya " +
                "OPENAI__APIKEY environment variable'ına ekleyin.");

        var modelId = configuration["OpenAI:ModelId"] ?? "gpt-4o";

        // ═══════════════════════════════════════════════════════════
        // ADIM 2: Infrastructure katmanı — Veri erişim servisleri
        // ═══════════════════════════════════════════════════════════
        // AddSingleton → Uygulama boyunca tek instance.
        // Neden Singleton? → MockStore static olduğu için aynı veriye
        // erişmek istiyoruz. Gerçek projede Scoped/Transient olabilir.
        services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        services.AddSingleton<ICartRepository, InMemoryCartRepository>();

        // ═══════════════════════════════════════════════════════════
        // ADIM 3: Application katmanı — İş mantığı servisleri
        // ═══════════════════════════════════════════════════════════
        // DI Container burada otomatik çözer:
        //   CartService(ICartRepository, IProductRepository) →
        //     → ICartRepository = InMemoryCartRepository (ADIM 2'de kayıt ettik)
        //     → IProductRepository = InMemoryProductRepository (ADIM 2'de kayıt ettik)
        services.AddSingleton<IProductService, ProductService>();
        services.AddSingleton<ICartService, CartService>();

        // ═══════════════════════════════════════════════════════════
        // ADIM 4: Semantic Kernel — Agent'ın beyni
        // ═══════════════════════════════════════════════════════════
        // AddKernel() → Kernel'ı DI'a kaydeder (Transient olarak).
        //   Her resolve'da yeni Kernel instance, ama aynı plugin'leri alır.
        // AddOpenAIChatCompletion() → LLM bağlantısını Kernel'a ekler.
        services.AddKernel();
        services.AddOpenAIChatCompletion(modelId, apiKey);

        // ═══════════════════════════════════════════════════════════
        // ADIM 5: Plugin'ler — Agent'ın "elleri" (tool'ları)
        // ═══════════════════════════════════════════════════════════
        // Plugin sınıflarını DI'a kaydediyoruz. Constructor'daki
        // IProductService, ICartService otomatik inject edilir.
        //
        // KernelPluginFactory.CreateFromObject → Plugin sınıfını Semantic Kernel'ın
        // anlayacağı KernelPlugin nesnesine dönüştürür.
        // [KernelFunction] ile işaretli metotları tarar ve tool olarak kaydeder.
        services.AddSingleton<ProductPlugin>();
        services.AddSingleton<CartPlugin>();

        services.AddSingleton<KernelPlugin>(sp =>
            KernelPluginFactory.CreateFromObject(
                sp.GetRequiredService<ProductPlugin>(), "ProductPlugin"));

        services.AddSingleton<KernelPlugin>(sp =>
            KernelPluginFactory.CreateFromObject(
                sp.GetRequiredService<CartPlugin>(), "CartPlugin"));

        // ═══════════════════════════════════════════════════════════
        // ADIM 6: Observability — Agent'ı izleme (Filter)
        // ═══════════════════════════════════════════════════════════
        // IAutoFunctionInvocationFilter → Her tool çağrısını intercept eder.
        // Kernel bu filter'ı DI'dan otomatik alır.
        services.AddSingleton<IAutoFunctionInvocationFilter, ToolLoggingFilter>();

        // ═══════════════════════════════════════════════════════════
        // ADIM 7: Orchestrator — Chat döngüsü
        // ═══════════════════════════════════════════════════════════
        // ChatOrchestrator(Kernel) → Kernel DI'dan inject edilir.
        services.AddTransient<ChatOrchestrator>();

        return services;
    }
}
