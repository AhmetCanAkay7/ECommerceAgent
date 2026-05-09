using System.Diagnostics;
using System.Text.Json;
using Microsoft.SemanticKernel;
using ECommerceAgent.ConsoleApp.Observability;

namespace ECommerceAgent.ConsoleApp.Filters;

public sealed class ObservabilityFilter : IAutoFunctionInvocationFilter
{
    private readonly AgentTraceContext _traceContext;

    public ObservabilityFilter(AgentTraceContext traceContext)
    {
        _traceContext = traceContext;
    }

    public async Task OnAutoFunctionInvocationAsync(
        AutoFunctionInvocationContext context, // context = o anki tool
        Func<AutoFunctionInvocationContext, Task> next)
    {
        var functionName = context.Function.Name;
        var pluginName = context.Function.PluginName ?? "UnknownPlugin";
        var arguments = ToArgumentDictionary(context.Arguments);

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"[Tool] START {pluginName}.{functionName}");
        Console.WriteLine($"[Tool] Args  {FormatArguments(arguments)}");
        Console.ResetColor();

        var stopwatch = Stopwatch.StartNew();
        Exception? exception = null;

        try
        {
            await next(context); // benden sonraki filter'ı çalıştır varsa, yoksa benden devam
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            var resultText = context.Result?.ToString() ?? string.Empty;
            var parsedResult = ToolResultMetadata.TryParse(resultText);
            var resultPreview = Truncate(resultText, 300);

            var trace = new ToolCallTrace
            {
                PluginName = pluginName,
                FunctionName = functionName,
                Arguments = arguments,
                DurationMs = stopwatch.ElapsedMilliseconds,
                Success = exception == null && (parsedResult?.Success ?? true),
                ErrorCode = parsedResult?.ErrorCode,
                RequiresEscalation = parsedResult?.RequiresEscalation ?? false,
                Message = parsedResult?.Message,
                ResultPreview = resultPreview,
                ExceptionType = exception?.GetType().Name,
                ExceptionMessage = exception?.Message
            };

            _traceContext.AddToolCall(trace);

            Console.ForegroundColor = trace.Success ? ConsoleColor.DarkGray : ConsoleColor.Red;
            Console.WriteLine($"[Tool] END   {pluginName}.{functionName} | Success: {trace.Success} | Duration: {trace.DurationMs}ms");
            if (!string.IsNullOrWhiteSpace(trace.Message))
                Console.WriteLine($"[Result] Message: {trace.Message}");
            if (!string.IsNullOrWhiteSpace(trace.ErrorCode))
                Console.WriteLine($"[Result] ErrorCode: {trace.ErrorCode}");
            if (trace.RequiresEscalation)
                Console.WriteLine("[Result] RequiresEscalation: true");
            if (exception != null)
                Console.WriteLine($"[Result] Exception: {trace.ExceptionType}: {trace.ExceptionMessage}");
            Console.WriteLine($"[Result] Preview: {resultPreview}");
            Console.ResetColor();
        }
    }

    private static Dictionary<string, string?> ToArgumentDictionary(KernelArguments? arguments)
    {
        if (arguments == null)
            return new Dictionary<string, string?>();

        return arguments.ToDictionary(
            argument => argument.Key,
            argument => argument.Value?.ToString());
    }

    private static string FormatArguments(Dictionary<string, string?> arguments)
    {
        if (arguments.Count == 0)
            return "parametresiz";

        return string.Join(", ", arguments.Select(argument => $"{argument.Key}: {argument.Value}"));
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "null";

        return value.Length <= maxLength ? value : value[..maxLength] + "...";
    }

    private sealed record ToolResultMetadata(
        bool Success,
        string? ErrorCode,
        bool RequiresEscalation,
        string? Message)
    {
        public static ToolResultMetadata? TryParse(string resultText)
        {
            if (string.IsNullOrWhiteSpace(resultText))
                return null;

            try
            {
                using var document = JsonDocument.Parse(resultText);
                var root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Object ||
                    !root.TryGetProperty("success", out var successProperty))
                {
                    return null;
                }

                var success = successProperty.ValueKind == JsonValueKind.True;
                var errorCode = TryGetString(root, "errorCode");
                var requiresEscalation = TryGetBoolean(root, "requiresEscalation");
                var message = TryGetString(root, "message");

                return new ToolResultMetadata(success, errorCode, requiresEscalation, message);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string? TryGetString(JsonElement root, string propertyName)
        {
            return root.TryGetProperty(propertyName, out var property) &&
                   property.ValueKind == JsonValueKind.String
                ? property.GetString()
                : null;
        }

        private static bool TryGetBoolean(JsonElement root, string propertyName)
        {
            return root.TryGetProperty(propertyName, out var property) &&
                   property.ValueKind == JsonValueKind.True;
        }
    }
}
