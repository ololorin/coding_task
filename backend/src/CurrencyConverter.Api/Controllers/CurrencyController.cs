using System.Globalization;
using CurrencyConverter.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CurrencyController : ControllerBase
{
    private readonly ICurrencyConverter _converter;

    public CurrencyController(ICurrencyConverter converter) => _converter = converter;

    /// <summary>
    /// Converts a dollar amount into words. The language is taken from the request culture
    /// resolved by the localization middleware (driven by the <c>Accept-Language</c> header);
    /// the response <c>Content-Language</c> header reflects the language actually used.
    /// </summary>
    /// <param name="amount">The amount in dollars using an invariant ('.') decimal separator.</param>
    [HttpGet("convert")]
    [AllowAnonymous]
    public ActionResult<string> Convert([FromQuery] decimal amount)
    {
        var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        try
        {
            return Ok(_converter.Convert(amount, language));
        }
        catch (CurrencyConversionException ex)
        {
            return Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid amount");
        }
    }
}
