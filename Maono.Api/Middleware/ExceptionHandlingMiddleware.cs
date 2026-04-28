using FluentValidation;
using Maono.Api.Common;

namespace Maono.Api.Middleware;

/// <summary>
/// Catches ALL non-success responses and wraps them in ApiResponse format.
/// Handles: exceptions (validation, auth, not found, 500) AND non-exception status codes
/// (404 routing, 405 method not allowed, 415 unsupported media, etc.)
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            // After pipeline — catch non-exception error status codes
            // (e.g. 404 from routing, 405 method not allowed, 415 unsupported media type)
            // Only intercept if the body hasn't been written yet (no controller handled it)
            if (!context.Response.HasStarted && context.Response.StatusCode >= 400)
            {
                await HandleNonExceptionStatusCode(context);
            }
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Erreur de validation");

            var errors = ex.Errors
                .Select(e => new ApiError(e.PropertyName, e.ErrorMessage))
                .ToList();

            await WriteErrorResponse(context, 400, "Erreur de validation.", errors);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Accès non autorisé");

            await WriteErrorResponse(context, 403,
                ex.Message.Length > 0 ? ex.Message : "Accès interdit.",
                "forbidden");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Ressource introuvable");

            await WriteErrorResponse(context, 404,
                ex.Message.Length > 0 ? ex.Message : "Ressource introuvable.",
                "not_found");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Opération invalide");

            await WriteErrorResponse(context, 400,
                ex.Message.Length > 0 ? ex.Message : "Opération invalide.",
                "invalid_operation");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argument invalide");

            await WriteErrorResponse(context, 400,
                ex.Message.Length > 0 ? ex.Message : "Paramètre invalide.",
                "invalid_argument");
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout");

            await WriteErrorResponse(context, 504,
                "Le service a mis trop de temps à répondre. Veuillez réessayer.",
                "timeout");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception non gérée");

            var isDev = context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment();

            await WriteErrorResponse(context, 500,
                isDev ? ex.Message : "Une erreur inattendue s'est produite.",
                "internal_error");
        }
    }

    /// <summary>
    /// Handles HTTP error status codes that were set by the framework (not by controllers).
    /// Examples: 404 (no route matched), 405 (method not allowed), 415 (unsupported media type).
    /// </summary>
    private static async Task HandleNonExceptionStatusCode(HttpContext context)
    {
        var statusCode = context.Response.StatusCode;

        var (message, errorCode) = statusCode switch
        {
            400 => ("Requête invalide.", "bad_request"),
            401 => ("Authentification requise.", "unauthorized"),
            403 => ("Accès interdit. Vous n'avez pas les permissions nécessaires.", "forbidden"),
            404 => ("La ressource demandée est introuvable.", "not_found"),
            405 => ("Méthode HTTP non autorisée pour cette ressource.", "method_not_allowed"),
            406 => ("Format de réponse non acceptable.", "not_acceptable"),
            408 => ("Délai d'attente de la requête dépassé.", "request_timeout"),
            409 => ("Conflit avec l'état actuel de la ressource.", "conflict"),
            413 => ("La taille de la requête dépasse la limite autorisée.", "payload_too_large"),
            415 => ("Type de contenu non supporté.", "unsupported_media_type"),
            422 => ("Entité non traitable.", "unprocessable_entity"),
            429 => ("Trop de requêtes. Veuillez patienter.", "too_many_requests"),
            >= 500 => ("Erreur serveur interne.", "server_error"),
            _ => ($"Erreur {statusCode}.", "error")
        };

        await WriteErrorResponse(context, statusCode, message, errorCode);
    }

    /// <summary>
    /// Writes a structured ApiResponse error to the HTTP response body.
    /// Guards against double-write if response has already started.
    /// </summary>
    private static async Task WriteErrorResponse(HttpContext context, int statusCode, string message, string errorCode)
    {
        if (context.Response.HasStarted) return;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object>.Error(message, statusCode, errorCode);
        await context.Response.WriteAsJsonAsync(response);
    }

    private static async Task WriteErrorResponse(HttpContext context, int statusCode, string message, List<ApiError> errors)
    {
        if (context.Response.HasStarted) return;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object>.Error(message, statusCode, errors);
        await context.Response.WriteAsJsonAsync(response);
    }
}
