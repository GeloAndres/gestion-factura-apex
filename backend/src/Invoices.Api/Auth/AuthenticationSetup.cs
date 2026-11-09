using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Invoices.Api.Auth;

public static class AuthenticationSetup
{
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AuthOptions>()
            .Bind(configuration.GetSection(AuthOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options => options.Users.Count > 0, "Configura al menos un usuario en Auth:Users.")
            .ValidateOnStart();

        services.AddSingleton<JwtTokenService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<AuthOptions>>((jwt, auth) =>
            {
                jwt.MapInboundClaims = false;
                jwt.TokenValidationParameters = JwtTokenService.CreateValidationParameters(auth.Value);
            });

        services.AddAuthorization();
        return services;
    }
}
