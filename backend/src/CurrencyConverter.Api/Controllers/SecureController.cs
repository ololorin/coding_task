using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Api.Controllers;

/// <summary>
/// Sample protected endpoint demonstrating that the API-key authentication scheme works.
/// Requires a valid <c>X-Api-Key</c> header; returns 401 otherwise.
/// </summary>
[ApiController]
[Route("api/secure")]
[Authorize]
public sealed class SecureController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { message = "authenticated", user = User.Identity?.Name });
}
