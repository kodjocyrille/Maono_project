namespace Maono.Infrastructure.Odoo;

public class OdooSettings
{
    public const string SectionName = "Odoo";
    public string BaseUrl { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}
