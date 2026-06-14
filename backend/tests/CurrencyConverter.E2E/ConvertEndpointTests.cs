using System.Net;
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
    public async Task Convert_returns_words_and_sets_content_language()
    {
        var client = _factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/currency/convert?amount=25.1");
        request.Headers.Add("Accept-Language", "en");

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var words = await response.Content.ReadAsStringAsync();
        Assert.Equal("twenty-five dollars and ten cents", words);

        Assert.Equal("en", response.Content.Headers.ContentLanguage.SingleOrDefault());
    }
}
