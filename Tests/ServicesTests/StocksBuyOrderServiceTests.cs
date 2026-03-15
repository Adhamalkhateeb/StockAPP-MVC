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

public class StocksBuyOrderServiceTests
{
    private readonly IBuyOrderService _sut;

    private readonly IFixture _fixture;
    private readonly Mock<IStocksRepository> _stockRepositoryMock;
    private readonly Mock<ILogger<StocksBuyOrderService>> _loggerMock;

    public StocksBuyOrderServiceTests()
    {
        _fixture = new Fixture();
        _stockRepositoryMock = new Mock<IStocksRepository>();
        _loggerMock = new Mock<ILogger<StocksBuyOrderService>>();
        _sut = new StocksBuyOrderService(_stockRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateBuyOrderAsync_NullBuyOrderRequest_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.CreateBuyOrderAsync(null);
        await act.Should().ThrowAsync<ArgumentNullException>();

        _stockRepositoryMock.Verify(x => x.CreateBuyOrderAsync(It.IsAny<BuyOrder>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100001)]
    public async Task CreateBuyOrderAsync_InvalidQuantity_ThrowsArgumentException(uint quantity)
    {
        var request = _fixture.Build<BuyOrderRequest>().With(x => x.Quantity, quantity).Create();

        var act = async () => await _sut.CreateBuyOrderAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();

        _stockRepositoryMock.Verify(x => x.CreateBuyOrderAsync(It.IsAny<BuyOrder>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100001)]
    public async Task CreateBuyOrderAsync_InvalidPrice_ThrowsArgumentException(decimal price)
    {
        var request = _fixture.Build<BuyOrderRequest>().With(x => x.Price, price).Create();

        var act = async () => await _sut.CreateBuyOrderAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();

        _stockRepositoryMock.Verify(x => x.CreateBuyOrderAsync(It.IsAny<BuyOrder>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task CreateBuyOrderAsync_NullOrEmptyStockSymbol_ThrowsArgumentException(
        string? stockSymbol
    )
    {
        var request = _fixture
            .Build<BuyOrderRequest>()
            .With(x => x.StockSymbol, stockSymbol)
            .Create();

        var act = async () => await _sut.CreateBuyOrderAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();

        _stockRepositoryMock.Verify(x => x.CreateBuyOrderAsync(It.IsAny<BuyOrder>()), Times.Never);
    }

    [Fact]
    public async Task CreateBuyOrderAsync_InvalidDateAndTimeOfOrder_ThrowsArgumentException()
    {
        var request = _fixture
            .Build<BuyOrderRequest>()
            .With(x => x.DateAndTimeOfOrder, new DateTime(1999, 12, 31))
            .Create();

        var act = async () => await _sut.CreateBuyOrderAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();

        _stockRepositoryMock.Verify(x => x.CreateBuyOrderAsync(It.IsAny<BuyOrder>()), Times.Never);
    }

    [Fact]
    public async Task CreateBuyOrderAsync_ValidBuyOrderRequest_AddOrderSuccessfully()
    {
        var request = _fixture.Create<BuyOrderRequest>();

        BuyOrder capturedOrder = null!;

        _stockRepositoryMock
            .Setup(x => x.CreateBuyOrderAsync(It.IsAny<BuyOrder>()))
            .Callback<BuyOrder>(x => capturedOrder = x)
            .ReturnsAsync((BuyOrder x) => x);

        var result = await _sut.CreateBuyOrderAsync(request);

        result.BuyOrderID.Should().NotBe(Guid.Empty);
        capturedOrder.Should().NotBeNull();

        _stockRepositoryMock.Verify(x => x.CreateBuyOrderAsync(It.IsAny<BuyOrder>()), Times.Once);
    }

    [Fact]
    public async Task GetBuyOrdersAsync_Empty_ReturnEmptyList()
    {
        _stockRepositoryMock.Setup(x => x.GetBuyOrdersAsync()).ReturnsAsync(new List<BuyOrder>());

        var result = await _sut.GetBuyOrdersAsync();

        result.Should().BeEmpty();

        _stockRepositoryMock.Verify(x => x.GetBuyOrdersAsync(), Times.Once);
    }

    [Fact]
    public async Task GetBuyOrdersAsync_HaveData_ReturnAllList()
    {
        List<BuyOrder> buyOrders =
        [
            _fixture.Build<BuyOrder>().Create(),
            _fixture.Build<BuyOrder>().Create(),
        ];

        _stockRepositoryMock.Setup(x => x.GetBuyOrdersAsync()).ReturnsAsync(buyOrders);

        var expected = buyOrders.Select(x => x.ToBuyOrderResponse()).ToList();
        var result = await _sut.GetBuyOrdersAsync();

        result.Should().BeEquivalentTo(expected);

        _stockRepositoryMock.Verify(x => x.GetBuyOrdersAsync(), Times.Once);
    }
}
