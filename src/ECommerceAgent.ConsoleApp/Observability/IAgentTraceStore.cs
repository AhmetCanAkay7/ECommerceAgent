namespace ECommerceAgent.ConsoleApp.Observability;

public interface IAgentTraceStore
{
    Task<string> SaveAsync(AgentTrace trace, CancellationToken cancellationToken = default);
}
