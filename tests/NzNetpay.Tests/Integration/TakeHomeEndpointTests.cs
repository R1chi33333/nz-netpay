using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace NzNetpay.Tests.Integration;

public sealed class TakeHomeEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public TakeHomeEndpointTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Take_home_60k_returns_full_breakdown()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/v1/take-home?salary=60000");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var root = body!.RootElement;
        Assert.Equal("2026-27", root.GetProperty("taxYear").GetString());
        Assert.Equal(10_220.50m, root.GetProperty("deductions").GetProperty("paye").GetDecimal());
        Assert.Equal(1_050.00m, root.GetProperty("deductions").GetProperty("accLevy").GetDecimal());
        Assert.True(root.GetProperty("takeHome").GetProperty("weekly").GetDecimal() > 0m);
    }

    [Fact]
    public async Task Take_home_supports_tax_year_and_options()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/v1/take-home?salary=200000&taxYear=2025-26&kiwiSaverRate=0.04&studentLoan=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var deductions = body!.RootElement.GetProperty("deductions");
        Assert.Equal(2_551.59m, deductions.GetProperty("accLevy").GetDecimal());
        Assert.Equal(8_000m, deductions.GetProperty("kiwiSaverEmployee").GetDecimal());
    }

    [Theory]
    [InlineData("/v1/take-home?salary=-1")]
    [InlineData("/v1/take-home?salary=99999999999")]
    [InlineData("/v1/take-home?salary=60000&taxYear=1999-00")]
    [InlineData("/v1/take-home?salary=60000&kiwiSaverRate=0.05")]
    public async Task Invalid_input_returns_problem_details(string url)
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Gst_add_and_remove_round_trip()
    {
        using var client = factory.CreateClient();

        var add = await client.GetFromJsonAsync<JsonDocument>("/v1/gst?amount=100");
        var remove = await client.GetFromJsonAsync<JsonDocument>("/v1/gst?amount=115&direction=remove");

        Assert.Equal(115m, add!.RootElement.GetProperty("inclusive").GetDecimal());
        Assert.Equal(100m, remove!.RootElement.GetProperty("exclusive").GetDecimal());
    }

    [Fact]
    public async Task Rates_endpoint_exposes_sources()
    {
        using var client = factory.CreateClient();

        var rates = await client.GetFromJsonAsync<JsonDocument>("/v1/rates");

        var years = rates!.RootElement.GetProperty("taxYears");
        Assert.Equal(2, years.GetArrayLength());
        var sources = years[0].GetProperty("sources");
        Assert.StartsWith("https://www.ird.govt.nz/", sources.GetProperty("acc").GetString());
    }

    [Fact]
    public async Task Swagger_ui_is_served_at_docs()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/docs/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
