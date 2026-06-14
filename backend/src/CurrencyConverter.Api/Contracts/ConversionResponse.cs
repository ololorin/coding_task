namespace CurrencyConverter.Api.Contracts;

/// <summary>
/// Response payload for the convert endpoint. Serializes as <c>{ "conversionResult": "..." }</c>.
/// </summary>
/// <param name="ConversionResult">The amount written out in words.</param>
public sealed record ConversionResponse(string ConversionResult);
