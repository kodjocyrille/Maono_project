# Maono API — Source de Vérité

> **Document vivant** — Mis à jour à chaque implémentation  
> Dernière mise à jour : Avril 2026  
> Ref : `maono_architecture_aspnet_detaillee_final.docx` + `USER_GUIDE.md`

---

## Table des matières

1. [Architecture & Principes](#1-architecture--principes)
2. [État actuel de l'API](#2-état-actuel-de-lapi)
3. [Format de réponse unifié](#3-format-de-réponse-unifié)
4. [Pipeline CQRS](#4-pipeline-cqrs)
5. [Sécurité & Rôles](#5-sécurité--rôles)
6. [Backlog d'implémentation](#6-backlog-dimplémentation)
7. [Détail des points à implémenter](#7-détail-des-points-à-implémenter)

---

## 1. Architecture & Principes

### Stack technique
| Composant | Technologie |
|---|---|
| Backend | ASP.NET Core 10 — Monolithe modulaire |
| Base de données | PostgreSQL (local : `localhost:5432/maono_db`) |
| Stockage fichiers | MinIO S3-compatible (`localhost:9000`) |
| ORM | EF Core 10 + Npgsql |
| Auth | ASP.NET Core Identity + JWT Bearer |
| CQRS | MediatR + FluentValidation |
| API Docs | Swagger UI (`/swagger`) + OpenAPI natif .NET 10 |
| Frontend | Next.js (repo séparé — non encore créé) |

### Structure de solution
```
Maono.Domain/         → Entités, agrégats, value objects, enums, domain events
Maono.Application/    → CQRS, handlers, DTOs, validators, interfaces
Maono.Infrastructure/ → EF Core, Identity, JWT, MinIO, repositories
Maono.Api/            → Controllers, middleware, OpenAPI, Program.cs
```

### Règles de dépendance
```
Domain       ← (aucune dépendance)
Application  ← Domain
Infrastructure ← Domain + Application
Api          ← Application + Infrastructure
```

### Principes fondamentaux
- **Aucun contrôleur n'accède directement à la DB** — tout passe par MediatR
- **Multi-tenant** : chaque entité tenant-owned porte un `WorkspaceId` + Global Query Filter EF
- **Soft-delete** pour les entités métier importantes (campagnes, contenus, clients)
- **Domain Events** ignorés par EF Core (exclus du modèle via `Ignore<DomainEvent>()`)

---

## 2. État actuel de l'API

### Contrôleurs implémentés

| Contrôleur | Auth | Endpoints | Statut |
|---|---|---|---|
| `AuthController` | Public/🔒 mixte | register, login, refresh, sessions (×4) | ✅ Complet |
| `WorkspacesController` | 🔒 Bearer | POST, GET list, GET {id}, PATCH settings | ✅ Complet |
| `ClientsController` | 🔒 Bearer | POST, GET list, GET {id}, PUT, DELETE | ✅ Complet |
| `CampaignsController` | 🔒 Bearer | POST, GET list, GET {id}, PUT, DELETE | ✅ Complet |
| `ContentsController` | 🔒 Bearer | POST, GET list, GET {id}, GET deadline, PUT, PATCH status, DELETE | ✅ Complet |
| `PublicationsController` | 🔒 Bearer | GET list, GET {id}, POST schedule, POST publish, POST retry, DELETE | ✅ Complet |
| `ApprovalsController` | 🔒 Bearer | POST internal, POST client, GET cycles | ✅ Complet |
| `AssetsController` | 🔒 Bearer | GET {id}, GET versions, POST restore | ⚠️ Lecture seule |
| `MessagesController` | 🔒 Bearer | GET by content, POST send | ✅ Complet |
| `NotificationsController` | 🔒 Bearer | GET list, POST read, POST read-all | ✅ Complet |
| `MissionsController` | 🔒 Bearer | POST, GET list, GET {id}, PUT, PATCH status, DELETE | ✅ Complet |
| `PerformanceController` | 🔒 Bearer | GET by publication, GET by campaign, GET summary | ✅ Complet |
| `CalendarController` | 🔒 Bearer | GET list, GET capacity | ⚠️ Lecture seule |

### Ce qui est en place (Infrastructure)

- ✅ MaonoDbContext avec Global Query Filter WorkspaceId
- ✅ Migration initiale `InitialCreate`
- ✅ `DatabaseSeeder` — admin bootstrap + rôles au démarrage
- ✅ JWT Bearer + ASP.NET Core Identity (`ApplicationUser`)
- ✅ Pipeline : `LoggingBehavior → ValidationBehavior → TransactionBehavior`
- ✅ `ExceptionHandlingMiddleware` avec format unifié
- ✅ `CurrentUserService` (UserId, WorkspaceId)
- ✅ `BearerSecuritySchemeTransformer` (bouton Authorize dans Swagger)
- ✅ Auto-migrate au démarrage
- ✅ `MaonoDbContextFactory` pour les migrations EF CLI

---

## 3. Format de réponse unifié

**Garanties : `message` jamais null, `statusCode` toujours présent**

### Succès
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Opération réussie",
  "data": { ... }
}
```

### Création (201)
```json
{
  "success": true,
  "statusCode": 201,
  "message": "Ressource créée",
  "data": { ... }
}
```

### Erreur
```json
{
  "success": false,
  "statusCode": 400,
  "message": "Identifiant ou mot de passe incorrect.",
  "errors": [
    { "code": "auth_failed", "message": "Identifiant ou mot de passe incorrect." }
  ]
}
```

> `data` → présent uniquement en succès  
> `errors` → présent uniquement en erreur  
> Classe : `Maono.Api/Common/ApiResponse.cs`

---

## 4. Pipeline CQRS

```
HTTP Request
  → Controller
  → MediatR.Send(command | query)
    → LoggingBehavior        (toutes les requêtes)
    → ValidationBehavior     (toutes les requêtes — FluentValidation)
    → TransactionBehavior    (ICommand uniquement — UoW + SaveChanges auto)
    → Handler
      → Repository
      → DB / MinIO / Services externes
  ← Result<T>
  → Controller → ApiResponse<T> → HTTP Response
```

**ICommand** = écriture → passe par TransactionBehavior (transaction auto)  
**IQuery** = lecture → court-circuite TransactionBehavior

---

## 5. Sécurité & Rôles

### Rôles définis (USER_GUIDE)

| Rôle | Description |
|---|---|
| `ADMIN` | Accès total — gestion users, intégrations, paramètres |
| `STRATEGIST` | Stratégie, campagnes, analytics |
| `CONTENT_PLANNER` | Calendrier, campagnes, contenus |
| `DESIGNER` | Contenus assignés |
| `PHOTOGRAPHER` | Contenus assignés |
| `VIDEOGRAPHER` | Contenus assignés |
| `SOCIAL_MEDIA_MANAGER` | File de publication |
| `CLIENT` | Portail client uniquement (accès par token) |

### Policies (à implémenter — Point 8)

| Policy | Rôles autorisés |
|---|---|
| `AdminOnly` | ADMIN |
| `CanManageContent` | ADMIN, STRATEGIST, CONTENT_PLANNER |
| `CanPublish` | ADMIN, SOCIAL_MEDIA_MANAGER |
| `CanApprove` | ADMIN, STRATEGIST, CONTENT_PLANNER |
| `CanViewAnalytics` | ADMIN, STRATEGIST |

> **État actuel** : tous les endpoints n'ont que `[Authorize]` — pas de vérification de rôle.

### Configuration JWT (`appsettings.json`)
```json
"Jwt": {
  "Secret": "MaonoSuperSecretKey2026!AtLeast32CharactersLong!",
  "Issuer": "Maono",
  "Audience": "MaonoApp",
  "AccessTokenExpiryMinutes": 60,
  "RefreshTokenExpiryDays": 7
}
```

---

## 6. Backlog d'implémentation

> **Légende** : 🔴 Bloquant | 🟡 Important | 🟢 V2  
> **Statut** : ⬜ À faire | 🔄 En cours | ✅ Fait

### Priorité 1 — Bloquants

| # | Point | Effort | Statut |
|---|---|---|---|
| P1 | Upload d'assets (Presigned URL + SHA-256) | ~2j | ✅ |
| P2 | Portail client (accès public par token) | ~2j | ✅ |
| P3 | Tâches de production par contenu | ~1j | ✅ |
| P4 | Memberships workspace | ~1j | ✅ |

### Priorité 2 — Importants

| # | Point | Effort | Statut |
|---|---|---|---|
| P5 | Calendrier en écriture (POST/PATCH/DELETE/validate) | ~0.5j | ✅ |
| P6 | Gestion des utilisateurs Admin (`AdminUsersController`) | ~1j | ✅ |
| P7 | KPI targets dans les campagnes | ~0.5j | ✅ |
| P8 | Policies d'autorisation par rôle | ~1j | ✅ |
| P9 | Santé des services détaillée | ~0.5j | ✅ |

### Priorité 3 — V2

| # | Point | Effort | Statut |
|---|---|---|---|
| V1 | Missions avancées (Milestones, Deliveries, BillingRecord) | ~3j | ⬜ |
| V2 | Workers background (publication planifiée, escalades) | ~3j | ⬜ |
| V3 | Analytics enrichi (12 mois, by-platform) | ~1j | ⬜ |
| V4 | Odoo billing en périphérie | ~2j | ⬜ |
| V5 | RLS PostgreSQL sur tables sensibles | ~1j | ⬜ |
| V6 | Audit logs complets | ~2j | ⬜ |
| V7 | Outbox pattern (intégration events) | ~2j | ⬜ |
| V8 | URLs signées pour assets privés | ~0.5j | ⬜ |

---

## 7. Détail des points à implémenter

---

### P1 — 📎 Upload d'assets (Presigned URL + SHA-256)

**Pourquoi :** Fichiers jusqu'à 1 Go+ — l'API ne peut pas être un proxy. Upload direct vers MinIO, l'API ne fait qu'orchestrer et valider l'intégrité.

**Workflow :**
```
1. Client → POST /api/assets/upload-session  { fileName, fileSize, mimeType, contentId, sha256 }
   API    → Génère presignedUrl MinIO (TTL 15min) + crée AssetUploadSession (Pending)
   ← { sessionId, presignedUrl, expiresAt }

2. Client → PUT presignedUrl (fichier brut, header x-amz-checksum-sha256)
   MinIO  ← 200 OK (vérifie SHA-256 à la réception)

3. Client → POST /api/assets/upload-session/{sessionId}/confirm  { sha256, actualSizeBytes }
   API    → HEAD object MinIO → compare size + SHA-256
   API    → Crée Asset + AssetVersion, session → Confirmed
   ← { assetId, path, version }
```

**Endpoints :**
```
POST  /api/assets/upload-session
GET   /api/assets/upload-session/{sessionId}
POST  /api/assets/upload-session/{sessionId}/confirm
```

**Entité à créer :** `AssetUploadSession`
```csharp
public class AssetUploadSession : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Guid ContentItemId { get; set; }
    public Guid InitiatedByUserId { get; set; }
    public string FileName { get; set; }
    public string MimeType { get; set; }
    public long DeclaredSizeBytes { get; set; }
    public string DeclaredSha256 { get; set; }     // hex 64 chars
    public string StorageKey { get; set; }          // {workspaceId}/{contentId}/{sessionId}/{fileName}
    public string PresignedUrl { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public UploadSessionStatus Status { get; set; } // Pending | Confirmed | Expired | Failed
    public string? FailureReason { get; set; }
    public DateTime? ConfirmedAtUtc { get; set; }
}
public enum UploadSessionStatus { Pending, Confirmed, Expired, Failed }
```

**Interface :**
```csharp
public interface IAssetStorageService
{
    Task<PresignedUrlResult> GeneratePresignedPutUrlAsync(string key, string mime, TimeSpan ttl, CancellationToken ct);
    Task<ObjectMetadata?> GetObjectMetadataAsync(string key, CancellationToken ct);
    Task<string> GeneratePresignedGetUrlAsync(string key, TimeSpan ttl, CancellationToken ct);
    Task DeleteObjectAsync(string key, CancellationToken ct);
}
public record PresignedUrlResult(string Url, DateTime ExpiresAt);
public record ObjectMetadata(long SizeBytes, string? ChecksumSha256, string ETag);
```

**Règles :**
- Taille max : 2 GB (configurable `MaxFileSizeMb`)
- TTL session : 15 min (configurable `UploadSessionTtlMinutes`)
- MIME whitelist : `image/*`, `video/*`, `application/pdf`, `audio/*`, `application/zip`
- SHA-256 : 64 chars hex, calculé côté client avant upload

**Checklist :**
- [ ] Entité `AssetUploadSession` + migration EF
- [ ] `IAssetStorageService` (Application/Common/Interfaces)
- [ ] `MinioAssetStorageService` (Infrastructure)
- [ ] `InitiateUploadSessionCommand` + handler + validator
- [ ] `ConfirmUploadSessionCommand` + handler
- [ ] `GetUploadSessionQuery` + handler
- [ ] 3 endpoints dans `AssetsController`
- [ ] Config appsettings (`MaxFileSizeMb`, `UploadSessionTtlMinutes`, `AllowedMimeTypes`)

---

### P2 — 🔗 Portail client (token public)

**Pourquoi :** Les clients doivent pouvoir approuver des contenus sans créer de compte. Accès via URL unique partagée.

**Workflow :**
```
ADMIN → POST /api/clients/{id}/portal-token
      ← { token, portalUrl: "https://app.maono.io/portal/{token}" }

CLIENT (sans JWT) → GET  /api/portal/{token}             → contenus CLIENT_REVIEW
                 → POST /api/portal/{token}/decisions     → approuver / demander corrections
                 → GET  /api/portal/{token}/published     → contenus publiés + métriques
```

**Endpoints :**
```
# Authentifiés (ADMIN ou CONTENT_PLANNER)
POST   /api/clients/{id}/portal-token
DELETE /api/clients/{id}/portal-token

# Portail public [AllowAnonymous]
GET    /api/portal/{token}
POST   /api/portal/{token}/decisions
GET    /api/portal/{token}/published
```

**Entité à créer :** `PortalAccessToken`
```csharp
public class PortalAccessToken : BaseEntity
{
    public Guid ClientOrganizationId { get; set; }
    public Guid WorkspaceId { get; set; }
    public string Token { get; set; }              // Guid.NewGuid().ToString("N")
    public DateTime? ExpiresAtUtc { get; set; }    // null = pas d'expiry
    public bool IsRevoked { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime? LastUsedAtUtc { get; set; }
}
```

**Sécurité :** `PortalController` est `[AllowAnonymous]` — pas de JWT, authentification via token dans l'URL validé à chaque requête.

**Validator :**
- `Decision` : "Approved" | "ChangesRequested"
- `Comment` : requis si `ChangesRequested`, max 2000 chars

**Checklist :**
- [ ] Entité `PortalAccessToken` + migration EF
- [ ] `GeneratePortalTokenCommand` + handler
- [ ] `RevokePortalTokenCommand` + handler
- [ ] `GetPortalContentsQuery` + handler
- [ ] `SubmitPortalDecisionCommand` + handler
- [ ] `GetPortalPublishedQuery` + handler
- [ ] `PortalController` `[AllowAnonymous]` — 3 endpoints
- [ ] 2 endpoints token dans `ClientsController`

---

### P3 — ✅ Tâches de production

**Pourquoi :** Le USER_GUIDE §6 décrit un système de tâches par contenu avec statuts et priorités. Aucun endpoint n'existe.

**Endpoints :**
```
GET    /api/contents/{contentId}/tasks
POST   /api/contents/{contentId}/tasks
PATCH  /api/tasks/{taskId}
DELETE /api/tasks/{taskId}
```

**Entité à créer :** `ContentTask`
```csharp
public class ContentTask : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Guid ContentItemId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public ContentTaskStatus Status { get; set; }    // Pending | InProgress | Completed | Blocked
    public ContentTaskPriority Priority { get; set; } // Low | Medium | High
    public DateTime? DueDate { get; set; }
    public string? BlockedReason { get; set; }        // requis si Status == Blocked
    public DateTime? CompletedAtUtc { get; set; }
}
```

**Règle :** Si `Status → Blocked`, `BlockedReason` est obligatoire.

**Checklist :**
- [ ] Entité `ContentTask` + enums + migration EF
- [ ] `CreateTaskCommand` + handler + validator
- [ ] `UpdateTaskCommand` + handler + validator
- [ ] `DeleteTaskCommand` + handler
- [ ] `ListTasksQuery` + handler
- [ ] `TasksController` — 4 endpoints

---

### P4 — 👥 Memberships workspace

**Pourquoi :** Impossible d'inviter des membres dans un workspace.

**Endpoints :**
```
GET    /api/workspaces/{workspaceId}/members
POST   /api/workspaces/{workspaceId}/members
PATCH  /api/memberships/{membershipId}
DELETE /api/memberships/{membershipId}
```

**Règles :**
- `InviteMember` et `UpdateMemberRole` : `[Authorize(Policy = "AdminOnly")]`
- Impossible d'inviter avec le rôle `ADMIN` directement
- Garde-fou : le dernier ADMIN ne peut pas être retiré ou rétrogradé
- `RemoveMember` : un membre peut se retirer lui-même OU un ADMIN peut retirer n'importe qui

**Checklist :**
- [ ] Vérifier/compléter entité `WorkspaceMembership` + migration si besoin
- [ ] `InviteMemberCommand` + handler + validator
- [ ] `UpdateMemberRoleCommand` + handler + validator
- [ ] `RemoveMemberCommand` + handler
- [ ] `ListMembersQuery` + handler
- [ ] `MembershipsController` — 4 endpoints

---

### P5 — 📅 Calendrier en écriture

**Pourquoi :** `CalendarController` expose uniquement des lectures.

**Endpoints :**
```
POST   /api/calendar
PATCH  /api/calendar/{id}
DELETE /api/calendar/{id}
POST   /api/calendar/{id}/validate
```

**Règle d'unicité :** `WorkspaceId + Platform + PublicationDate` — pas deux posts le même jour sur la même plateforme.

**Checklist :**
- [ ] `CreateCalendarEntryCommand` + handler + validator
- [ ] `UpdateCalendarEntryCommand` + handler + validator
- [ ] `DeleteCalendarEntryCommand` + handler
- [ ] `ValidateCalendarEntryCommand` + handler
- [ ] 4 nouveaux endpoints dans `CalendarController`

---

### P6 — 👤 Gestion des utilisateurs Admin

**Pourquoi :** Le §12 du USER_GUIDE décrit la gestion des comptes depuis `/admin/users`.

**Endpoints :**
```
GET    /api/admin/users
POST   /api/admin/users
PATCH  /api/admin/users/{id}
DELETE /api/admin/users/{id}    → Soft-disable (LockoutEnd = MaxValue)
```

**Protection :** `[Authorize(Policy = "AdminOnly")]` sur tout le contrôleur.

**Checklist :**
- [ ] `ListUsersQuery` + handler
- [ ] `CreateUserCommand` + handler + validator
- [ ] `UpdateUserCommand` + handler + validator
- [ ] `DeactivateUserCommand` + handler (révoque sessions actives)
- [ ] `AdminUsersController` — 4 endpoints

---

### P7 — 🎯 KPI Targets dans les campagnes

**Pourquoi :** Le USER_GUIDE dit de définir les KPI avant de créer des contenus — actuellement `Campaign` n'a aucun champ KPI.

**Champs à ajouter sur `Campaign` :**
```csharp
public long? TargetReach { get; set; }
public decimal? TargetCtr { get; set; }
public long? TargetConversions { get; set; }
public decimal? TargetEngagementRate { get; set; }
public string[]? TargetPlatforms { get; set; }
```

**Endpoint :**
```
PATCH  /api/campaigns/{id}/kpi-targets
```

**Checklist :**
- [ ] Champs KPI sur `Campaign` + migration EF
- [ ] Mettre à jour `CreateCampaignCommand` (champs optionnels)
- [ ] Mettre à jour `CampaignDto`
- [ ] `UpdateCampaignKpiTargetsCommand` + handler
- [ ] Endpoint dans `CampaignsController`

---

### P8 — 🔐 Policies d'autorisation par rôle

**Pourquoi :** Actuellement `[Authorize]` seul — n'importe quel rôle peut tout faire.

**Policies à définir dans `Program.cs` :**
```csharp
options.AddPolicy("AdminOnly",         p => p.RequireRole("ADMIN"));
options.AddPolicy("CanManageContent",  p => p.RequireRole("ADMIN", "STRATEGIST", "CONTENT_PLANNER"));
options.AddPolicy("CanPublish",        p => p.RequireRole("ADMIN", "SOCIAL_MEDIA_MANAGER"));
options.AddPolicy("CanApprove",        p => p.RequireRole("ADMIN", "STRATEGIST", "CONTENT_PLANNER"));
options.AddPolicy("CanViewAnalytics",  p => p.RequireRole("ADMIN", "STRATEGIST"));
```

**Matrice d'application :**

| Contrôleur / Action | Policy |
|---|---|
| `AdminUsersController` (tout) | `AdminOnly` |
| `WorkspacesController` POST, PATCH settings | `AdminOnly` |
| `MembershipsController` POST/PATCH/DELETE | `AdminOnly` |
| `CampaignsController` POST/PUT/DELETE | `CanManageContent` |
| `ContentsController` POST/PUT/DELETE | `CanManageContent` |
| `CalendarController` POST/PATCH/DELETE/validate | `CanManageContent` |
| `PublicationsController` POST schedule, publish, retry | `CanPublish` |
| `ApprovalsController` POST internal | `CanApprove` |
| `PerformanceController` (tout) | `CanViewAnalytics` |
| `ClientsController` POST portal-token | `CanManageContent` |

**Checklist :**
- [ ] Définir les 5 policies dans `Program.cs`
- [ ] Appliquer `[Authorize(Policy = "...")]` sur chaque action
- [ ] Vérifier que le JWT inclut bien les claims de rôle à la connexion

---

### P9 — 🩺 Santé des services (Admin)

**Pourquoi :** Le §12 du USER_GUIDE décrit un tableau de santé par service. `/health` actuel retourne uniquement un statut global.

**Endpoints :**
```
GET    /api/admin/health
POST   /api/admin/integrations/sync    → Déclencher une synchronisation manuelle
```

**Package requis :**
```
dotnet add Maono.Api package AspNetCore.HealthChecks.UI.Client
dotnet add Maono.Infrastructure package AspNetCore.HealthChecks.NpgSql
```

**Configuration :**
```csharp
builder.Services.AddHealthChecks()
    .AddNpgsql(connectionString, name: "postgresql")
    .AddUrlGroup(new Uri(minioUrl), name: "minio")
    .AddCheck("api", () => HealthCheckResult.Healthy());

app.MapHealthChecks("/api/admin/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

**Checklist :**
- [ ] Installer packages health checks
- [ ] Configurer `AddHealthChecks()` (PostgreSQL + MinIO)
- [ ] Exposer `GET /api/admin/health` avec JSON détaillé
- [ ] `POST /api/admin/integrations/sync` (stub → V2)

---

## Ordre d'implémentation recommandé

```
Semaine 1 — Bloquants
  Jour 1   : P8 — Policies de rôle (transversal, rapide, impact immédiat)
  Jour 2-3 : P1 — Upload assets (Presigned URL + SHA-256)
  Jour 4   : P4 — Memberships
  Jour 5   : P3 — Tâches de production

Semaine 2 — Bloquants + Importants
  Jour 1-2 : P2 — Portail client
  Jour 3   : P5 — Calendrier en écriture
             P7 — KPI targets (0.5j chacun)
  Jour 4   : P6 — UsersController Admin
  Jour 5   : P9 — Santé des services
```

---

*Document maintenu par l'équipe backend Maono — à mettre à jour après chaque implémentation.*
