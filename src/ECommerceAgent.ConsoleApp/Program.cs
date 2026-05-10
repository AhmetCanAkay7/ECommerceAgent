using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ECommerceAgent.ConsoleApp.Configuration;
using ECommerceAgent.ConsoleApp.Evaluation;
using ECommerceAgent.ConsoleApp.Orchestration;

if (args.Any(arg => arg.Equals("--analyze-traces", StringComparison.OrdinalIgnoreCase)))
{
    TraceEvaluationRunner.Run(AppContext.BaseDirectory);
    return;
}

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();
services.AddECommerceAgent(configuration);

var serviceProvider = services.BuildServiceProvider();

var orchestrator = serviceProvider.GetRequiredService<ChatOrchestrator>();
await orchestrator.RunAsync();
