using System.Diagnostics;
using Microsoft.SemanticKernel;

namespace ECommerceAgent.ConsoleApp.Filters;
public class ToolLoggingFilter : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(
        AutoFunctionInvocationContext context,
        Func<AutoFunctionInvocationContext, Task> next)
    {
        var functionName = context.Function.Name;
        var pluginName = context.Function.PluginName;

        // Parametreleri oku — LLM'in tool'a gönderdiği argümanlar
        var arguments = context.Arguments;
        var paramStr = arguments != null
            ? string.Join(", ", arguments.Select(a => $"{a.Key}: {a.Value}"))
            : "parametresiz";

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"\n [{pluginName}] {functionName} çağrılıyor...");
        Console.WriteLine($"     Parametreler: {paramStr}");
        Console.ResetColor();

        var stopwatch = Stopwatch.StartNew();
        await next(context);
        stopwatch.Stop();

        var result = context.Result;
        var resultPreview = result?.ToString() ?? "null";

        // Uzun sonuçları kırp — log'u okunabilir tut
        if (resultPreview.Length > 200)
            resultPreview = resultPreview[..200] + "...";

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  ✅ {functionName} tamamlandı ({stopwatch.ElapsedMilliseconds}ms)");
        Console.WriteLine($"     Sonuç: {resultPreview}");
        Console.ResetColor();
    }
}
