using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ECommerceAgent.ConsoleApp.Configuration;
using ECommerceAgent.ConsoleApp.Orchestration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();
services.AddECommerceAgent(configuration);


var serviceProvider = services.BuildServiceProvider();

// ═══════════════════════════════════════════════════════════
// Uygulama Başlatma
// ═══════════════════════════════════════════════════════════
// ChatOrchestrator'ı DI'dan resolve ediyoruz.
// Container otomatik olarak:
//   1. Kernel oluşturur (+ OpenAI bağlantısı + Plugin'ler + Filter)
//   2. ChatOrchestrator(Kernel) constructor'ına Kernel'ı inject eder
var orchestrator = serviceProvider.GetRequiredService<ChatOrchestrator>();
await orchestrator.RunAsync();
