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

public class FinnhubStocksServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IFinnhubRepository> _finnhubRepositoryMock;
    private readonly Mock<ILogger<FinnhubStocksService>> _loggerMock;
    private readonly IFinnhubStocksService _sut;

    public FinnhubStocksServiceTests()
    {
        _fixture = new Fixture();
        _finnhubRepositoryMock = new Mock<IFinnhubRepository>();
        _loggerMock = new Mock<ILogger<FinnhubStocksService>>();
        _sut = new FinnhubStocksService(_finnhubRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetStocksAsync_WhenRepositoryReturnsNull_ReturnsNull()
    {
        var exchange = _fixture.Create<string>();

        _finnhubRepositoryMock
            .Setup(x => x.GetStocksAsync(exchange, null, null, null))
            .ReturnsAsync((List<StockSymbolResponse>?)null);

        var result = await _sut.GetStocksAsync(exchange);

        result.Should().BeNull();
        _finnhubRepositoryMock.Verify(
            x => x.GetStocksAsync(exchange, null, null, null),
            Times.Once
        );
    }

    [Fact]
    public async Task GetStocksAsync_WhenRepositoryReturnsEmptyList_ReturnsNull()
    {
        var exchange = _fixture.Create<string>();

        _finnhubRepositoryMock
            .Setup(x => x.GetStocksAsync(exchange, null, null, null))
            .ReturnsAsync(new List<StockSymbolResponse>());

        var result = await _sut.GetStocksAsync(exchange);

        result.Should().BeNull();
        _finnhubRepositoryMock.Verify(
            x => x.GetStocksAsync(exchange, null, null, null),
            Times.Once
        );
    }

    [Fact]
    public async Task GetStocksAsync_WhenRepositoryReturnsData_ReturnsData()
    {
        var exchange = _fixture.Create<string>();
        var mic = _fixture.Create<string>();
        var securityType = _fixture.Create<string>();
        var currency = _fixture.Create<string>();
        var expected = _fixture.CreateMany<StockSymbolResponse>(3).ToList();

        _finnhubRepositoryMock
            .Setup(x => x.GetStocksAsync(exchange, mic, securityType, currency))
            .ReturnsAsync(expected);

        var result = await _sut.GetStocksAsync(exchange, mic, securityType, currency);

        result.Should().BeEquivalentTo(expected);
        _finnhubRepositoryMock.Verify(
            x => x.GetStocksAsync(exchange, mic, securityType, currency),
            Times.Once
        );
    }

    [Fact]
    public async Task GetStockSnapshotAsync_WhenRepositoryThrowsAccessDenied_ReturnsUnavailableSnapshot()
    {
        var stockSymbol = _fixture.Create<string>();

        _finnhubRepositoryMock
            .Setup(x => x.GetCompanyProfileAsync(stockSymbol))
            .ThrowsAsync(new FinnhubAccessDeniedException("https://finnhub.io/test"));

        var result = await _sut.GetStockSnapshotAsync(stockSymbol);

        result.StockSymbol.Should().Be(stockSymbol);
        result.IsLiveDataAvailable.Should().BeFalse();
        result.IsAccessDenied.Should().BeTrue();
        result.UserMessage.Should().NotBeNullOrWhiteSpace();
    }
}
