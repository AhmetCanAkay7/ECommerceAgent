using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace ECommerceAgent.ConsoleApp.Orchestration;

public class ChatOrchestrator
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly ChatHistory _chatHistory;

    private const string SystemPrompt =
        """
        Sen bir alışveriş asistanısın. Kullanıcıya ürün aramada, sepet yönetiminde yardımcı olursun.
        Sadece bu konularda yardım edersin. Konu dışı sorulara kibarca reddedersin.
        Türkçe yanıt verirsin. Fiyatları TL cinsinden gösterirsin.
        """;

    public ChatOrchestrator(Kernel kernel)
    {
        _kernel = kernel;
        _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        _chatHistory = new ChatHistory(SystemPrompt);
    }

    public async Task RunAsync()
    {
        Console.WriteLine("🛒 E-Ticaret Sepet Asistanı");
        Console.WriteLine("Çıkmak için 'q' yazın.");
        Console.WriteLine(new string('-', 40));

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Sen > ");
            Console.ResetColor();

            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
                continue;

            if (input.Equals("q", StringComparison.OrdinalIgnoreCase))
                break;

            _chatHistory.AddUserMessage(input);

            var settings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var response = await _chatCompletionService.GetChatMessageContentAsync(
                _chatHistory,
                settings,
                _kernel);

            _chatHistory.AddAssistantMessage(response.Content ?? string.Empty);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Asistan > ");
            Console.ResetColor();
            Console.WriteLine(response.Content);
            Console.WriteLine();
        }

        Console.WriteLine("Görüşmek üzere! 👋");
    }
}
