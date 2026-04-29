using System.Diagnostics;
using Microsoft.SemanticKernel;

namespace ECommerceAgent.ConsoleApp.Filters;

/// <summary>
/// Tool çağrısı loglama filter'ı — Agent'ın her hareketini izler.
/// 
/// IAutoFunctionInvocationFilter, Semantic Kernel'ın middleware pattern'ıdır.
/// Her tool çağrısından ÖNCE ve SONRA çalışır.
/// 
/// 🎓 Neden bu kadar önemli?
/// 
/// 1. LLM non-deterministic → Aynı soruya farklı zamanlarda farklı tool çağırabilir.
///    Log olmadan "neden yanlış tool çağrıldı?" sorusuna cevap veremezsin.
/// 
/// 2. IBM sunumundaki "observability" dersi:
///    → "Kim hangi tool'u çağırdı izlenmeli"
///    → "Agentic AI sadece akıllı davranmak değil, izlenebilir davranmak zorunda"
/// 
/// 3. Production'da bu loglar:
///    → Performans sorunlarını tespit eder (hangi tool yavaş?)
///    → Maliyet analizi yapar (kaç tool call = kaç token?)
///    → Hata ayıklama sağlar (tool hata döndü mü?)
/// 
/// Akış:
///   LLM karar verir → "SearchProducts çağıracağım"
///     → Filter ÖNCE: "🔧 SearchProducts çağrılıyor, params: {query: süt}"
///       → Plugin metodu çalışır → sonuç döner
///     → Filter SONRA: "✅ SearchProducts tamamlandı (125ms)"
/// </summary>
public class ToolLoggingFilter : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(
        AutoFunctionInvocationContext context,
        Func<AutoFunctionInvocationContext, Task> next)
    {
        // ═══════════════════════════════════════════════════════════
        // TOOL ÇAĞRISI ÖNCESİ — Hangi tool, hangi parametrelerle?
        // ═══════════════════════════════════════════════════════════
        var functionName = context.Function.Name;
        var pluginName = context.Function.PluginName;

        // Parametreleri oku — LLM'in tool'a gönderdiği argümanlar
        var arguments = context.Arguments;
        var paramStr = arguments != null
            ? string.Join(", ", arguments.Select(a => $"{a.Key}: {a.Value}"))
            : "parametresiz";

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"\n  🔧 [{pluginName}] {functionName} çağrılıyor...");
        Console.WriteLine($"     Parametreler: {paramStr}");
        Console.ResetColor();

        // ═══════════════════════════════════════════════════════════
        // TOOL ÇALIŞIYOR — Süreyi ölçüyoruz
        // ═══════════════════════════════════════════════════════════
        // next() → Asıl plugin metodunu çalıştırır.
        // Bu satır olmazsa tool ÇALIŞMAZ — middleware zinciri kırılır.
        var stopwatch = Stopwatch.StartNew();
        await next(context);
        stopwatch.Stop();

        // ═══════════════════════════════════════════════════════════
        // TOOL ÇAĞRISI SONRASI — Sonuç ne, ne kadar sürdü?
        // ═══════════════════════════════════════════════════════════
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
