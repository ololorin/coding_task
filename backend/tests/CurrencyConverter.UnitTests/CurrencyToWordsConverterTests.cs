using System.Globalization;
using CurrencyConverter.Domain;
using CurrencyConverter.Domain.Spelling;
using Xunit;

namespace CurrencyConverter.UnitTests;

public class CurrencyToWordsConverterTests
{
    private static ICurrencyConverter Create() =>
        new CurrencyToWordsConverter(new INumberSpeller[]
        {
            new EnglishNumberSpeller(),
            new GermanNumberSpeller()
        });

    private static decimal Amount(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);

    // The six examples straight from the task PDF.
    [Theory]
    [InlineData("0", "zero dollars")]
    [InlineData("1", "one dollar")]
    [InlineData("25.1", "twenty-five dollars and ten cents")]
    [InlineData("0.01", "zero dollars and one cent")]
    [InlineData("45100", "forty-five thousand one hundred dollars")]
    [InlineData("999999999.99",
        "nine hundred ninety-nine million nine hundred ninety-nine thousand nine hundred ninety-nine dollars and ninety-nine cents")]
    public void Converts_english_pdf_examples(string amount, string expected)
    {
        Assert.Equal(expected, Create().Convert(Amount(amount), SupportedLanguages.English));
    }

    // German equivalents of the same examples (Dollar/Cent, German numerals & word order).
    [Theory]
    [InlineData("0", "null Dollar")]
    [InlineData("1", "ein Dollar")]
    [InlineData("25.1", "fünfundzwanzig Dollar und zehn Cent")]
    [InlineData("0.01", "null Dollar und ein Cent")]
    [InlineData("45100", "fünfundvierzigtausendeinhundert Dollar")]
    [InlineData("999999999.99",
        "neunhundertneunundneunzig Millionen neunhundertneunundneunzigtausendneunhundertneunundneunzig Dollar und neunundneunzig Cent")]
    public void Converts_german_pdf_examples(string amount, string expected)
    {
        Assert.Equal(expected, Create().Convert(Amount(amount), SupportedLanguages.German));
    }

    // Edge cases exercising hyphenation, scale words, and the "und" ordering.
    [Theory]
    [InlineData("21", "twenty-one dollars")]
    [InlineData("100", "one hundred dollars")]
    [InlineData("1000000", "one million dollars")]
    [InlineData("1000001", "one million one dollars")]
    public void Converts_english_edge_cases(string amount, string expected)
    {
        Assert.Equal(expected, Create().Convert(Amount(amount), SupportedLanguages.English));
    }

    [Theory]
    [InlineData("21", "einundzwanzig Dollar")]
    [InlineData("100", "einhundert Dollar")]
    [InlineData("1000000", "eine Million Dollar")]
    [InlineData("2000000", "zwei Millionen Dollar")]
    [InlineData("16", "sechzehn Dollar")]
    [InlineData("30", "dreißig Dollar")]
    public void Converts_german_edge_cases(string amount, string expected)
    {
        Assert.Equal(expected, Create().Convert(Amount(amount), SupportedLanguages.German));
    }

    [Fact]
    public void Unknown_language_falls_back_to_english()
    {
        Assert.Equal("one dollar", Create().Convert(1m, "fr"));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("1000000000")]   // one above the maximum dollars
    [InlineData("1000000000.00")]
    [InlineData("1.001")]        // more than two decimal places
    public void Rejects_invalid_amounts(string amount)
    {
        Assert.Throws<CurrencyConversionException>(() => Create().Convert(Amount(amount), SupportedLanguages.English));
    }

    [Fact]
    public void Accepts_the_maximum_amount()
    {
        var result = Create().Convert(CurrencyToWordsConverter.MaxAmount, SupportedLanguages.English);
        Assert.StartsWith("nine hundred ninety-nine million", result);
        Assert.EndsWith("ninety-nine cents", result);
    }
}
