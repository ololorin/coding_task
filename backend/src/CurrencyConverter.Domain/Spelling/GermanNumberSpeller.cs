using System.Text;

namespace CurrencyConverter.Domain.Spelling;

/// <summary>
/// German number spelling. Sub-million numbers are written as a single glued word with
/// units before tens ("fünfundzwanzig"); "Million(en)" is gender/number inflected and
/// space-separated. The attributive form "ein" is always used (never standalone "eins")
/// because every spelled number here is followed by a currency noun.
/// </summary>
public sealed class GermanNumberSpeller : NumberSpellerBase
{
    private static readonly string[] Ones =
    {
        "null", "ein", "zwei", "drei", "vier", "fünf", "sechs", "sieben", "acht", "neun",
        "zehn", "elf", "zwölf", "dreizehn", "vierzehn", "fünfzehn", "sechzehn",
        "siebzehn", "achtzehn", "neunzehn"
    };

    private static readonly string[] Tens =
    {
        "", "", "zwanzig", "dreißig", "vierzig", "fünfzig", "sechzig", "siebzig", "achtzig", "neunzig"
    };

    public override string Language => SupportedLanguages.German;
    public override string And => "und";
    protected override string Zero => "null";

    public override string DollarNoun(long dollars) => "Dollar";
    public override string CentNoun(int cents) => "Cent";

    protected override string SpellNonZero(long value)
    {
        var (millions, thousands, units) = SplitGroups(value);
        var belowMillion = SpellBelowMillion(thousands, units);

        if (millions == 0) return belowMillion;

        var millionPart = millions == 1 ? "eine Million" : $"{SpellHundreds(millions)} Millionen";
        return belowMillion.Length == 0 ? millionPart : $"{millionPart} {belowMillion}";
    }

    /// <summary>Spells the thousands and units groups as a single glued word, e.g. "fünfundvierzigtausendeinhundert".</summary>
    private static string SpellBelowMillion(int thousands, int units)
    {
        var sb = new StringBuilder();
        if (thousands > 0) sb.Append(SpellHundreds(thousands)).Append("tausend");
        if (units > 0) sb.Append(SpellHundreds(units));
        return sb.ToString();
    }

    /// <summary>Spells a value in 1..999 as a glued word, e.g. "einhundertfünfundzwanzig".</summary>
    private static string SpellHundreds(int value)
    {
        var hundreds = value / 100;
        var remainder = value % 100;
        var sb = new StringBuilder();
        if (hundreds > 0) sb.Append(Ones[hundreds]).Append("hundert");
        if (remainder > 0) sb.Append(SpellBelowHundred(remainder));
        return sb.ToString();
    }

    private static string SpellBelowHundred(int value)
    {
        if (value < 20) return Ones[value];
        var tens = Tens[value / 10];
        var ones = value % 10;
        return ones == 0 ? tens : $"{Ones[ones]}und{tens}";
    }
}
