using AutoFixture;
using Core.DTOs;
using Core.Extensions;
using Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceContracts;
using ServiceContracts.StocksServices;
using Services.StocksService;

namespace Tests.ServicesTests;

public class StocksSellOrderServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IStocksRepository> _stockRepositoryMock;
    private readonly Mock<ILogger<StocksSellOrderService>> _loggerMock;
    private readonly ISellOrderService _sut;

    public StocksSellOrderServiceTests()
    {
        _fixture = new Fixture();
        _stockRepositoryMock = new Mock<IStocksRepository>();
        _loggerMock = new Mock<ILogger<StocksSellOrderService>>();
        _sut = new StocksSellOrderService(_stockRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateSellOrderAsync_NullSellOrderRequest_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.CreateSellOrderAsync(null);
        await act.Should().ThrowAsync<ArgumentNullException>();

        _stockRepositoryMock.Verify(
            x => x.CreateSellOrderAsync(It.IsAny<SellOrder>()),
            Times.Never
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100001)]
    public async Task CreateSellOrderAsync_InvalidQuantity_ThrowsArgumentException(uint quantity)
    {
        var request = _fixture.Build<SellOrderRequest>().With(x => x.Quantity, quantity).Create();

        var act = async () => await _sut.CreateSellOrderAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();

        _stockRepositoryMock.Verify(
            x => x.CreateSellOrderAsync(It.IsAny<SellOrder>()),
            Times.Never
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100001)]
    public async Task CreateSellOrderAsync_InvalidPrice_ThrowsArgumentException(decimal price)
    {
        var request = _fixture.Build<SellOrderRequest>().With(x => x.Price, price).Create();

        var act = async () => await _sut.CreateSellOrderAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();

        _stockRepositoryMock.Verify(
            x => x.CreateSellOrderAsync(It.IsAny<SellOrder>()),
            Times.Never
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task CreateSellOrderAsync_NullOrEmptyStockSymbol_ThrowsArgumentException(
        string? stockSymbol
    )
    {
        var request = _fixture
            .Build<SellOrderRequest>()
            .With(x => x.StockSymbol, stockSymbol)
            .Create();

        var act = async () => await _sut.CreateSellOrderAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();

        _stockRepositoryMock.Verify(
            x => x.CreateSellOrderAsync(It.IsAny<SellOrder>()),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateSellOrderAsync_InvalidDateAndTimeOfOrder_ThrowsArgumentException()
    {
        var request = _fixture
            .Build<SellOrderRequest>()
            .With(x => x.DateAndTimeOfOrder, new DateTime(1999, 12, 31))
            .Create();

        var act = async () => await _sut.CreateSellOrderAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();

        _stockRepositoryMock.Verify(
            x => x.CreateSellOrderAsync(It.IsAny<SellOrder>()),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateSellOrderAsync_ValidSellOrderRequest_AddOrderSuccessfully()
    {
        var request = _fixture.Create<SellOrderRequest>();

        SellOrder capturedSellOrder = null!;
        _stockRepositoryMock
            .Setup(x => x.CreateSellOrderAsync(It.IsAny<SellOrder>()))
            .Callback<SellOrder>(x => capturedSellOrder = x)
            .ReturnsAsync((SellOrder x) => x);

        var result = await _sut.CreateSellOrderAsync(request);

        result.SellOrderID.Should().NotBe(Guid.Empty);
        capturedSellOrder.Should().NotBeNull();

        _stockRepositoryMock.Verify(x => x.CreateSellOrderAsync(It.IsAny<SellOrder>()), Times.Once);
    }

    [Fact]
    public async Task GetSellOrdersAsync_Empty_ReturnEmptyList()
    {
        _stockRepositoryMock.Setup(x => x.GetSellOrdersAsync()).ReturnsAsync(new List<SellOrder>());

        var result = await _sut.GetSellOrdersAsync();

        result.Should().BeEmpty();

        _stockRepositoryMock.Verify(x => x.GetSellOrdersAsync(), Times.Once);
    }

    [Fact]
    public async Task GetSellOrdersAsync_HaveData_ReturnAllList()
    {
        List<SellOrder> sellOrders =
        [
            _fixture.Build<SellOrder>().Create(),
            _fixture.Build<SellOrder>().Create(),
        ];

        _stockRepositoryMock.Setup(x => x.GetSellOrdersAsync()).ReturnsAsync(sellOrders);

        var expected = sellOrders.Select(x => x.ToSellOrderResponse()).ToList();
        var result = await _sut.GetSellOrdersAsync();

        result.Should().BeEquivalentTo(expected);

        _stockRepositoryMock.Verify(x => x.GetSellOrdersAsync(), Times.Once);
    }
}
