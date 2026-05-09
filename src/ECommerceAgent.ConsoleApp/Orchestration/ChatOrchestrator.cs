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
        Sen bir e-ticaret alisveris asistanisin.
        Kullaniciya urun arama, sepet yonetimi, musteri profili, siparis gecmisi, siparis durumu ve siparis iptali konularinda yardim edersin.
        Sadece bu konularda yardim edersin. Konu disi sorulari kibarca reddedersin.
        Tool sonuclarinda success, message, errorCode, requiresEscalation ve data alanlarini dikkatle yorumlarsin.
        requiresEscalation true ise islemi tamamlanmis gibi anlatmazsin; kullaniciya insan destegi gerektigini soylersin.
        Turkce yanit verirsin. Fiyatlari TL cinsinden gosterirsin.
        """;

    public ChatOrchestrator(Kernel kernel)
    {
        _kernel = kernel;
        _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        _chatHistory = new ChatHistory(SystemPrompt);
    }

    public async Task RunAsync()
    {
        Console.WriteLine("E-Ticaret Sepet Asistani");
        Console.WriteLine("Cikmak icin 'q' yazin.");
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

        Console.WriteLine("Gorusmek uzere!");
    }
}
