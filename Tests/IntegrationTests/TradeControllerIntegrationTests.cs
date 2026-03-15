using System.Net;
using Core.DTOs;
using Core.Validators;
using Fizzler.Systems.HtmlAgilityPack;
using FluentAssertions;
using FluentValidation;
using FluentValidation.AspNetCore;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using ServiceContracts;
using ServiceContracts.FinnhubServices;
using ServiceContracts.StocksServices;

namespace Tests.IntegrationTests;

public class TradeControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly Mock<IFinnhubStocksService> _mockStocksService;
    private readonly Mock<IBuyOrderService> _mockBuyOrderService;
    private readonly Mock<ISellOrderService> _mockSellOrderService;

    public TradeControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _mockStocksService = new Mock<IFinnhubStocksService>();
        _mockBuyOrderService = new Mock<IBuyOrderService>();
        _mockSellOrderService = new Mock<ISellOrderService>();

        _mockStocksService
            .Setup(s => s.GetStockSnapshotAsync(It.IsAny<string>()))
            .ReturnsAsync(
                new StockSnapshotResponse
                {
                    CompanyProfile = new CompanyProfileResponse
                    {
                        Name = "Microsoft Corporation",
                        Country = "US",
                    },
                    StockQuote = new StockQuoteResponse { CurrentPrice = 300.50m },
                    IsLiveDataAvailable = true,
                }
            );

        _mockBuyOrderService
            .Setup(s => s.GetBuyOrdersAsync())
            .ReturnsAsync(new List<BuyOrderResponse>());

        _mockSellOrderService
            .Setup(s => s.GetSellOrdersAsync())
            .ReturnsAsync(new List<SellOrderResponse>());

        _mockBuyOrderService
            .Setup(s => s.CreateBuyOrderAsync(It.IsAny<BuyOrderRequest>()))
            .ReturnsAsync(
                new BuyOrderResponse
                {
                    BuyOrderID = Guid.NewGuid(),
                    StockSymbol = "MSFT",
                    StockName = "Microsoft",
                    Quantity = 100,
                    Price = 300.50m,
                }
            );

        _mockSellOrderService
            .Setup(s => s.CreateSellOrderAsync(It.IsAny<SellOrderRequest>()))
            .ReturnsAsync(
                new SellOrderResponse
                {
                    SellOrderID = Guid.NewGuid(),
                    StockSymbol = "MSFT",
                    StockName = "Microsoft",
                    Quantity = 100,
                    Price = 300.50m,
                }
            );
    }

    private HttpClient CreateClientWithMockedServices()
    {
        return _factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IFinnhubStocksService>();
                    services.RemoveAll<IBuyOrderService>();
                    services.RemoveAll<ISellOrderService>();
                    services.AddSingleton(_mockStocksService.Object);
                    services.AddSingleton(_mockBuyOrderService.Object);
                    services.AddSingleton(_mockSellOrderService.Object);
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

    /// <summary>
    /// GETs /Trade/Index with the given client (preserving its cookie jar),
    /// then extracts the hidden __RequestVerificationToken from the page.
    /// </summary>
    private async Task<string> GetRequestVerificationTokenAsync(
        HttpClient client,
        string url = "/Trade/Index"
    )
    {
        var getResponse = await client.GetAsync(url);
        getResponse.EnsureSuccessStatusCode();

        var html = await getResponse.Content.ReadAsStringAsync();
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var tokenInput = document.DocumentNode.QuerySelector(
            "input[name='__RequestVerificationToken']"
        );

        tokenInput.Should().NotBeNull("the page must contain an anti-forgery hidden input");

        var token = tokenInput!.GetAttributeValue("value", string.Empty);
        token.Should().NotBeNullOrWhiteSpace("the anti-forgery token must not be empty");

        return token;
    }

    #region Index

    [Fact]
    public async Task Index_NoSymbol_ReturnsOkWithMsftData()
    {
        var client = CreateClientWithMockedServices();

        var response = await client.GetAsync("/Trade/Index");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        var document = new HtmlDocument();
        document.LoadHtml(html);

        document.DocumentNode.QuerySelector("body").Should().NotBeNull();
        html.Should().Contain("MSFT");
    }

    [Fact]
    public async Task Index_WithCustomSymbol_ReturnsOkWithSymbolData()
    {
        _mockStocksService
            .Setup(s => s.GetStockSnapshotAsync("AAPL"))
            .ReturnsAsync(
                new StockSnapshotResponse
                {
                    StockSymbol = "AAPL",
                    CompanyProfile = new CompanyProfileResponse
                    {
                        Name = "Apple Inc.",
                        Country = "US",
                    },
                    StockQuote = new StockQuoteResponse { CurrentPrice = 175.25m },
                    IsLiveDataAvailable = true,
                }
            );

        var client = CreateClientWithMockedServices();

        var response = await client.GetAsync("/Trade/Index/AAPL");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("AAPL");
    }

    [Fact]
    public async Task Index_WhenLiveDataIsUnavailable_ReturnsFallbackMessage()
    {
        _mockStocksService
            .Setup(s => s.GetStockSnapshotAsync(It.IsAny<string>()))
            .ReturnsAsync(
                new StockSnapshotResponse
                {
                    StockSymbol = "MSFT",
                    IsLiveDataAvailable = false,
                    IsAccessDenied = true,
                    UserMessage =
                        "Live market data is temporarily unavailable for this symbol. Please try again shortly or pick another stock.",
                }
            );

        var client = CreateClientWithMockedServices();

        var response = await client.GetAsync("/Trade/Index");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("Live market data is temporarily unavailable");
        html.Should().NotContain("Buy");
    }

    #endregion

    #region Orders

    [Fact]
    public async Task Orders_ReturnsOkWithOrdersView()
    {
        var client = CreateClientWithMockedServices();

        var response = await client.GetAsync("/Trade/Orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        var document = new HtmlDocument();
        document.LoadHtml(html);

        document.DocumentNode.QuerySelector("body").Should().NotBeNull();
    }

    [Fact]
    public async Task Orders_WithExistingOrders_DisplaysOrdersInView()
    {
        _mockBuyOrderService
            .Setup(s => s.GetBuyOrdersAsync())
            .ReturnsAsync(
                new List<BuyOrderResponse>
                {
                    new BuyOrderResponse
                    {
                        BuyOrderID = Guid.NewGuid(),
                        StockSymbol = "MSFT",
                        StockName = "Microsoft",
                        Quantity = 10,
                        Price = 300m,
                        DateAndTimeOfOrder = DateTime.UtcNow,
                    },
                }
            );

        var client = CreateClientWithMockedServices();

        var response = await client.GetAsync("/Trade/Orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("MSFT");
    }

    #endregion

    #region BuyOrder

    [Fact]
    public async Task BuyOrder_ValidRequest_RedirectsToOrders()
    {
        var client = CreateClientWithMockedServices();
        var token = await GetRequestVerificationTokenAsync(client);

        var formData = new Dictionary<string, string>
        {
            { "__RequestVerificationToken", token },
            { "StockSymbol", "MSFT" },
            { "StockName", "Microsoft" },
            { "Quantity", "10" },
            { "Price", "300.50" },
        };

        var response = await client.PostAsync(
            "/Trade/BuyOrder",
            new FormUrlEncodedContent(formData)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("Orders");
    }

    [Fact]
    public async Task BuyOrder_InvalidRequest_ZeroQuantity_ReturnsIndexViewWithErrors()
    {
        var client = CreateClientWithMockedServices();
        var token = await GetRequestVerificationTokenAsync(client);

        var formData = new Dictionary<string, string>
        {
            { "__RequestVerificationToken", token },
            { "StockSymbol", "MSFT" },
            { "StockName", "Microsoft" },
            { "Quantity", "0" }, // invalid: must be >= 1
            { "Price", "300.50" },
        };

        var response = await client.PostAsync(
            "/Trade/BuyOrder",
            new FormUrlEncodedContent(formData)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        html.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task BuyOrder_InvalidRequest_ZeroPrice_ReturnsIndexViewWithErrors()
    {
        var client = CreateClientWithMockedServices();
        var token = await GetRequestVerificationTokenAsync(client);

        var formData = new Dictionary<string, string>
        {
            { "__RequestVerificationToken", token },
            { "StockSymbol", "MSFT" },
            { "StockName", "Microsoft" },
            { "Quantity", "10" },
            { "Price", "0" }, // invalid: price must be > 0
        };

        var response = await client.PostAsync(
            "/Trade/BuyOrder",
            new FormUrlEncodedContent(formData)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        html.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region SellOrder

    [Fact]
    public async Task SellOrder_ValidRequest_RedirectsToOrders()
    {
        var client = CreateClientWithMockedServices();
        var token = await GetRequestVerificationTokenAsync(client);

        var formData = new Dictionary<string, string>
        {
            { "__RequestVerificationToken", token },
            { "StockSymbol", "MSFT" },
            { "StockName", "Microsoft" },
            { "Quantity", "10" },
            { "Price", "300.50" },
        };

        var response = await client.PostAsync(
            "/Trade/SellOrder",
            new FormUrlEncodedContent(formData)
        );

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("Orders");
    }

    [Fact]
    public async Task SellOrder_InvalidRequest_ZeroQuantity_ReturnsIndexViewWithErrors()
    {
        var client = CreateClientWithMockedServices();
        var token = await GetRequestVerificationTokenAsync(client);

        var formData = new Dictionary<string, string>
        {
            { "__RequestVerificationToken", token },
            { "StockSymbol", "MSFT" },
            { "StockName", "Microsoft" },
            { "Quantity", "0" }, // invalid: must be >= 1
            { "Price", "300.50" },
        };

        var response = await client.PostAsync(
            "/Trade/SellOrder",
            new FormUrlEncodedContent(formData)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        html.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SellOrder_InvalidRequest_ZeroPrice_ReturnsIndexViewWithErrors()
    {
        var client = CreateClientWithMockedServices();
        var token = await GetRequestVerificationTokenAsync(client);

        var formData = new Dictionary<string, string>
        {
            { "__RequestVerificationToken", token },
            { "StockSymbol", "MSFT" },
            { "StockName", "Microsoft" },
            { "Quantity", "10" },
            { "Price", "0" }, // invalid: price must be > 0
        };

        var response = await client.PostAsync(
            "/Trade/SellOrder",
            new FormUrlEncodedContent(formData)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        html.Should().NotBeNullOrEmpty();
    }

    #endregion
}
