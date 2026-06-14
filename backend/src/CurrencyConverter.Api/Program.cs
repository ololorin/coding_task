using System.Globalization;
using CurrencyConverter.Api.Authentication;
using CurrencyConverter.Domain;
using CurrencyConverter.Domain.Spelling;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "frontend";

// --- Conversion services (one converter, one speller per language) ---
builder.Services.AddSingleton<INumberSpeller, EnglishNumberSpeller>();
builder.Services.AddSingleton<INumberSpeller, GermanNumberSpeller>();
builder.Services.AddSingleton<ICurrencyConverter, CurrencyToWordsConverter>();

// --- Localization: resolve language from Accept-Language, echo it in Content-Language ---
var supportedCultures = SupportedLanguages.All.Select(code => new CultureInfo(code)).ToArray();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(SupportedLanguages.Default);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.ApplyCurrentCultureToResponseHeaders = true;
});

// --- API-key authentication (scaffolded; the convert endpoint stays anonymous) ---
builder.Services
    .AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationOptions.DefaultScheme,
        options => options.ApiKey = builder.Configuration["ApiKey"] ?? string.Empty);
builder.Services.AddAuthorization();

// --- CORS for the React dev client ---
builder.Services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
    policy
        .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithExposedHeaders("Content-Language")));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRequestLocalization();
app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposed for the E2E test project (WebApplicationFactory<Program>).
public partial class Program;
