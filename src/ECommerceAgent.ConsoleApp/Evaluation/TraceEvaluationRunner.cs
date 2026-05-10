using System.Text;
using System.Text.Json;

namespace ECommerceAgent.ConsoleApp.Evaluation;

public static class TraceEvaluationRunner
{
    private static readonly HashSet<string> CartWriteTools = new(StringComparer.OrdinalIgnoreCase)
    {
        "AddToCart",
        "RemoveFromCart"
    };

    private static readonly HashSet<string> ReadOnlyTools = new(StringComparer.OrdinalIgnoreCase)
    {
        "SearchProducts",
        "GetCart",
        "GetCustomerProfile",
        "GetOrderHistory",
        "GetOrderStatus",
        "GetOrderDetails"
    };

    private static readonly HashSet<string> WriteTools = new(StringComparer.OrdinalIgnoreCase)
    {
        "AddToCart",
        "RemoveFromCart",
        "CancelOrder"
    };

    public static void Run(string appBaseDirectory)
    {
        var traceRoot = Path.Combine(appBaseDirectory, "traces");
        if (!Directory.Exists(traceRoot))
        {
            Console.WriteLine($"Trace klasoru bulunamadi: {traceRoot}");
            Console.WriteLine("Once console agent ile birkac manuel senaryo calistirin.");
            return;
        }

        var traceFiles = Directory
            .EnumerateFiles(traceRoot, "*.json", SearchOption.AllDirectories)
            .OrderBy(path => path)
            .ToList();

        if (traceFiles.Count == 0)
        {
            Console.WriteLine($"Trace dosyasi bulunamadi: {traceRoot}");
            return;
        }

        var analyzedTraces = traceFiles
            .Select(ReadTrace)
            .Where(trace => trace is not null)
            .Cast<AnalyzedTrace>()
            .ToList();

        var issues = analyzedTraces.SelectMany(Analyze).ToList();

        PrintSummary(traceRoot, analyzedTraces, issues);
        PrintIssues(issues);

        var reportPath = WriteMarkdownReport(appBaseDirectory, traceRoot, analyzedTraces, issues);
        Console.WriteLine();
        Console.WriteLine($"Markdown report  : {reportPath}");
    }

    private static AnalyzedTrace? ReadTrace(string path)
    {
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var root = document.RootElement;

            var toolCalls = new List<AnalyzedToolCall>();
            if (root.TryGetProperty("toolCalls", out var toolCallsElement) &&
                toolCallsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var toolCall in toolCallsElement.EnumerateArray())
                {
                    var functionName = GetString(toolCall, "functionName");
                    var riskLevel = GetString(toolCall, "riskLevel");

                    toolCalls.Add(new AnalyzedToolCall(
                        functionName,
                        string.IsNullOrWhiteSpace(riskLevel) ? InferRiskLevel(functionName) : riskLevel,
                        GetBool(toolCall, "success"),
                        GetBool(toolCall, "requiresEscalation"),
                        GetString(toolCall, "errorCode")));
                }
            }

            var metrics = root.TryGetProperty("metrics", out var metricsElement)
                ? metricsElement
                : default;

