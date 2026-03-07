using AutoFixture;
using Core.DTOs;
using Core.Extensions;
using Entities;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Moq;
using ServiceContracts;
using Services;

namespace Tests;

public class StocksServiceTests
{
    private IStocksService _sut;

    private readonly IFixture _fixture;
    private readonly Mock<IStocksRepository> _stockRepositoryMock;

    public StocksServiceTests()
    {
        _fixture = new Fixture();
        _stockRepositoryMock = new Mock<IStocksRepository>();
        _sut = new StockService(_stockRepositoryMock.Object);
    }

    #region  CreateBuyOrder

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

    #endregion


    #region  CreateSellOrder

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

    #endregion


    #region  GetBuyOrdersAsync

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
        List<BuyOrder> buyOrders = new List<BuyOrder>()
        {
            _fixture.Build<BuyOrder>().Create(),
            _fixture.Build<BuyOrder>().Create(),
        };

        _stockRepositoryMock.Setup(x => x.GetBuyOrdersAsync()).ReturnsAsync(buyOrders);

        var expected = buyOrders.Select(x => x.ToBuyOrderResponse()).ToList();
        var result = await _sut.GetBuyOrdersAsync();

        result.Should().BeEquivalentTo(expected);

        _stockRepositoryMock.Verify(x => x.GetBuyOrdersAsync(), Times.Once);
    }

    #endregion

    #region  GetSellOrdersAsync

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
        List<SellOrder> sellOrders = new List<SellOrder>()
        {
            _fixture.Build<SellOrder>().Create(),
            _fixture.Build<SellOrder>().Create(),
        };

        _stockRepositoryMock.Setup(x => x.GetSellOrdersAsync()).ReturnsAsync(sellOrders);

        var expected = sellOrders.Select(x => x.ToSellOrderResponse()).ToList();
        var result = await _sut.GetSellOrdersAsync();

        result.Should().BeEquivalentTo(expected);

        _stockRepositoryMock.Verify(x => x.GetSellOrdersAsync(), Times.Once);
    }

    #endregion
}
