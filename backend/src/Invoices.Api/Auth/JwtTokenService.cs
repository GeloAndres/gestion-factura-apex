using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Invoices.Api.Auth;

public sealed record IssuedToken(string AccessToken, DateTimeOffset ExpiresAt);

public sealed class JwtTokenService(IOptions<AuthOptions> options)
{
    public bool ValidateCredentials(string username, string password)
    {
        var user = options.Value.Users.FirstOrDefault(candidate =>
            string.Equals(candidate.Username, username, StringComparison.OrdinalIgnoreCase));
        return user is not null && PasswordHasher.Verify(password, user.PasswordHash);
    }

    public IssuedToken CreateToken(string username)
    {
        var auth = options.Value;

        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(auth.TokenLifetimeMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = auth.Issuer,
            Audience = auth.Audience,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = username,
                [JwtRegisteredClaimNames.Name] = username,
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString("N"),
            },
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(CreateSigningKey(auth), SecurityAlgorithms.HmacSha256),
        };

        return new IssuedToken(new JsonWebTokenHandler().CreateToken(descriptor), expiresAt);
    }

    public static TokenValidationParameters CreateValidationParameters(AuthOptions auth) => new()
    {
        ValidateIssuer = true,
        ValidIssuer = auth.Issuer,
        ValidateAudience = true,
        ValidAudience = auth.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = CreateSigningKey(auth),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30),
        NameClaimType = JwtRegisteredClaimNames.Name,
    };

    private static SymmetricSecurityKey CreateSigningKey(AuthOptions auth) =>
        new(Encoding.UTF8.GetBytes(auth.SigningKey));
}