            return new AnalyzedTrace(
                path,
                GetString(root, "turnId"),
                GetString(root, "userInput"),
                GetBool(root, "blockedByInputGuardrail"),
                GetBool(root, "outputGuardrailWarning"),
                metrics.ValueKind == JsonValueKind.Object ? GetLong(metrics, "durationMs") : 0,
                metrics.ValueKind == JsonValueKind.Object ? GetInt(metrics, "toolCallCount") : toolCalls.Count,
                metrics.ValueKind == JsonValueKind.Object ? GetInt(metrics, "errorCount") : toolCalls.Count(t => !t.Success),
                metrics.ValueKind == JsonValueKind.Object ? GetInt(metrics, "escalationCount") : toolCalls.Count(t => t.RequiresEscalation),
                GetEffectiveToolCount(metrics, "readOnlyToolCallCount", toolCalls, "ReadOnly"),
                GetEffectiveWriteToolCount(metrics, toolCalls),
                toolCalls);
        }
        catch (Exception ex)
        {
            return new AnalyzedTrace(
                path,
                "unreadable",
                $"Trace okunamadi: {ex.Message}",
                BlockedByInputGuardrail: false,
                OutputGuardrailWarning: false,
                DurationMs: 0,
                ToolCallCount: 0,
                ErrorCount: 1,
                EscalationCount: 0,
                ReadOnlyToolCallCount: 0,
                WriteToolCallCount: 0,
                ToolCalls: new List<AnalyzedToolCall>());
        }
    }

    private static IEnumerable<EvaluationIssue> Analyze(AnalyzedTrace trace)
    {
        if (trace.BlockedByInputGuardrail)
        {
            yield return new EvaluationIssue("Info", "Safety", trace, "Input guardrail kullanici istegini LLM'e gitmeden blokladi.");
            yield break;
        }

        if (trace.OutputGuardrailWarning)
        {
            yield return new EvaluationIssue("Warning", "Safety", trace, "Output guardrail assistant cevabinda riskli ifade yakaladi.");
        }

        if (trace.ToolCallCount == 0)
        {
            yield return new EvaluationIssue("Warning", "ToolSelection", trace, "E-ticaret asistaninda hic tool kullanilmadan cevap uretilmis olabilir.");
        }

        if (trace.ErrorCount > 0)
        {
            yield return new EvaluationIssue("Warning", "Reliability", trace, $"{trace.ErrorCount} tool hatasi veya basarisiz sonuc var.");
        }

        if (trace.EscalationCount > 0)
        {
            yield return new EvaluationIssue("Info", "Escalation", trace, $"{trace.EscalationCount} tool sonucu insan destegi gerektiriyor.");
        }

        if (trace.DurationMs > 15_000)
        {
            yield return new EvaluationIssue("Warning", "Latency", trace, $"Turn latency yuksek: {trace.DurationMs}ms.");
        }

        if (trace.ToolCalls.Any(t => t.RiskLevel.Equals("Unknown", StringComparison.OrdinalIgnoreCase)))
        {
            yield return new EvaluationIssue("Warning", "ToolRisk", trace, "En az bir tool risk policy tarafindan Unknown siniflanmis.");
        }

        if (trace.ToolCalls.Any(t => CartWriteTools.Contains(t.FunctionName)) &&
            !trace.ToolCalls.Any(t => t.FunctionName.Equals("GetCart", StringComparison.OrdinalIgnoreCase)))
        {
            yield return new EvaluationIssue("Warning", "ReadBeforeWrite", trace, "Sepet write islemi oncesinde GetCart gorunmuyor.");
        }

        if (trace.ToolCalls.Any(t => t.FunctionName.Equals("AddToCart", StringComparison.OrdinalIgnoreCase)) &&
            !trace.ToolCalls.Any(t => t.FunctionName.Equals("SearchProducts", StringComparison.OrdinalIgnoreCase)))
        {
            yield return new EvaluationIssue("Warning", "Grounding", trace, "AddToCart oncesinde SearchProducts gorunmuyor; productId grounding zayif olabilir.");
        }
    }

    private static void PrintSummary(string traceRoot, IReadOnlyCollection<AnalyzedTrace> traces, IReadOnlyCollection<EvaluationIssue> issues)
    {
        var totalTools = traces.Sum(t => t.ToolCallCount);
        var avgDuration = traces.Count == 0 ? 0 : traces.Average(t => t.DurationMs);

        Console.WriteLine("Trace Evaluation Summary");
        Console.WriteLine(new string('-', 40));
        Console.WriteLine($"Trace root       : {traceRoot}");
        Console.WriteLine($"Turns            : {traces.Count}");
        Console.WriteLine($"Tool calls       : {totalTools}");
        Console.WriteLine($"Read tools       : {traces.Sum(t => t.ReadOnlyToolCallCount)}");
        Console.WriteLine($"Write tools      : {traces.Sum(t => t.WriteToolCallCount)}");
        Console.WriteLine($"Errors           : {traces.Sum(t => t.ErrorCount)}");
        Console.WriteLine($"Escalations      : {traces.Sum(t => t.EscalationCount)}");
        Console.WriteLine($"Guardrail blocks : {traces.Count(t => t.BlockedByInputGuardrail)}");
        Console.WriteLine($"Output warnings  : {traces.Count(t => t.OutputGuardrailWarning)}");
        Console.WriteLine($"Avg duration     : {avgDuration:F0}ms");
        Console.WriteLine();

        Console.WriteLine("Issue categories");
        Console.WriteLine(new string('-', 40));
        foreach (var group in issues.GroupBy(i => i.Category).OrderByDescending(g => g.Count()))
        {
            Console.WriteLine($"{group.Key,-16}: {group.Count()}");
        }

        if (issues.Count == 0)
            Console.WriteLine("Issue bulunamadi.");

        Console.WriteLine();
    }

    private static void PrintIssues(IReadOnlyCollection<EvaluationIssue> issues)
    {
        Console.WriteLine("Findings");
        Console.WriteLine(new string('-', 40));

        foreach (var issue in issues
            .OrderByDescending(i => i.Severity == "Warning")
            .ThenBy(i => i.Category)
            .Take(20))
        {
            Console.WriteLine($"[{issue.Severity}] {issue.Category} | Turn: {issue.Trace.TurnId}");
            Console.WriteLine($"Input : {Truncate(issue.Trace.UserInput, 110)}");
            Console.WriteLine($"Detail: {issue.Detail}");
            Console.WriteLine();
        }

        if (issues.Count > 20)
            Console.WriteLine($"Ilk 20 finding gosterildi. Toplam finding: {issues.Count}");

        if (issues.Count == 0)
            Console.WriteLine("Raporlanacak finding yok.");
    }

    private static string WriteMarkdownReport(
        string appBaseDirectory,
        string traceRoot,
        IReadOnlyCollection<AnalyzedTrace> traces,
        IReadOnlyCollection<EvaluationIssue> issues)
    {
        var repositoryRoot = FindRepositoryRoot(appBaseDirectory);
        var reportDirectory = Path.Combine(repositoryRoot, "docs", "evaluation_reports");
        Directory.CreateDirectory(reportDirectory);

        var reportPath = Path.Combine(
            reportDirectory,
            $"{DateTime.Now:yyyyMMdd-HHmmss}-trace-evaluation.md");

        var markdown = BuildMarkdownReport(traceRoot, traces, issues);
        File.WriteAllText(reportPath, markdown);

        return reportPath;
    }

    private static string BuildMarkdownReport(
        string traceRoot,
        IReadOnlyCollection<AnalyzedTrace> traces,
        IReadOnlyCollection<EvaluationIssue> issues)
    {
        var totalTools = traces.Sum(t => t.ToolCallCount);
        var avgDuration = traces.Count == 0 ? 0 : traces.Average(t => t.DurationMs);
        var builder = new StringBuilder();

        builder.AppendLine("# Trace Evaluation Report");
        builder.AppendLine();
        builder.AppendLine($"GeneratedAt: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"TraceRoot: `{traceRoot}`");
        builder.AppendLine();

        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine("| Metric | Value |");
        builder.AppendLine("| --- | ---: |");
        builder.AppendLine($"| Turns | {traces.Count} |");
        builder.AppendLine($"| Tool calls | {totalTools} |");
        builder.AppendLine($"| Read tools | {traces.Sum(t => t.ReadOnlyToolCallCount)} |");
        builder.AppendLine($"| Write tools | {traces.Sum(t => t.WriteToolCallCount)} |");
        builder.AppendLine($"| Errors | {traces.Sum(t => t.ErrorCount)} |");
        builder.AppendLine($"| Escalations | {traces.Sum(t => t.EscalationCount)} |");
        builder.AppendLine($"| Guardrail blocks | {traces.Count(t => t.BlockedByInputGuardrail)} |");
        builder.AppendLine($"| Output warnings | {traces.Count(t => t.OutputGuardrailWarning)} |");
        builder.AppendLine($"| Avg duration | {avgDuration:F0}ms |");
        builder.AppendLine();

        builder.AppendLine("## Issue Categories");
        builder.AppendLine();
        builder.AppendLine("| Category | Count |");
        builder.AppendLine("| --- | ---: |");

        var categoryGroups = issues
            .GroupBy(i => i.Category)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .ToList();

        if (categoryGroups.Count == 0)
        {
            builder.AppendLine("| None | 0 |");
        }
        else
        {
            foreach (var group in categoryGroups)
                builder.AppendLine($"| {EscapeMarkdownTableCell(group.Key)} | {group.Count()} |");
        }

        builder.AppendLine();
        builder.AppendLine("## Findings By Category");
        builder.AppendLine();

        if (issues.Count == 0)
        {
            builder.AppendLine("No findings.");
            builder.AppendLine();
        }
        else
        {
            foreach (var group in categoryGroups)
            {
                builder.AppendLine($"### {group.Key}");
                builder.AppendLine();
                builder.AppendLine("| Severity | Turn | Input | Detail |");
                builder.AppendLine("| --- | --- | --- | --- |");

                foreach (var issue in group
                    .OrderByDescending(i => i.Severity == "Warning")
                    .ThenBy(i => i.Trace.TurnId))
                {
                    builder.AppendLine(
                        $"| {EscapeMarkdownTableCell(issue.Severity)} " +
                        $"| `{EscapeMarkdownTableCell(issue.Trace.TurnId)}` " +
                        $"| {EscapeMarkdownTableCell(Truncate(issue.Trace.UserInput, 120))} " +
                        $"| {EscapeMarkdownTableCell(issue.Detail)} |");
                }

                builder.AppendLine();
            }
        }

        builder.AppendLine("## Turn Overview");
        builder.AppendLine();
        builder.AppendLine("| Turn | Input | Tools | Errors | Escalations | Duration |");
        builder.AppendLine("| --- | --- | ---: | ---: | ---: | ---: |");

        foreach (var trace in traces.OrderBy(t => t.Path))
        {
            builder.AppendLine(
                $"| `{EscapeMarkdownTableCell(trace.TurnId)}` " +
                $"| {EscapeMarkdownTableCell(Truncate(trace.UserInput, 100))} " +
                $"| {trace.ToolCallCount} " +
                $"| {trace.ErrorCount} " +
                $"| {trace.EscalationCount} " +
                $"| {trace.DurationMs}ms |");
        }

        builder.AppendLine();
        builder.AppendLine("## How To Read This Report");
        builder.AppendLine();
        builder.AppendLine("- ToolSelection: Agent expected to use a tool but did not, or likely chose the wrong tool.");
        builder.AppendLine("- Grounding: A product/order/customer decision may not be backed by tool data.");
        builder.AppendLine("- ReadBeforeWrite: A write action happened without the expected current-state read.");
        builder.AppendLine("- Safety: Guardrail block or warning was observed.");
        builder.AppendLine("- Escalation: A tool result says human support is needed.");
        builder.AppendLine("- Reliability: Tool failure, not-found result, or unsuccessful business operation.");
        builder.AppendLine("- Latency: Turn duration is high enough to investigate.");

        return builder.ToString();
    }

    private static string FindRepositoryRoot(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);

        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ECommerceAgent.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return AppContext.BaseDirectory;
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;
    }

    private static int GetInt(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.TryGetInt32(out var result)
            ? result
            : 0;
    }

    private static long GetLong(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.TryGetInt64(out var result)
            ? result
            : 0;
    }

    private static bool GetBool(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) &&
               value.ValueKind is JsonValueKind.True or JsonValueKind.False &&
               value.GetBoolean();
    }

    private static int GetEffectiveToolCount(
        JsonElement metrics,
        string propertyName,
        IReadOnlyCollection<AnalyzedToolCall> toolCalls,
        string riskLevel)
    {
        var metricValue = metrics.ValueKind == JsonValueKind.Object ? GetInt(metrics, propertyName) : 0;
        return metricValue > 0
            ? metricValue
            : toolCalls.Count(toolCall => toolCall.RiskLevel.Equals(riskLevel, StringComparison.OrdinalIgnoreCase));
    }

    private static int GetEffectiveWriteToolCount(JsonElement metrics, IReadOnlyCollection<AnalyzedToolCall> toolCalls)
    {
        var metricValue = metrics.ValueKind == JsonValueKind.Object ? GetInt(metrics, "writeToolCallCount") : 0;
        return metricValue > 0
            ? metricValue
            : toolCalls.Count(toolCall =>
                toolCall.RiskLevel.Equals("LowRiskWrite", StringComparison.OrdinalIgnoreCase) ||
                toolCall.RiskLevel.Equals("MediumRiskWrite", StringComparison.OrdinalIgnoreCase));
    }

    private static string InferRiskLevel(string functionName)
    {
        if (ReadOnlyTools.Contains(functionName))
            return "ReadOnly";

        if (WriteTools.Contains(functionName))
            return functionName.Equals("CancelOrder", StringComparison.OrdinalIgnoreCase)
                ? "MediumRiskWrite"
                : "LowRiskWrite";

        return "Unknown";
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength] + "...";
    }

    private static string EscapeMarkdownTableCell(string value)
    {
        return value
            .Replace("|", "\\|")
            .Replace("\r", " ")
            .Replace("\n", " ");
    }

    private sealed record AnalyzedTrace(
        string Path,
        string TurnId,
        string UserInput,
        bool BlockedByInputGuardrail,
        bool OutputGuardrailWarning,
        long DurationMs,
        int ToolCallCount,
        int ErrorCount,
        int EscalationCount,
        int ReadOnlyToolCallCount,
        int WriteToolCallCount,
        IReadOnlyList<AnalyzedToolCall> ToolCalls);

    private sealed record AnalyzedToolCall(
        string FunctionName,
        string RiskLevel,
        bool Success,
        bool RequiresEscalation,
        string ErrorCode);

    private sealed record EvaluationIssue(
        string Severity,
        string Category,
        AnalyzedTrace Trace,
        string Detail);
}
