namespace Maono.Application.Features.Clients.DTOs;

public record ClientDto(
    Guid Id,
    string Name,
    string? LegalName,
    string? BillingEmail,
    string? Phone,
    int ContactCount,
    DateTime CreatedAtUtc
);

public record ClientDetailDto(
    Guid Id,
    string Name,
    string? LegalName,
    string? BillingEmail,
    string? Phone,
    string? Notes,
    DateTime CreatedAtUtc,
    List<ClientContactDto> Contacts,
    BrandProfileDto? BrandProfile
);

public record ClientContactDto(Guid Id, string FullName, string? Email, string? Phone, string? Position, bool IsPrimaryApprover);
public record BrandProfileDto(Guid Id, string? BrandTone, string? Palette, string? LogoUrl);
