using AutoFixture;
using Core.DTOs;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RepositoryContracts;
using ServiceContracts.FinnhubServices;
using Services;

namespace Tests.ServicesTests;

public class FinnhubSearchStocksServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IFinnhubRepository> _finnhubRepositoryMock;
    private readonly Mock<ILogger<FinnhubSearchStocksService>> _loggerMock;
    private readonly IFinnhubSearchStocksService _sut;

    public FinnhubSearchStocksServiceTests()
    {
        _fixture = new Fixture();
        _finnhubRepositoryMock = new Mock<IFinnhubRepository>();
        _loggerMock = new Mock<ILogger<FinnhubSearchStocksService>>();
        _sut = new FinnhubSearchStocksService(_finnhubRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SearchStocksAsync_WhenRepositoryReturnsNull_ReturnsNull()
    {
        var query = _fixture.Create<string>();

        _finnhubRepositoryMock
            .Setup(x => x.SearchStocksAsync(query, null))
            .ReturnsAsync((SymbolLookupResultDto)null!);

        var result = await _sut.SearchStocksAsync(query);

        result.Should().NotBeNull();
        result.Result.Should().BeNull();
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

        result.Should().NotBeNull();
        result.Result.Should().BeNull();
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

        result.Should().NotBeNull();
        result.Result.Should().BeEmpty();
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
}
