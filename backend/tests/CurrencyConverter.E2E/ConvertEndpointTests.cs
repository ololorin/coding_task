using System.Net;
using System.Net.Http.Json;
using CurrencyConverter.Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CurrencyConverter.E2E;

/// <summary>
/// End-to-end test that drives the real HTTP pipeline (controllers, localization,
/// model binding) through an in-memory server. One happy-path case.
/// </summary>
public class ConvertEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ConvertEndpointTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Convert_WithAcceptLanguageHeader_ReturnsWordsAndSetsContentLanguage()
    {
        var client = _factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/currency/convert?amount=25.1");
        request.Headers.Add("Accept-Language", "en");

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ConversionResponse>();
        Assert.NotNull(payload);
        Assert.Equal("twenty-five dollars and ten cents", payload.ConversionResult);

        Assert.Equal("en", response.Content.Headers.ContentLanguage.SingleOrDefault());
    }
}
