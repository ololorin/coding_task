namespace CurrencyConverter.Domain;

/// <summary>
/// Thrown when an amount cannot be converted because it violates the domain rules
/// (negative, above the maximum, or more than two decimal places).
/// </summary>
public sealed class CurrencyConversionException : Exception
{
    public CurrencyConversionException(string message) : base(message)
    {
    }
}
