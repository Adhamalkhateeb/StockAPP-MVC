using AutoFixture;
using Core.DTOs;
using Core.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Rotativa.AspNetCore;
using ServiceContracts;
using StocksApp;
using StocksApp.Controllers;
using StocksApp.Models;

namespace Tests.ControllersTests;

public class TradeControllerTests
{
    private readonly Mock<IFinnhubService> _mockFinnhubService;
    private readonly Mock<IStocksService> _mockStocksService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<TradeController>> _mockLogger;
    private readonly IOptions<TradingOptions> _tradingOptions;
    private readonly TradeController _controller;
    private readonly IFixture _fixture;

    public TradeControllerTests()
    {
        _fixture = new Fixture();

        _mockFinnhubService = new Mock<IFinnhubService>();
        _mockStocksService = new Mock<IStocksService>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<TradeController>>();

        var tradingOptions = _fixture
            .Build<TradingOptions>()
            .With(o => o.DefaultOrderQuantity, 100u)
            .Create();

        _tradingOptions = Options.Create(tradingOptions);

        _mockConfiguration.Setup(c => c["FinnhubToken"]).Returns("test-token");

        _controller = new TradeController(
            _mockLogger.Object,
            _tradingOptions,
            _mockStocksService.Object,
            _mockFinnhubService.Object,
            _mockConfiguration.Object
        )
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };

        var validator = new Mock<IObjectModelValidator>();
        validator.Setup(v =>
            v.Validate(
                It.IsAny<ActionContext>(),
                It.IsAny<ValidationStateDictionary>(),
                It.IsAny<string>(),
                It.IsAny<object>()
            )
        );

