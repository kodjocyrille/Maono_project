using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Maono.Api.OpenApi;

/// <summary>
/// Adds Bearer token security scheme to the OpenAPI document.
/// This enables the "Authorize" 🔒 button in Swagger UI.
/// </summary>
public sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    private readonly IAuthenticationSchemeProvider _authSchemeProvider;

    public BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authSchemeProvider)
    {
        _authSchemeProvider = authSchemeProvider;
    }

    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken ct)
    {
        var authSchemes = await _authSchemeProvider.GetAllSchemesAsync();
        if (authSchemes.Any(s => s.Name == "Bearer"))
        {
            var securityScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter your JWT token. Example: eyJhbGciOiJIUzI1NiIs..."
            };

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes["Bearer"] = securityScheme;

            var schemeRef = new OpenApiSecuritySchemeReference("Bearer", document);

            if (document.Paths != null)
            {
                foreach (var pathItem in document.Paths.Values)
                {
                    if (pathItem.Operations == null) continue;
                    foreach (var operation in pathItem.Operations.Values)
                    {
                        operation.Security ??= [];
                        operation.Security.Add(new OpenApiSecurityRequirement
                        {
                            [schemeRef] = new List<string>()
                        });
                    }
                }
            }
        }
    }
}
