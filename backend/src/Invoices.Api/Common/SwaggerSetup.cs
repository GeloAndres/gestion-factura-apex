using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Invoices.Api.Common;

public static class SwaggerSetup
{
    public const string BearerScheme = "Bearer";

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Invoices API",
                Version = "v1",
                Description =
                    "API REST para gestionar facturas: carga desde JSON, búsqueda, notas de crédito y reportes. " +
                    "Obtén un token en POST /api/auth/login y pégalo en el botón Authorize.",
            });

            var xmlFile = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
            if (File.Exists(xmlFile))
            {
                options.IncludeXmlComments(xmlFile);
            }

            options.AddSecurityDefinition(BearerScheme, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Token JWT obtenido en POST /api/auth/login.",
            });
            options.OperationFilter<AuthorizeOperationFilter>();
            options.SupportNonNullableReferenceTypes();
        });

        return services;
    }
}

internal sealed class AuthorizeOperationFilter : IOperationFilter
{
    private static readonly OpenApiSecurityScheme BearerReference = new()
    {
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = SwaggerSetup.BearerScheme },
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        var requiresAuthorization = metadata.OfType<IAuthorizeData>().Any() && !metadata.OfType<IAllowAnonymous>().Any();
        if (!requiresAuthorization)
        {
            return;
        }

        operation.Security = [new OpenApiSecurityRequirement { [BearerReference] = [] }];
        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Falta el token o no es válido." });
    }
}
