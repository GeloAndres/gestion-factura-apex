using System.ComponentModel.DataAnnotations;

namespace Invoices.Api.Auth;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Required]
    [MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int TokenLifetimeMinutes { get; set; } = 120;

    public List<AuthUser> Users { get; set; } = [];
}

public sealed class AuthUser
{
    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;
}
