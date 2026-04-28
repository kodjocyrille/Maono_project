using Maono.Domain.Content.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Content.Entities;

/// <summary>
/// Reusable taxonomy tag within a workspace.
/// </summary>
public class Tag : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Category { get; set; }
}
