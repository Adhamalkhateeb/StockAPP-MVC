using AutoFixture;
using Core.DTOs;
using FluentAssertions;
using Moq;
using RepositoryContracts;
using ServiceContracts;
using Services;

namespace Tests;

public class FinnhubServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IFinnhubRepository> _finnhubRepositoryMock;
    private readonly IFinnhubService _sut;

    public FinnhubServiceTests()
    {
        _fixture = new Fixture();
        _finnhubRepositoryMock = new Mock<IFinnhubRepository>();
        _sut = new FinnhubService(_finnhubRepositoryMock.Object);
    }

    #region GetCompanyProfileAsync

    [Fact]
    public async Task GetCompanyProfileAsync_WhenRepositoryReturnsNull_ReturnsNull()
    {
        var stockSymbol = _fixture.Create<string>();
        _finnhubRepositoryMock
            .Setup(x => x.GetCompanyProfileAsync(stockSymbol))
            .ReturnsAsync((CompanyProfileResponse?)null);

        var result = await _sut.GetCompanyProfileAsync(stockSymbol);

        result.Should().BeNull();
        _finnhubRepositoryMock.Verify(x => x.GetCompanyProfileAsync(stockSymbol), Times.Once);
    }

    [Fact]
    public async Task GetCompanyProfileAsync_WhenRepositoryReturnsData_ReturnsData()
    {
        var stockSymbol = _fixture.Create<string>();
        var expected = _fixture.Create<CompanyProfileResponse>();

        _finnhubRepositoryMock
            .Setup(x => x.GetCompanyProfileAsync(stockSymbol))
            .ReturnsAsync(expected);

        var result = await _sut.GetCompanyProfileAsync(stockSymbol);

        result.Should().BeEquivalentTo(expected);
        _finnhubRepositoryMock.Verify(x => x.GetCompanyProfileAsync(stockSymbol), Times.Once);
    }

    #endregion

    #region GetStockPriceQuoteAsync

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

    #endregion

    #region GetStocksAsync

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

    #endregion

    #region SearchStocksAsync

    [Fact]
    public async Task SearchStocksAsync_WhenRepositoryReturnsNull_ReturnsNull()
    {
        var query = _fixture.Create<string>();

        _finnhubRepositoryMock
            .Setup(x => x.SearchStocksAsync(query, null))
            .ReturnsAsync((SymbolLookupResultDto?)null);

        var result = await _sut.SearchStocksAsync(query);

        result.Should().BeNull();
        _finnhubRepositoryMock.Verify(x => x.SearchStocksAsync(query, null), Times.Once);
    }

    [Fact]
    public async Task SearchStocksAsync_WhenRepositoryResultPropertyIsNull_ReturnsNull()
    {
        var query = _fixture.Create<string>();
        var repositoryResult = new SymbolLookupResultDto { Count = 0, Result = null };

        _finnhubRepositoryMock
            .Setup(x => x.SearchStocksAsync(query, null))
            .ReturnsAsync(repositoryResult);

        var result = await _sut.SearchStocksAsync(query);

        result.Should().BeNull();
        _finnhubRepositoryMock.Verify(x => x.SearchStocksAsync(query, null), Times.Once);
    }

    [Fact]
    public async Task SearchStocksAsync_WhenRepositoryResultIsEmpty_ReturnsNull()
    {
        var query = _fixture.Create<string>();
        var repositoryResult = new SymbolLookupResultDto
        {
            Count = 0,
            Result = new List<SymbolLookupItemDto>(),
        };

        _finnhubRepositoryMock
            .Setup(x => x.SearchStocksAsync(query, null))
            .ReturnsAsync(repositoryResult);

        var result = await _sut.SearchStocksAsync(query);

        result.Should().BeNull();
        _finnhubRepositoryMock.Verify(x => x.SearchStocksAsync(query, null), Times.Once);
    }

    [Fact]
    public async Task SearchStocksAsync_WhenRepositoryReturnsData_ReturnsData()
    {
        var query = _fixture.Create<string>();
        var exchange = _fixture.Create<string>();
        var repositoryResult = new SymbolLookupResultDto
        {
            Count = 1,
            Result = new List<SymbolLookupItemDto> { _fixture.Create<SymbolLookupItemDto>() },
        };

        _finnhubRepositoryMock
            .Setup(x => x.SearchStocksAsync(query, exchange))
            .ReturnsAsync(repositoryResult);

        var result = await _sut.SearchStocksAsync(query, exchange);

        result.Should().BeEquivalentTo(repositoryResult);
        _finnhubRepositoryMock.Verify(x => x.SearchStocksAsync(query, exchange), Times.Once);
    }

    #endregion
}
