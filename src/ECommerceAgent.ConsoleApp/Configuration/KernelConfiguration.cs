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

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddECommerceAgent(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException(
                "OpenAI:ApiKey yapılandırılmamış! appsettings.json'a veya " +
                "OPENAI__APIKEY environment variable'ına ekleyin.");

        var modelId = configuration["OpenAI:ModelId"] ?? "gpt-4o";

        services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        services.AddSingleton<ICartRepository, InMemoryCartRepository>();
        services.AddSingleton<ICustomerRepository, InMemoryCustomerRepository>();
        services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();

        services.AddSingleton<IProductService, ProductService>();
        services.AddSingleton<ICartService, CartService>();
        services.AddSingleton<ICustomerService, CustomerService>();
        services.AddSingleton<IOrderService, OrderService>();

        // AddOpenAIChatCompletion() → LLM bağlantısını Kernel'a ekler.
        services.AddKernel();
        services.AddOpenAIChatCompletion(modelId, apiKey);

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

        // Observability — Agent'ı izleme (Filter)

        services.AddSingleton<IAutoFunctionInvocationFilter, ToolLoggingFilter>();

        // ChatOrchestrator(Kernel) → Kernel DI'dan inject edilir.
        services.AddTransient<ChatOrchestrator>();

        return services;
    }
}
