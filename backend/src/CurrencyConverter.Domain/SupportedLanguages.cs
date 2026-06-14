namespace CurrencyConverter.Domain;

/// <summary>
/// Two-letter ISO 639-1 codes for the languages supported by the conversion service.
/// </summary>
public static class SupportedLanguages
{
    public const string English = "en";
    public const string German = "de";

    /// <summary>Language used when the requested one is not supported.</summary>
    public const string Default = English;

    public static readonly IReadOnlyList<string> All = new[] { English, German };
}
