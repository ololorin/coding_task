namespace CurrencyConverter.Domain.Spelling;

/// <summary>
/// English (US-style) number spelling: tens and units are hyphenated ("twenty-five"),
/// scale words are invariant and space-separated, and there is no "and" inside a number.
/// </summary>
public sealed class EnglishNumberSpeller : NumberSpellerBase
{
    private static readonly string[] Ones =
    {
        "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
        "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen",
        "seventeen", "eighteen", "nineteen"
    };

    private static readonly string[] Tens =
    {
        "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety"
    };

    public override string Language => SupportedLanguages.English;
    public override string And => "and";
    protected override string Zero => "zero";

    public override string DollarNoun(long dollars) => dollars == 1 ? "dollar" : "dollars";
    public override string CentNoun(int cents) => cents == 1 ? "cent" : "cents";

    protected override string SpellNonZero(long value)
    {
        var (millions, thousands, units) = SplitGroups(value);
        var parts = new List<string>(3);
        if (millions > 0) parts.Add($"{SpellHundreds(millions)} million");
        if (thousands > 0) parts.Add($"{SpellHundreds(thousands)} thousand");
        if (units > 0) parts.Add(SpellHundreds(units));
        return string.Join(" ", parts);
    }

    /// <summary>Spells a value in 1..999, e.g. "one hundred twenty-five".</summary>
    private static string SpellHundreds(int value)
    {
        var hundreds = value / 100;
        var remainder = value % 100;
        var parts = new List<string>(2);
        if (hundreds > 0) parts.Add($"{Ones[hundreds]} hundred");
        if (remainder > 0) parts.Add(SpellBelowHundred(remainder));
        return string.Join(" ", parts);
    }

    private static string SpellBelowHundred(int value)
    {
        if (value < 20) return Ones[value];
        var tens = Tens[value / 10];
        var ones = value % 10;
        return ones == 0 ? tens : $"{tens}-{Ones[ones]}";
    }
}
