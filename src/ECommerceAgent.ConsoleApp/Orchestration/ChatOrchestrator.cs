using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Diagnostics;
using ECommerceAgent.ConsoleApp.Observability;

namespace ECommerceAgent.ConsoleApp.Orchestration;

public class ChatOrchestrator
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly ChatHistory _chatHistory;
    private readonly AgentTraceContext _traceContext;
    private readonly IAgentTraceStore _traceStore;
    private readonly string _conversationId = Guid.NewGuid().ToString("N");

    private const string SystemPrompt =
        """
        Sen bir e-ticaret alisveris asistanisin.
        Kullaniciya urun arama, sepet yonetimi, musteri profili, siparis gecmisi, siparis durumu ve siparis iptali konularinda yardim edersin.
        Sadece bu konularda yardim edersin. Konu disi sorulari kibarca reddedersin.
        Tool sonuclarinda success, message, errorCode, requiresEscalation ve data alanlarini dikkatle yorumlarsin.
        requiresEscalation true ise islemi tamamlanmis gibi anlatmazsin; kullaniciya insan destegi gerektigini soylersin.
        Dinamik state iceren konularda chat history'ye guvenmezsin; cevap vermeden once ilgili read tool'u cagirirsin.
        Sepet sorularinda her zaman GetCart tool'unu kullanirsin.
        Urun veya stok sorularinda SearchProducts tool'unu kullanirsin.
        Siparis durumu icin GetOrderStatus, siparis detayi icin GetOrderDetails tool'unu kullanirsin.
        Musteri profili icin GetCustomerProfile, musteri siparis gecmisi icin GetOrderHistory tool'unu kullanirsin.
        Write action oncesinde guncel state'in Application service tarafindan dogrulanacagini bilirsin; tool sonucu basarisizsa islemi tamamlanmis gibi anlatmazsin.
        Turkce yanit verirsin. Fiyatlari TL cinsinden gosterirsin.
        """;

    public ChatOrchestrator(
        Kernel kernel,
        AgentTraceContext traceContext,
        IAgentTraceStore traceStore)
    {
        _kernel = kernel;
        _traceContext = traceContext;
        _traceStore = traceStore;
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

            var turnStopwatch = Stopwatch.StartNew();
            var trace = _traceContext.BeginTurn(_conversationId, input);
            _chatHistory.AddUserMessage(input);

            var settings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            try
            {
                var response = await _chatCompletionService.GetChatMessageContentAsync(
                    _chatHistory,
                    settings,
                    _kernel);

                var assistantOutput = response.Content ?? string.Empty;
                _chatHistory.AddAssistantMessage(assistantOutput);

                turnStopwatch.Stop();
                _traceContext.CompleteTurn(assistantOutput, turnStopwatch.ElapsedMilliseconds);
                await _traceStore.SaveAsync(trace);
                PrintTraceSummary(trace);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Asistan > ");
                Console.ResetColor();
                Console.WriteLine(response.Content);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                turnStopwatch.Stop();
                _traceContext.FailTurn(ex, turnStopwatch.ElapsedMilliseconds);
                await _traceStore.SaveAsync(trace);
                PrintTraceSummary(trace);
                throw;
            }
            finally
            {
                _traceContext.Clear();
            }
        }

        Console.WriteLine("Gorusmek uzere!");
    }

    private static void PrintTraceSummary(AgentTrace trace)
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(
            $"Trace: {trace.TurnId} | Tools: {trace.Metrics.ToolCallCount} | " +
            $"Errors: {trace.Metrics.ErrorCount} | Escalations: {trace.Metrics.EscalationCount} | " +
            $"Duration: {trace.Metrics.DurationMs}ms");
        Console.ResetColor();
    }
}
