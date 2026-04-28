using System.Text.Json.Serialization;

namespace Maono.Api.Common;

/// <summary>
/// Unified API response wrapper.
/// Guarantees: message is NEVER null, statusCode is ALWAYS present.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ApiError>? Errors { get; set; }

    // ── Success Factories ─────────────────────────────────────
    public static ApiResponse<T> Ok(T data, string message = "Opération réussie", int statusCode = 200) => new()
    {
        Success = true,
        StatusCode = statusCode,
        Message = message,
        Data = data
    };

    public static ApiResponse<T> Created(T data, string message = "Ressource créée") => new()
    {
        Success = true,
        StatusCode = 201,
        Message = message,
        Data = data
    };

    // ── Error Factories ───────────────────────────────────────
    public static ApiResponse<T> Error(string message, int statusCode = 400, string? errorCode = null) => new()
    {
        Success = false,
        StatusCode = statusCode,
        Message = message,
        Errors = new List<ApiError>
        {
            new(errorCode ?? "domain_rule", message)
        }
    };

    public static ApiResponse<T> Error(string message, int statusCode, List<ApiError> errors) => new()
    {
        Success = false,
        StatusCode = statusCode,
        Message = message,
        Errors = errors
    };
}

/// <summary>
/// Non-generic version for responses without data.
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string message = "Opération réussie", int statusCode = 200) => new()
    {
        Success = true,
        StatusCode = statusCode,
        Message = message
    };
}

/// <summary>
/// Structured error detail.
/// </summary>
public record ApiError(string Code, string Message);
