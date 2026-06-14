namespace CurrencyConverter.Domain;

/// <summary>
/// Converts a dollar amount into its written-out words in a supported language.
/// </summary>
public interface ICurrencyConverter
{
    /// <summary>
    /// Converts <paramref name="amount"/> (0..999,999,999.99, at most two decimals) into words.
    /// </summary>
    /// <param name="amount">The amount in dollars; the fractional part is interpreted as cents.</param>
    /// <param name="languageCode">A two-letter ISO 639-1 code; unsupported codes fall back to the default language.</param>
    /// <exception cref="CurrencyConversionException">The amount is negative, too large, or has more than two decimals.</exception>
    string Convert(decimal amount, string languageCode);
}
