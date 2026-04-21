using Microsoft.Extensions.Configuration;
using ECommerceAgent.ConsoleApp.Configuration;
using ECommerceAgent.ConsoleApp.Orchestration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var apiKey = configuration["OpenAI:ApiKey"]!;
var modelId = configuration["OpenAI:ModelId"] ?? "gpt-4o";

var kernel = KernelConfiguration.CreateKernel(apiKey, modelId);
var orchestrator = new ChatOrchestrator(kernel);
await orchestrator.RunAsync();
