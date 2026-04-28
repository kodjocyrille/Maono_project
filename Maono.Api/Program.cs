using Maono.Application;
using Maono.Infrastructure;
using Maono.Api.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Authorization policies — ECR-007: permission-based (claims injected in JWT by JwtTokenService)
builder.Services.AddAuthorization(options =>
{
    // Admin — full access
    options.AddPolicy("AdminOnly",
        p => p.RequireClaim("permission", "admin.full"));

    // Content management
    options.AddPolicy("CanManageContent",
        p => p.RequireClaim("permission", "content.create"));

    // Publishing
    options.AddPolicy("CanPublish",
        p => p.RequireClaim("permission", "publications.publish"));

    // Internal approval
    options.AddPolicy("CanApprove",
        p => p.RequireClaim("permission", "approvals.review"));

    // Analytics
    options.AddPolicy("CanViewAnalytics",
        p => p.RequireClaim("permission", "performance.read"));

    // Assets — upload & manage
    options.AddPolicy("CanManageAssets",
        p => p.RequireClaim("permission", "assets.upload"));

    // Planning — calendar management
    options.AddPolicy("CanManagePlanning",
        p => p.RequireClaim("permission", "planning.create"));

    // Missions — freelance mode
    options.AddPolicy("CanManageMissions",
        p => p.RequireClaim("permission", "missions.create"));

    // Workspace — member & role management
    options.AddPolicy("CanManageWorkspace",
        p => p.RequireClaim("permission", "workspace.manage"));
});

// Controllers & OpenAPI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<Maono.Api.OpenApi.BearerSecuritySchemeTransformer>();
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Auto-migrate + seed on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Maono.Infrastructure.Persistence.MaonoDbContext>();
    await db.Database.MigrateAsync();
}
await Maono.Infrastructure.Seeding.DatabaseSeeder.SeedAsync(app.Services);

// Middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    // Expose OpenAPI JSON document at /openapi/v1.json
    app.MapOpenApi();

    // Swagger UI — pointed at the built-in OpenAPI document
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Maono API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("/api/admin/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            totalDuration = $"{report.TotalDuration.TotalMilliseconds:F0}ms",
            timestamp = DateTime.UtcNow,
            services = report.Entries.ToDictionary(
                e => e.Key,
                e => new
                {
                    status = e.Value.Status.ToString(),
                    duration = $"{e.Value.Duration.TotalMilliseconds:F0}ms",
                    error = e.Value.Exception?.Message
                })
        });
        await context.Response.WriteAsync(result);
    }
}).RequireAuthorization("AdminOnly");

app.Run();
