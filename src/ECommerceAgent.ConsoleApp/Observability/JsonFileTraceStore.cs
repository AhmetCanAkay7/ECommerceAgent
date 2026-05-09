using System.Text.Json;

namespace ECommerceAgent.ConsoleApp.Observability;

public sealed class JsonFileTraceStore : IAgentTraceStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly string _traceRootPath;

    public JsonFileTraceStore()
    {
        _traceRootPath = Path.Combine(AppContext.BaseDirectory, "traces");
    }

    public async Task<string> SaveAsync(AgentTrace trace, CancellationToken cancellationToken = default)
    {
        var dateFolder = trace.StartedAtUtc.ToString("yyyy-MM-dd");
        var directory = Path.Combine(_traceRootPath, dateFolder);
        Directory.CreateDirectory(directory);

        var fileName = $"{trace.StartedAtUtc:HHmmssfff}-{trace.TurnId}.json";
        var path = Path.Combine(directory, fileName);
        var json = JsonSerializer.Serialize(trace, JsonOptions);

        await File.WriteAllTextAsync(path, json, cancellationToken);
        return path;
    }
}
