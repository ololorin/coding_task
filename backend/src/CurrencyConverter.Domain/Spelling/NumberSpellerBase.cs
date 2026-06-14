namespace CurrencyConverter.Domain.Spelling;

/// <summary>
/// Shared scaffolding for number spellers: the zero special-case and the base-1000
/// decomposition into million/thousand/unit groups. Concrete spellers only implement
/// the language-specific composition of those groups.
/// </summary>
public abstract class NumberSpellerBase : INumberSpeller
{
    public abstract string Language { get; }
    public abstract string And { get; }
    public abstract string DollarNoun(long dollars);
    public abstract string CentNoun(int cents);

    /// <summary>The word for zero ("zero" / "null").</summary>
    protected abstract string Zero { get; }

    /// <summary>Spells a strictly positive number (the zero case is handled by the base).</summary>
    protected abstract string SpellNonZero(long value);

    public string SpellInteger(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        return value == 0 ? Zero : SpellNonZero(value);
    }

    /// <summary>Splits a value (0..999,999,999) into its million, thousand and unit groups (each 0..999).</summary>
    protected static (int Millions, int Thousands, int Units) SplitGroups(long value)
    {
        var millions = (int)(value / 1_000_000);
        var thousands = (int)(value / 1_000 % 1_000);
        var units = (int)(value % 1_000);
        return (millions, thousands, units);
    }
}
