namespace Maono.Domain.Identity.Enums;

/// <summary>
/// ECR-022 — Freelance profile type per CdCF §3.2.
/// Determines which tasks and features are accessible in freelance mode.
/// </summary>
public enum FreelanceProfile
{
    None = 0,
    CommunityManager = 1,
    GraphicDesigner = 2,
    Videographer = 3,
    Photographer = 4,
    Copywriter = 5
}
