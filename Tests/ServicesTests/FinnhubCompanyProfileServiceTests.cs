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

public class FinnhubCompanyProfileServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IFinnhubRepository> _finnhubRepositoryMock;
    private readonly Mock<ILogger<FinnhubCompanyProfileService>> _loggerMock;
    private readonly IFinnhubCompanyProfileService _sut;

    public FinnhubCompanyProfileServiceTests()
    {
        _fixture = new Fixture();
        _finnhubRepositoryMock = new Mock<IFinnhubRepository>();
        _loggerMock = new Mock<ILogger<FinnhubCompanyProfileService>>();
        _sut = new FinnhubCompanyProfileService(_finnhubRepositoryMock.Object, _loggerMock.Object);
    }

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

    [Fact]
    public async Task GetCompanyProfileAsync_WhenRepositoryThrowsAccessDenied_ReturnsNull()
    {
        var stockSymbol = _fixture.Create<string>();

        _finnhubRepositoryMock
            .Setup(x => x.GetCompanyProfileAsync(stockSymbol))
            .ThrowsAsync(new FinnhubAccessDeniedException("https://finnhub.io/test"));

        var result = await _sut.GetCompanyProfileAsync(stockSymbol);

        result.Should().BeNull();
    }
}