        _controller.ObjectValidator = validator.Object;
    }

    #region  Index
    [Fact]
    public async Task Index_DefaultSymbol_ReturnsViewWithStockTrade()
    {
        _mockFinnhubService
            .Setup(x => x.GetStockSnapshotAsync("MSFT"))
            .ReturnsAsync(
                new StockSnapshotResponse
                {
                    StockSymbol = "MSFT",
                    CompanyProfile = new CompanyProfileResponse { Name = "Microsoft" },
                    StockQuote = new StockQuoteResponse { CurrentPrice = 300.5m },
                    IsLiveDataAvailable = true,
                }
            );

        var result = await _controller.Index(null);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<StockTrade>().Subject;

        model.Should().NotBeNull();
        model.StockSymbol.Should().Be("MSFT");
        model.StockName.Should().Be("Microsoft");
        model.Quantity.Should().Be(100);
        model.Price.Should().Be(300.50m);
        model.CanTrade.Should().BeTrue();
    }

    [Fact]
    public async Task Index_CustomSymbol_ReturnsViewWithCorrectStockTrade()
    {
        _mockFinnhubService
            .Setup(x => x.GetStockSnapshotAsync("AAPL"))
            .ReturnsAsync(
                new StockSnapshotResponse
                {
                    StockSymbol = "AAPL",
                    CompanyProfile = new CompanyProfileResponse { Name = "Apple Inc." },
                    StockQuote = new StockQuoteResponse { CurrentPrice = 300.5m },
                    IsLiveDataAvailable = true,
                }
            );

        var result = await _controller.Index("AAPL");

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<StockTrade>().Subject;

        model.Should().NotBeNull();
        model.StockSymbol.Should().Be("AAPL");
        model.StockName.Should().Be("Apple Inc.");
        model.Price.Should().Be(300.50m);
        model.CanTrade.Should().BeTrue();
    }

    [Fact]
    public async Task Index_FinnhubReturnsUnavailableSnapshot_ReturnsNonTradableViewModel()
    {
        _mockFinnhubService
            .Setup(x => x.GetStockSnapshotAsync("Invalid"))
            .ReturnsAsync(
                new StockSnapshotResponse
                {
                    StockSymbol = "Invalid",
                    IsLiveDataAvailable = false,
                    IsAccessDenied = true,
                    UserMessage = "Live market data is temporarily unavailable for this symbol. Please try again shortly or pick another stock.",
                }
            );

        var result = await _controller.Index("Invalid");

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<StockTrade>().Subject;

        model.StockSymbol.Should().Be("Invalid");
        model.StockName.Should().Be("Invalid");
        model.Price.Should().Be(0);
        model.CanTrade.Should().BeFalse();
        model.DataUnavailableMessage.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Index_SetsFinnhubTokenInViewBag()
    {
        _mockFinnhubService
            .Setup(s => s.GetStockSnapshotAsync(It.IsAny<string>()))
            .ReturnsAsync(
                new StockSnapshotResponse
                {
                    StockSymbol = "Success",
                    CompanyProfile = _fixture.Create<CompanyProfileResponse>(),
                    StockQuote = _fixture.Create<StockQuoteResponse>(),
                    IsLiveDataAvailable = true,
                }
            );

        await _controller.Index("Success");

        ((string)_controller.ViewBag.FinnhubToken).Should().Be("test-token");
    }

    #endregion

    #region Orders

    [Fact]
    public async Task Orders_ReturnsViewWithBuyAndSellOrders()
    {
        var buyOrders = _fixture.CreateMany<BuyOrderResponse>(2).ToList();
        var sellOrders = _fixture.CreateMany<SellOrderResponse>(1).ToList();

        _mockStocksService.Setup(x => x.GetBuyOrdersAsync()).ReturnsAsync(buyOrders);
        _mockStocksService.Setup(x => x.GetSellOrdersAsync()).ReturnsAsync(sellOrders);

        var expectedModel = new Orders { BuyOrders = buyOrders, SellOrders = sellOrders };
        var result = await _controller.Orders();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<Orders>().Subject;

        model.Should().BeEquivalentTo(expectedModel);
    }

    [Fact]
    public async Task Orders_EmptyOrders_ReturnsViewWithEmptyLists()
    {
        _mockStocksService
            .Setup(x => x.GetBuyOrdersAsync())
            .ReturnsAsync(new List<BuyOrderResponse>());
        _mockStocksService
            .Setup(x => x.GetSellOrdersAsync())
            .ReturnsAsync(new List<SellOrderResponse>());

        var result = await _controller.Orders();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<Orders>().Subject;

        model.BuyOrders.Should().BeEmpty();
        model.SellOrders.Should().BeEmpty();
    }
    #endregion

    #region  BuyOrder

    [Fact]
    public async Task BuyOrder_ValidRequest_RedirectsToOrders()
    {
        var request = _fixture.Create<BuyOrderRequest>();

        _mockStocksService
            .Setup(x => x.CreateBuyOrderAsync(It.IsAny<BuyOrderRequest>()))
            .ReturnsAsync(_fixture.Create<BuyOrderResponse>());

        var result = await _controller.BuyOrder(request);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(_controller.Orders));

        _mockStocksService.Verify(
            x => x.CreateBuyOrderAsync(It.IsAny<BuyOrderRequest>()),
            Times.Once
        );
    }

    [Fact]
    public async Task BuyOrder_InvalidRequest_ControllerStillRedirects()
    {
        var request = _fixture.Build<BuyOrderRequest>().With(r => r.Quantity, 0u).Create();

        _mockStocksService
            .Setup(x => x.CreateBuyOrderAsync(It.IsAny<BuyOrderRequest>()))
            .ReturnsAsync(_fixture.Create<BuyOrderResponse>());

        _controller.ModelState.AddModelError("Quantity", "Invalid");

        var result = await _controller.BuyOrder(request);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(_controller.Orders));
    }

    #endregion


    #region  SellOrder

    [Fact]
    public async Task SellOrder_ValidRequest_RedirectsToOrders()
    {
        var request = _fixture.Create<SellOrderRequest>();
        var response = request.ToSellOrder().ToSellOrderResponse();

        _mockStocksService.Setup(x => x.CreateSellOrderAsync(request)).ReturnsAsync(response);

        var result = await _controller.SellOrder(request);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be(nameof(_controller.Orders));

        _mockStocksService.Verify(
            s => s.CreateSellOrderAsync(It.IsAny<SellOrderRequest>()),
            Times.Once
        );
    }

    [Fact]
    public async Task SellOrder_InvalidRequest_ControllerStillRedirects()
    {
        var request = _fixture.Build<SellOrderRequest>().With(r => r.Quantity, 0u).Create();

        _mockStocksService
            .Setup(x => x.CreateSellOrderAsync(It.IsAny<SellOrderRequest>()))
            .ReturnsAsync(_fixture.Create<SellOrderResponse>());

        _controller.ModelState.AddModelError("Quantity", "Invalid");

        var result = await _controller.SellOrder(request);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(_controller.Orders));
    }

    #endregion

    #region OrdersPDF

    [Fact]
    public async Task OrdersPDF_ReturnsPdfResult()
    {
        // Arrange
        _mockStocksService
            .Setup(s => s.GetBuyOrdersAsync())
            .ReturnsAsync(_fixture.CreateMany<BuyOrderResponse>(1).ToList());
        _mockStocksService
            .Setup(s => s.GetSellOrdersAsync())
            .ReturnsAsync(_fixture.CreateMany<SellOrderResponse>(1).ToList());

        // Act
        var result = await _controller.OrdersPDF();

        var pdfResult = result.Should().BeOfType<ViewAsPdf>().Subject;
        var model = pdfResult.Model.Should().BeAssignableTo<List<OrderResponse>>().Subject;

        model.Should().HaveCount(2);
    }

    [Fact]
    public async Task OrdersPDF_OrdersSortedByDateDescending()
    {
        // Arrange
        var older = DateTime.UtcNow.AddDays(-2);
        var newer = DateTime.UtcNow;

        var buyOrders = _fixture
            .Build<BuyOrderResponse>()
            .With(o => o.DateAndTimeOfOrder, older)
            .CreateMany(1)
            .ToList();

        var sellOrders = _fixture
            .Build<SellOrderResponse>()
            .With(o => o.DateAndTimeOfOrder, newer)
            .CreateMany(1)
            .ToList();

        _mockStocksService.Setup(s => s.GetBuyOrdersAsync()).ReturnsAsync(buyOrders);
        _mockStocksService.Setup(s => s.GetSellOrdersAsync()).ReturnsAsync(sellOrders);

        // Act
        var result = await _controller.OrdersPDF();

        // Assert
        var pdfResult = result.Should().BeOfType<ViewAsPdf>().Subject;
        var model = pdfResult.Model.Should().BeAssignableTo<List<OrderResponse>>().Subject;

        model.Should().HaveCount(2).And.BeInDescendingOrder(o => o.DateAndTimeOfOrder);
    }

    [Fact]
    public async Task OrdersPDF_EmptyOrders_ReturnsEmptyPdf()
    {
        // Arrange
        _mockStocksService
            .Setup(s => s.GetBuyOrdersAsync())
            .ReturnsAsync(new List<BuyOrderResponse>());
        _mockStocksService
            .Setup(s => s.GetSellOrdersAsync())
            .ReturnsAsync(new List<SellOrderResponse>());

        // Act
        var result = await _controller.OrdersPDF();

        // Assert
        var pdfResult = result.Should().BeOfType<ViewAsPdf>().Subject;
        var model = pdfResult.Model.Should().BeAssignableTo<List<OrderResponse>>().Subject;

        model.Should().BeEmpty();
    }

    #endregion
}
