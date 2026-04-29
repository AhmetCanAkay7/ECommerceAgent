using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ECommerceAgent.ConsoleApp.Configuration;
using ECommerceAgent.ConsoleApp.Orchestration;

// ═══════════════════════════════════════════════════════════
// Configuration Pipeline
// ═══════════════════════════════════════════════════════════
// Konfigürasyon kaynakları öncelik sırasıyla yüklenir:
//   1. appsettings.json        → Temel ayarlar (model, genel config)
//   2. Environment Variables   → Hassas bilgiler (API key)
//
// Environment variable örneği (PowerShell):
//   $env:OpenAI__ApiKey = "sk-proj-..."
//
// Çift underscore (__) → JSON'daki ":" (nested key) karşılığı.
// Environment variable varsa appsettings.json'daki değeri ezer (override).
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .Build();

// ═══════════════════════════════════════════════════════════
// DI Container Kurulumu
// ═══════════════════════════════════════════════════════════
// ServiceCollection = DI Container'ın "kayıt defteri".
// AddECommerceAgent → Tüm bağımlılıkları tek satırla kaydeder:
//   Repository'ler, Service'ler, Plugin'ler, Kernel, Filter, Orchestrator
//
// 🎓 Önceki hali:
//   var kernel = KernelConfiguration.CreateKernel(apiKey, modelId);
//   var orchestrator = new ChatOrchestrator(kernel);
//
// Şimdiki hali: Container her şeyi kendisi çözer.
var services = new ServiceCollection();
services.AddECommerceAgent(configuration);

// BuildServiceProvider → Kayıtları dondurup "resolver" oluşturur.
// Artık GetRequiredService ile istediğimiz servisi alabiliriz.
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
