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
using ECommerceAgent.ConsoleApp.Observability;

namespace ECommerceAgent.ConsoleApp.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddECommerceAgent(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey) ||
            apiKey.Equals("YOUR_API_KEY", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "OpenAI:ApiKey configured degil. OpenAI__ApiKey environment variable kullanin " +
                "veya git'e girmeyen appsettings.Local.json dosyasina ekleyin.");
        }

        var modelId = configuration["OpenAI:ModelId"];
        if (string.IsNullOrWhiteSpace(modelId) ||
            modelId.Equals("YOUR_MODEL_ID", StringComparison.OrdinalIgnoreCase))
        {
            modelId = "gpt-4o";
        }

        services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        services.AddSingleton<ICartRepository, InMemoryCartRepository>();
        services.AddSingleton<ICustomerRepository, InMemoryCustomerRepository>();
        services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();

        services.AddSingleton<IProductService, ProductService>();
        services.AddSingleton<ICartService, CartService>();
        services.AddSingleton<ICustomerService, CustomerService>();
        services.AddSingleton<IOrderService, OrderService>();

        services.AddKernel();
        services.AddOpenAIChatCompletion(modelId, apiKey);

        services.AddSingleton<AgentTraceContext>();
        services.AddSingleton<IAgentTraceStore, JsonFileTraceStore>();

        services.AddSingleton<ProductPlugin>();
        services.AddSingleton<CartPlugin>();
        services.AddSingleton<CustomerPlugin>();
        services.AddSingleton<OrderPlugin>();

        services.AddSingleton<KernelPlugin>(sp =>
            KernelPluginFactory.CreateFromObject(
                sp.GetRequiredService<ProductPlugin>(), "ProductPlugin"));

        services.AddSingleton<KernelPlugin>(sp =>
            KernelPluginFactory.CreateFromObject(
                sp.GetRequiredService<CartPlugin>(), "CartPlugin"));

        services.AddSingleton<KernelPlugin>(sp =>
            KernelPluginFactory.CreateFromObject(
                sp.GetRequiredService<CustomerPlugin>(), "CustomerPlugin"));

        services.AddSingleton<KernelPlugin>(sp =>
            KernelPluginFactory.CreateFromObject(
                sp.GetRequiredService<OrderPlugin>(), "OrderPlugin"));

        services.AddSingleton<IAutoFunctionInvocationFilter, ObservabilityFilter>();
        services.AddTransient<ChatOrchestrator>();

        return services;
    }
}
