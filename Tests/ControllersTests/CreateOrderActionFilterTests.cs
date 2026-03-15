using AutoFixture;
using Core.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ServiceContracts;
using ServiceContracts.FinnhubServices;
using ServiceContracts.StocksServices;
using StocksApp;
using StocksApp.Controllers;
using StocksApp.Models;

namespace Tests.ControllersTests;

public class CreateOrderActionFilterTests
{
    private readonly CreateOrderActionFilter _filter;
    private readonly TradeController _controller;
    private readonly IFixture _fixture;

    public CreateOrderActionFilterTests()
    {
        _fixture = new Fixture();

        var filterLogger = new Mock<ILogger<CreateOrderActionFilter>>();
        _filter = new CreateOrderActionFilter(filterLogger.Object);

        var controllerLogger = new Mock<ILogger<TradeController>>();
        var mockFinnhub = new Mock<IFinnhubStocksService>();
        var mockBuyOrders = new Mock<IBuyOrderService>();
        var mockSellOrders = new Mock<ISellOrderService>();
        var mockConfig = new Mock<IConfiguration>();
        var tradingOptions = Options.Create(
            _fixture.Build<TradingOptions>().With(o => o.DefaultOrderQuantity, 100u).Create()
        );

        _controller = new TradeController(
            controllerLogger.Object,
            tradingOptions,
            mockFinnhub.Object,
            mockBuyOrders.Object,
            mockSellOrders.Object,
            mockConfig.Object
        )
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
    }

    private ActionExecutingContext CreateContext(object? argument = null)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor()
        );
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = actionContext.HttpContext,
        };

        var arguments = new Dictionary<string, object?>();
        if (argument is not null)
            arguments["request"] = argument;

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            arguments,
            _controller
        );
    }

    private ActionExecutionDelegate CreateNext(ActionExecutingContext context, bool[] wasCalled)
    {
        return () =>
        {
            wasCalled[0] = true;
            return Task.FromResult(
                new ActionExecutedContext(context, new List<IFilterMetadata>(), _controller)
            );
        };
    }

    #region Valid model — next() is called

    [Fact]
    public async Task OnActionExecutionAsync_ValidBuyOrderModel_CallsNext()
    {
        var request = _fixture.Create<BuyOrderRequest>();
        var context = CreateContext(request);
        var wasCalled = new bool[] { false };

        await _filter.OnActionExecutionAsync(context, CreateNext(context, wasCalled));

        wasCalled[0].Should().BeTrue();
        context.Result.Should().BeNull();
    }

    [Fact]
    public async Task OnActionExecutionAsync_ValidSellOrderModel_CallsNext()
    {
        var request = _fixture.Create<SellOrderRequest>();
        var context = CreateContext(request);
        var wasCalled = new bool[] { false };

        await _filter.OnActionExecutionAsync(context, CreateNext(context, wasCalled));

        wasCalled[0].Should().BeTrue();
        context.Result.Should().BeNull();
    }

    #endregion

    #region Invalid model — short-circuits with Index view

    [Fact]
    public async Task OnActionExecutionAsync_InvalidBuyOrderModel_DoesNotCallNext()
    {
        var request = _fixture.Build<BuyOrderRequest>().With(r => r.Quantity, 0u).Create();
        var context = CreateContext(request);
        _controller.ModelState.AddModelError("Quantity", "Quantity must be between 1 and 100000.");
        var wasCalled = new bool[] { false };

        await _filter.OnActionExecutionAsync(context, CreateNext(context, wasCalled));

        wasCalled[0].Should().BeFalse();
    }

    [Fact]
    public async Task OnActionExecutionAsync_InvalidBuyOrderModel_ReturnsIndexViewWithStockTrade()
    {
        var request = _fixture
            .Build<BuyOrderRequest>()
            .With(r => r.Quantity, 0u)
            .With(r => r.StockSymbol, "AAPL")
            .With(r => r.StockName, "Apple Inc.")
            .With(r => r.Price, 150m)
            .Create();

        var context = CreateContext(request);
        _controller.ModelState.AddModelError("Quantity", "Quantity must be between 1 and 100000.");

        await _filter.OnActionExecutionAsync(context, CreateNext(context, new bool[] { false }));

        var viewResult = context.Result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be(nameof(TradeController.Index));
        var model = viewResult.Model.Should().BeOfType<StockTrade>().Subject;
        model.StockSymbol.Should().Be("AAPL");
        model.StockName.Should().Be("Apple Inc.");
        model.Quantity.Should().Be(0u);
        model.Price.Should().Be(150m);
    }

    [Fact]
    public async Task OnActionExecutionAsync_InvalidSellOrderModel_ReturnsIndexViewWithStockTrade()
    {
        var request = _fixture
            .Build<SellOrderRequest>()
            .With(r => r.Quantity, 0u)
            .With(r => r.StockSymbol, "TSLA")
            .With(r => r.StockName, "Tesla")
            .With(r => r.Price, 200m)
            .Create();
        var context = CreateContext(request);
        _controller.ModelState.AddModelError("Quantity", "Quantity must be between 1 and 100000.");

        await _filter.OnActionExecutionAsync(context, CreateNext(context, new bool[] { false }));

        var viewResult = context.Result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().Be(nameof(TradeController.Index));
        var model = viewResult.Model.Should().BeOfType<StockTrade>().Subject;
        model.StockSymbol.Should().Be("TSLA");
        model.StockName.Should().Be("Tesla");
        model.Quantity.Should().Be(0u);
        model.Price.Should().Be(200m);
    }

    #endregion

    #region Invalid model — ViewData errors are populated

    [Fact]
    public async Task OnActionExecutionAsync_InvalidModel_SetsViewDataErrors()
    {
        var request = _fixture.Build<BuyOrderRequest>().With(r => r.Quantity, 0u).Create();
        var context = CreateContext(request);
        _controller.ModelState.AddModelError("Quantity", "Quantity must be between 1 and 100000.");
        _controller.ModelState.AddModelError("Price", "Price cannot be zero.");

        await _filter.OnActionExecutionAsync(context, CreateNext(context, new bool[] { false }));

        var errors = (List<string>)_controller.ViewData["Errors"]!;
        errors.Should().NotBeNullOrEmpty();
        errors.Should().Contain("Quantity must be between 1 and 100000.");
        errors.Should().Contain("Price cannot be zero.");
    }

    #endregion
}
