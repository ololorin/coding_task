using CurrencyConverter.Domain.Spelling;

namespace CurrencyConverter.Domain;

/// <summary>
/// The single conversion service. It validates the amount, splits it into dollars and
/// cents, and assembles the final sentence once for every language, delegating only the
/// language-specific words to the matching <see cref="INumberSpeller"/>.
/// </summary>
public sealed class CurrencyToWordsConverter : ICurrencyConverter
{
    /// <summary>Largest amount allowed: 999,999,999 dollars and 99 cents.</summary>
    public const decimal MaxAmount = 999_999_999.99m;

    private readonly IReadOnlyDictionary<string, INumberSpeller> _spellers;

    public CurrencyToWordsConverter(IEnumerable<INumberSpeller> spellers)
    {
        _spellers = spellers.ToDictionary(s => s.Language, StringComparer.OrdinalIgnoreCase);
    }

    public string Convert(decimal amount, string languageCode)
    {
        Validate(amount);

        var speller = ResolveSpeller(languageCode);

        var dollars = (long)decimal.Truncate(amount);
        var cents = (int)decimal.Round((amount - dollars) * 100m);

        var result = $"{speller.SpellInteger(dollars)} {speller.DollarNoun(dollars)}";

        if (cents > 0)
        {
            result += $" {speller.And} {speller.SpellInteger(cents)} {speller.CentNoun(cents)}";
        }

        return result;
    }

    private static void Validate(decimal amount)
    {
        if (amount < 0)
        {
            throw new CurrencyConversionException("The amount must not be negative.");
        }

        if (amount > MaxAmount)
        {
            throw new CurrencyConversionException($"The amount must not exceed {MaxAmount}.");
        }

        if (decimal.Round(amount, 2) != amount)
        {
            throw new CurrencyConversionException("The amount must not have more than two decimal places.");
        }
    }

    private INumberSpeller ResolveSpeller(string languageCode)
    {
        if (!string.IsNullOrWhiteSpace(languageCode) && _spellers.TryGetValue(languageCode, out var speller))
        {
            return speller;
        }

        return _spellers[SupportedLanguages.Default];
    }
}
