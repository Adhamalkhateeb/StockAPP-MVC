using AutoFixture;
using Core.DTOs;
using Core.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RepositoryContracts;
using ServiceContracts.FinnhubServices;
using Services;

namespace Tests.ServicesTests;

public class FinnhubStockPriceQuoteServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IFinnhubRepository> _finnhubRepositoryMock;
    private readonly Mock<ILogger<FinnhubStockPriceQuoteService>> _loggerMock;
    private readonly IFinnhubStockPriceQuoteService _sut;

    public FinnhubStockPriceQuoteServiceTests()
    {
        _fixture = new Fixture();
        _finnhubRepositoryMock = new Mock<IFinnhubRepository>();
        _loggerMock = new Mock<ILogger<FinnhubStockPriceQuoteService>>();
        _sut = new FinnhubStockPriceQuoteService(_finnhubRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetStockPriceQuoteAsync_WhenRepositoryReturnsNull_ReturnsNull()
    {
        var stockSymbol = _fixture.Create<string>();
        _finnhubRepositoryMock
            .Setup(x => x.GetStockPriceQuoteAsync(stockSymbol))
            .ReturnsAsync((StockQuoteResponse?)null);

        var result = await _sut.GetStockPriceQuoteAsync(stockSymbol);

        result.Should().BeNull();
        _finnhubRepositoryMock.Verify(x => x.GetStockPriceQuoteAsync(stockSymbol), Times.Once);
    }

    [Fact]
    public async Task GetStockPriceQuoteAsync_WhenRepositoryReturnsData_ReturnsData()
    {
        var stockSymbol = _fixture.Create<string>();
        var expected = _fixture.Create<StockQuoteResponse>();

        _finnhubRepositoryMock
            .Setup(x => x.GetStockPriceQuoteAsync(stockSymbol))
            .ReturnsAsync(expected);

        var result = await _sut.GetStockPriceQuoteAsync(stockSymbol);

        result.Should().BeEquivalentTo(expected);
        _finnhubRepositoryMock.Verify(x => x.GetStockPriceQuoteAsync(stockSymbol), Times.Once);
    }

    [Fact]
    public async Task GetStockPriceQuoteAsync_WhenRepositoryThrowsAccessDenied_ReturnsNull()
    {
        var stockSymbol = _fixture.Create<string>();

        _finnhubRepositoryMock
            .Setup(x => x.GetStockPriceQuoteAsync(stockSymbol))
            .ThrowsAsync(new FinnhubAccessDeniedException("https://finnhub.io/test"));

        var result = await _sut.GetStockPriceQuoteAsync(stockSymbol);

        result.Should().BeNull();
    }
}
