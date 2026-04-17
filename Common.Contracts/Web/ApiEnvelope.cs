namespace Common.Contracts.Web;

public record ApiEnvelope<T>(bool Success, string Message, T? Data)
{
    public static ApiEnvelope<T> Ok(string message, T? data = default) => new(true, message, data);
    public static ApiEnvelope<T> Fail(string message, T? data = default) => new(false, message, data);
}
