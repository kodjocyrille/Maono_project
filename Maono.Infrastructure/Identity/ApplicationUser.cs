using Microsoft.AspNetCore.Identity;

namespace Maono.Infrastructure.Identity;

/// <summary>
/// ASP.NET Core Identity user. Extended with application-specific fields.
/// Links to Domain User entity via the Id.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public Guid? DomainUserId { get; set; }
}
