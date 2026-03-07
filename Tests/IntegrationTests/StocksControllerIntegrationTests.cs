using System.Net;
using Core.DTOs;
using Fizzler.Systems.HtmlAgilityPack;
using FluentAssertions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using ServiceContracts;
using StocksApp;

public class StocksControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly Mock<IFinnhubService> _mockFinnhubService;

    public StocksControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _mockFinnhubService = new Mock<IFinnhubService>();

        _mockFinnhubService
            .Setup(s => s.GetStocksAsync("US"))
            .ReturnsAsync(new List<StockSymbolResponse>());

        _mockFinnhubService
            .Setup(s => s.GetCompanyProfileAsync(It.IsAny<string>()))
            .ReturnsAsync(new CompanyProfileResponse { Name = "Microsoft", Country = "US" });

        _mockFinnhubService
            .Setup(s => s.GetStockPriceQuoteAsync(It.IsAny<string>()))
            .ReturnsAsync(new StockQuoteResponse { CurrentPrice = 300.50m });
    }

    private HttpClient CreateClientWithMockedService(IEnumerable<string>? popularStocks = null)
    {
        var configuredPopularStocks =
            popularStocks?.ToList() ?? new List<string> { "MSFT", "AAPL", "GOOG" };

        return _factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IFinnhubService>();
                    services.AddSingleton(_mockFinnhubService.Object);

                    services.PostConfigure<TradingOptions>(options =>
                    {
                        options.PopularStocks = configuredPopularStocks;
                    });
                });
            })
            .CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false,
                    HandleCookies = true,
                }
            );
    }

    [Fact]
    public async Task Explore_ShowAllFalse_ReturnsOnlyPopularStocks()
    {
        var symbols = new List<StockSymbolResponse>
        {
            new() { Symbol = "MSFT", Description = "Microsoft Corporation" },
            new() { Symbol = "AAPL", Description = "Apple Inc." },
            new() { Symbol = "GOOG", Description = "Alphabet Inc." },
            new() { Symbol = "ZZZZ", Description = "Non Popular Corp" },
        };

        _mockFinnhubService.Setup(s => s.GetStocksAsync("US")).ReturnsAsync(symbols);

        var client = CreateClientWithMockedService(new[] { "MSFT", "AAPL", "GOOG" });

        var response = await client.GetAsync("/Explore");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var stockItems = document.DocumentNode.QuerySelectorAll("#stocks-list ul.list li").ToList();

        stockItems.Should().HaveCount(3);
        html.Should().Contain("(MSFT)");
        html.Should().Contain("(AAPL)");
        html.Should().Contain("(GOOG)");
        html.Should().NotContain("(ZZZZ)");
    }

    [Fact]
    public async Task Explore_ShowAllTrue_ReturnsAllStocks()
    {
        var symbols = new List<StockSymbolResponse>
        {
            new() { Symbol = "MSFT", Description = "Microsoft Corporation" },
            new() { Symbol = "AAPL", Description = "Apple Inc." },
            new() { Symbol = "GOOG", Description = "Alphabet Inc." },
            new() { Symbol = "ZZZZ", Description = "Non Popular Corp" },
        };

        _mockFinnhubService.Setup(s => s.GetStocksAsync("US")).ReturnsAsync(symbols);

        var client = CreateClientWithMockedService(new[] { "MSFT", "AAPL", "GOOG" });

        var response = await client.GetAsync("/Explore?showAll=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var stockItems = document.DocumentNode.QuerySelectorAll("#stocks-list ul.list li").ToList();

        stockItems.Should().HaveCount(4);
        html.Should().Contain("(MSFT)");
        html.Should().Contain("(AAPL)");
        html.Should().Contain("(GOOG)");
        html.Should().Contain("(ZZZZ)");
    }

    [Fact]
    public async Task Explore_EmptyStocks_ReturnsNoStocksMessage()
    {
        _mockFinnhubService
            .Setup(s => s.GetStocksAsync("US"))
            .ReturnsAsync(new List<StockSymbolResponse>());

        var client = CreateClientWithMockedService();

        var response = await client.GetAsync("/Explore");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("No stocks");
    }

    [Fact]
    public async Task Explore_RootRoute_ReturnsOk()
    {
        _mockFinnhubService
            .Setup(s => s.GetStocksAsync("US"))
            .ReturnsAsync(
                new List<StockSymbolResponse>
                {
                    new() { Symbol = "MSFT", Description = "Microsoft Corporation" },
                }
            );

        var client = CreateClientWithMockedService(new[] { "MSFT" });

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("(MSFT)");
    }
}
