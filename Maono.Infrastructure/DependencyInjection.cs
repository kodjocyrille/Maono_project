using Maono.Domain.Common;
using Maono.Domain.Identity.Repository;
using Maono.Domain.Campaigns.Repository;
using Maono.Domain.Clients.Repository;
using Maono.Domain.Content.Repository;
using Maono.Domain.Missions.Repository;
using Maono.Domain.Publications.Repository;
using Maono.Domain.Approval.Repository;
using Maono.Domain.Assets.Repository;
using Maono.Domain.Notifications.Repository;
using Maono.Domain.Performance.Repository;
using Maono.Domain.Planning.Repository;
using System.Text;
using Amazon.S3;
using Maono.Application.Common.Interfaces;
using Maono.Infrastructure.Authentication;
using Maono.Infrastructure.Identity;
using Maono.Infrastructure.Odoo;
using Maono.Infrastructure.Persistence;
using Maono.Infrastructure.Services;
using Maono.Infrastructure.Storage;
using Maono.Infrastructure.Workers;
using Maono.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Maono.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddDbContext<MaonoDbContext>((sp, options) =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        // Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<MaonoDbContext>()
        .AddDefaultTokenProviders();

        // JWT
        var jwtSettings = new JwtSettings();
        configuration.Bind(JwtSettings.SectionName, jwtSettings);
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        if (string.IsNullOrWhiteSpace(jwtSettings.Secret) || jwtSettings.Secret.Length < 32)
            throw new InvalidOperationException(
                $"JWT Secret is missing or too short ({jwtSettings.Secret?.Length ?? 0} chars). " +
                "Configure 'Jwt:Secret' in appsettings.json with at least 32 characters.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = System.Security.Claims.ClaimTypes.Role
            };

            options.Events = new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    // 401 — pas authentifié ou token invalide/expiré
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    var json = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        success = false,
                        statusCode = 401,
                        message = "Authentification requise. Veuillez fournir un jeton JWT valide.",
                        errors = new[] { new { code = "unauthorized", message = context.ErrorDescription ?? "Jeton manquant ou expiré." } }
                    });
                    await context.Response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes(json));
                },
                OnForbidden = async context =>
                {
                    // 403 — authentifié mais pas les droits
                    context.Response.StatusCode = 403;
                    context.Response.ContentType = "application/json";
                    var json = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        success = false,
                        statusCode = 403,
                        message = "Accès interdit. Vous n'avez pas les permissions nécessaires pour cette action.",
                        errors = new[] { new { code = "forbidden", message = "Rôle insuffisant pour cette ressource." } }
                    });
                    await context.Response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes(json));
                }
            };
        });

        // MinIO / S3 Storage
        services.Configure<MinioStorageSettings>(configuration.GetSection(MinioStorageSettings.SectionName));
        var storageSettings = new MinioStorageSettings();
        configuration.Bind(MinioStorageSettings.SectionName, storageSettings);

        services.AddSingleton<IAmazonS3>(_ =>
        {
            var config = new AmazonS3Config
            {
                ServiceURL = storageSettings.ServiceUrl,
                ForcePathStyle = true,
                UseHttp = storageSettings.ServiceUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            };
            return new AmazonS3Client(storageSettings.AccessKey, storageSettings.SecretKey, config);
        });

        // Odoo
        services.Configure<OdooSettings>(configuration.GetSection(OdooSettings.SectionName));
        services.AddHttpClient<IOdooBillingService, OdooBillingService>((sp, client) =>
        {
            var settings = new OdooSettings();
            configuration.Bind(OdooSettings.SectionName, settings);
            client.BaseAddress = new Uri(settings.BaseUrl.TrimEnd('/'));
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        });
        services.AddHttpClient<IOdooContactSyncService, OdooContactSyncService>((sp, client) =>
        {
            var settings = new OdooSettings();
            configuration.Bind(OdooSettings.SectionName, settings);
            client.BaseAddress = new Uri(settings.BaseUrl.TrimEnd('/'));
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        });

        // Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IAssetStorageService, MinioAssetStorageService>();
        services.AddScoped<IAuditWriter, AuditWriter>();
        services.AddScoped<IUnitOfWork, Persistence.UnitOfWork>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IInviteTokenService, Invitations.JwtInviteTokenService>();

        // Repositories
        services.AddScoped<IUserRepository, Persistence.Repositories.UserRepository>();
        services.AddScoped<IWorkspaceRepository, Persistence.Repositories.WorkspaceRepository>();
        services.AddScoped<ISessionRepository, Persistence.Repositories.SessionRepository>();
        services.AddScoped<IRefreshTokenRepository, Persistence.Repositories.RefreshTokenRepository>();
        services.AddScoped<ICampaignRepository, Persistence.Repositories.CampaignRepository>();
        services.AddScoped<IClientRepository, Persistence.Repositories.ClientRepository>();
        services.AddScoped<IContentRepository, Persistence.Repositories.ContentRepository>();
        services.AddScoped<IPublicationRepository, Persistence.Repositories.PublicationRepository>();
        services.AddScoped<IMissionRepository, Persistence.Repositories.MissionRepository>();
        services.AddScoped<IApprovalRepository, Persistence.Repositories.ApprovalRepository>();
        services.AddScoped<IAssetRepository, Persistence.Repositories.AssetRepository>();
        services.AddScoped<IAssetVersionRepository, Persistence.Repositories.AssetVersionRepository>();
        services.AddScoped<INotificationRepository, Persistence.Repositories.NotificationRepository>();
        services.AddScoped<IPerformanceRepository, Persistence.Repositories.PerformanceRepository>();
        services.AddScoped<ICalendarRepository, Persistence.Repositories.CalendarRepository>();
        services.AddScoped<IContentMessageRepository, Persistence.Repositories.ContentMessageRepository>();
        services.AddScoped<IMembershipRepository, Persistence.Repositories.MembershipRepository>();
        services.AddScoped<IContentTaskRepository, Persistence.Repositories.ContentTaskRepository>();
        services.AddScoped<IAssetUploadSessionRepository, Persistence.Repositories.AssetUploadSessionRepository>();
        services.AddScoped<IPortalAccessTokenRepository, Persistence.Repositories.PortalAccessTokenRepository>();
        services.AddScoped<IRoleRepository, Persistence.Repositories.RoleRepository>();
        services.AddScoped<IPermissionRepository, Persistence.Repositories.PermissionRepository>();

        // Open generic repository for entities without dedicated repos (ECR-011, ECR-017)
        services.AddScoped(typeof(IGenericRepository<>), typeof(Persistence.Repositories.GenericRepository<>));

        services.AddHttpContextAccessor();

        // Health Checks
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("DefaultConnection")!, name: "postgresql");

        // Background Workers
        services.AddHostedService<DeadlineEscalationWorker>();
        services.AddHostedService<ApprovalReminderWorker>();
        services.AddHostedService<ScheduledPublicationWorker>();
        services.AddHostedService<PublicationRetryWorker>();
        services.AddHostedService<MetricsCollectionWorker>();
        services.AddHostedService<OdooSyncWorker>();

        return services;
    }
}
