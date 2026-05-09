namespace ECommerceAgent.Application.Common;

public sealed class Result<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? ErrorCode { get; init; }
    public bool RequiresEscalation { get; init; }
    public T? Data { get; init; }

    public static Result<T> Ok(T data, string message = "")
    {
        return new Result<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static Result<T> Ok(string message)
    {
        return new Result<T>
        {
            Success = true,
            Message=message
        };
    }

    public static Result<T> Fail(
        string message,
        string? errorCode = null,
        bool requiresEscalation = false)
    {
        return new Result<T>
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode,
            RequiresEscalation = requiresEscalation
        };
    }
}
