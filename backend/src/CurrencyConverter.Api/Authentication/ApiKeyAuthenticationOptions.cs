using Microsoft.AspNetCore.Authentication;

namespace CurrencyConverter.Api.Authentication;

/// <summary>
/// Options for the API-key authentication scheme.
/// </summary>
public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "ApiKey";

    /// <summary>The request header carrying the API key.</summary>
    public const string HeaderName = "X-Api-Key";

    /// <summary>The expected API key. Configured from app settings.</summary>
    public string ApiKey { get; set; } = string.Empty;
}
