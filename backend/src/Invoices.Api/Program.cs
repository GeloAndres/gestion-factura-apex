using System.Text.Json.Serialization;
using Invoices.Api.Auth;
using Invoices.Api.Common;
using Invoices.Api.Startup;
using Invoices.Application;
using Invoices.Application.Common;
using Invoices.Infrastructure;
using Invoices.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Formatters;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure();

builder.Services.AddOptions<DatabaseOptions>()
    .Configure<IConfiguration, IHostEnvironment>((options, configuration, environment) =>
    {
        options.ConnectionString = configuration.GetConnectionString("Invoices") ?? string.Empty;
        options.BasePath = environment.ContentRootPath;
    })
    .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "Falta ConnectionStrings:Invoices.")
    .ValidateOnStart();
builder.Services.AddOptions<BusinessCalendarOptions>()
    .Bind(builder.Configuration.GetSection(BusinessCalendarOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<ImportOptions>()
    .Bind(builder.Configuration.GetSection(ImportOptions.SectionName));

builder.Services
    .AddControllers(options =>
    {
        options.OutputFormatters.RemoveType<StringOutputFormatter>();
        foreach (var formatter in options.OutputFormatters.OfType<SystemTextJsonOutputFormatter>())
        {
            formatter.SupportedMediaTypes.Remove("text/json");
        }
    })
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()));
builder.Services.AddApiAuthentication(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = "Invoices API";
    options.EnablePersistAuthorization();
});

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

await app.InitializeDatabaseAsync();
await app.RunAsync();

public partial class Program;
