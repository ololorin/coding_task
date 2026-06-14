namespace CurrencyConverter.Domain.Spelling;

/// <summary>
/// Language-specific spelling of whole numbers and the currency vocabulary used to
/// build the final sentence. One implementation per supported language; the shared
/// sentence assembly lives in <see cref="CurrencyToWordsConverter"/> so it is never
/// duplicated across languages.
/// </summary>
public interface INumberSpeller
{
    /// <summary>Two-letter ISO 639-1 language code this speller handles (see <see cref="SupportedLanguages"/>).</summary>
    string Language { get; }

    /// <summary>The connector between the dollars and cents clauses ("and" / "und").</summary>
    string And { get; }

    /// <summary>Spells a whole number in the range 0..999,999,999 in the target language.</summary>
    string SpellInteger(long value);

    /// <summary>The dollar noun in the correct grammatical form for the given amount.</summary>
    string DollarNoun(long dollars);

    /// <summary>The cent noun in the correct grammatical form for the given amount.</summary>
    string CentNoun(int cents);
}
