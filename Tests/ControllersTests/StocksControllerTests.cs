using AutoFixture;
using Core.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ServiceContracts;
using ServiceContracts.FinnhubServices;
using StocksApp;

namespace Tests.ControllersTests;

public class StocksControllerTests
{
    private readonly Mock<IFinnhubSearchStocksService> _mockSearchStocksService;
    private readonly Mock<IFinnhubStocksService> _mockStocksService;
    private readonly Mock<ILogger<StocksController>> _mockLogger;

    private readonly IFixture _fixture;

    public StocksControllerTests()
    {
        _fixture = new Fixture();

        _mockStocksService = new Mock<IFinnhubStocksService>();
        _mockSearchStocksService = new Mock<IFinnhubSearchStocksService>();
        _mockLogger = new Mock<ILogger<StocksController>>();
    }

    private StocksController CreateController(IEnumerable<string>? popularStocks = null)
    {
        var tradingOptions = _fixture
            .Build<TradingOptions>()
            .With(
                o => o.PopularStocks,
                () => new List<string>(popularStocks?.ToArray() ?? Array.Empty<string>())
            )
            .Create();

        return new StocksController(
            _mockLogger.Object,
            Options.Create(tradingOptions),
            _mockSearchStocksService.Object,
            _mockStocksService.Object
        );
    }

    #region  Explore

    [Fact]
    public async Task Explore_ShowAllFalse_ReturnsOnlyPopularStocks()
    {
        var popularSymbols = new[] { "MSFT", "AAPL", "GOOG" };

        var apiStocks = popularSymbols
            .Select(sym => _fixture.Build<StockSymbolResponse>().With(s => s.Symbol, sym).Create())
            .Concat(_fixture.CreateMany<StockSymbolResponse>(5))
            .ToList();

        _mockStocksService.Setup(x => x.GetStocksAsync("US")).ReturnsAsync(apiStocks);

        var controller = CreateController(popularSymbols);

        var result = await controller.Explore(null, false);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<Stock>>().Subject.ToList();

        model.Should().HaveCount(3);
        model.Select(s => s.StockSymbol).Should().BeEquivalentTo(popularSymbols);
    }

    [Fact]
    public async Task Explore_ShowAllTrue_ReturnsAllStocks()
    {
        var apiStocks = _fixture.CreateMany<StockSymbolResponse>(10).ToList();

        _mockStocksService.Setup(s => s.GetStocksAsync("US")).ReturnsAsync(apiStocks);

        var controller = CreateController();

        var result = await controller.Explore(stock: null, showAll: true);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<Stock>>().Subject;

        model.Should().HaveCount(10);
    }

    [Fact]
    public async Task Explore_SetsViewBagStock_WhenStockProvided()
    {
        // Arrange
        _mockStocksService
            .Setup(s => s.GetStocksAsync("US"))
            .ReturnsAsync(_fixture.CreateMany<StockSymbolResponse>(1).ToList());

        var controller = CreateController();

        // Act
        await controller.Explore(stock: "AAPL", showAll: true);

        // Assert
        ((string)controller.ViewBag.Stock)
            .Should()
            .Be("AAPL");
    }

    [Fact]
    public async Task Explore_FinnhubReturnsNull_ReturnsViewWithEmptyModel()
    {
        // Arrange
        _mockStocksService
            .Setup(s => s.GetStocksAsync("US"))
            .ReturnsAsync((List<StockSymbolResponse>?)null);

        var controller = CreateController(new[] { "MSFT" });

        // Act
        var result = await controller.Explore(stock: null, showAll: false);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<Stock>>().Subject;

        model.Should().BeEmpty();
    }

    #endregion
}
