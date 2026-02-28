using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using ServiceContracts.DTOs;
using ServiceContracts.Extensions;
using ServiceContracts.Interfaces;
using Services;

namespace Tests;

public class StocksServiceTests
{

    private IStocksService _sut;
    private readonly Mock<IValidator<OrderRequest>> _validatorMock;

    public StocksServiceTests()
    {

        _validatorMock = new Mock<IValidator<OrderRequest>>();
        _sut = new StockService(_validatorMock.Object);
    }

    #region Helpers

    private void SetupValidatorAsValid()
    {
        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<OrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
    }

    private void SetupValidatorAsInvalid(string propertyName = "Any", string message = "Validation failed")
    {
        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<OrderRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[]
            {
                new ValidationFailure(propertyName, message)
            }));
    }
    private static BuyOrderRequest CreateValidBuyRequest(
        uint quantity = 10,
        double price = 1000,
        string? stockSymbol = "AAPL",
        DateTime? date = null)
    {
        return new BuyOrderRequest
        {
            StockSymbol = stockSymbol,
            StockName = "Apple",
            DateAndTimeOfOrder = date ?? DateTime.UtcNow,
            Quantity = quantity,
            Price = price
        };
    }

    private static SellOrderRequest CreateValidSellRequest(
        uint quantity = 10,
        double price = 1000,
        string? stockSymbol = "AAPL",
        DateTime? date = null)
    {
        return new SellOrderRequest
        {
            StockSymbol = stockSymbol,
            StockName = "Apple",
            DateAndTimeOfOrder = date ?? DateTime.UtcNow,
            Quantity = quantity,
            Price = price
        };
    }

    private async Task<List<BuyOrderResponse>> SeedBuyOrdersAsync()
    {
        var buyOrderRequests = new List<BuyOrderRequest>
        {
            CreateValidBuyRequest(10, 2000, "MSFT", new DateTime(2001, 10, 12)),
            CreateValidBuyRequest(12, 3000, "MSFT", new DateTime(2026, 1, 1)),
            CreateValidBuyRequest()
        };

        var results = new List<BuyOrderResponse>();

        foreach (var bo in buyOrderRequests)
        {
            var res = await _sut.CreateBuyOrderAsync(bo);
            results.Add(res);
        }

        return results;
    }

    private async Task<List<SellOrderResponse>> SeedSellOrdersAsync()
    {
        var sellOrderRequests = new List<SellOrderRequest>
        {
            CreateValidSellRequest(10,2000,"MSFT",new DateTime(2001,10,12)),
            CreateValidSellRequest(12,3000,"AAPL",new DateTime(2026,1,1)),
            CreateValidSellRequest(),
        };

        var results = new List<SellOrderResponse>();

        foreach (var bo in sellOrderRequests)
        {
            var res = await _sut.CreateSellOrderAsync(bo);
            results.Add(res);
        }

        return results;
    }
    #endregion


    #region  CreateBuyOrder
    [Fact]
    public async Task CreateBuyOrderAsync_NullBuyOrderRequest_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateBuyOrderAsync(null));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100001)]
    public async Task CreateBuyOrderAsync_InvalidQuantity_ThrowsArgumentException(uint quantity)
    {
        SetupValidatorAsInvalid("Quantity");

        var request = CreateValidBuyRequest(quantity: quantity);

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateBuyOrderAsync(request));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100001)]
    public async Task CreateBuyOrderAsync_InvalidPrice_ThrowsArgumentException(double price)
    {
        SetupValidatorAsInvalid("Price");

        var request = CreateValidBuyRequest(price: price);


        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateBuyOrderAsync(request));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task CreateBuyOrderAsync_NullOrEmptyStockSymbol_ThrowsArgumentException(string stockSymbol)
    {
        SetupValidatorAsInvalid("StockSymbol");


        var request = CreateValidBuyRequest(stockSymbol: stockSymbol);


        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateBuyOrderAsync(request));
    }

    [Fact]
    public async Task CreateBuyOrderAsync_InvalidDateAndTimeOfOrder_ThrowsArgumentException()
    {
        SetupValidatorAsInvalid("DateAndTimeOfOrder");

        var request = CreateValidBuyRequest(date: new DateTime(1999, 12, 31));

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateBuyOrderAsync(request));
    }

    [Fact]
    public async Task CreateBuyOrderAsync_ValidBuyOrderRequest_ThrowsArgumentException()
    {
        SetupValidatorAsValid();


        var request = CreateValidBuyRequest();

        var result = await _sut.CreateBuyOrderAsync(request);
        var allBuyOrders = await _sut.GetBuyOrdersAsync();

        Assert.NotEqual(result.BuyOrderID, Guid.Empty);
        Assert.Contains(result, allBuyOrders);
    }



    #endregion


    #region  CreateSellOrder

    [Fact]
    public async Task CreateSellOrderAsync_NullSellOrderRequest_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateSellOrderAsync(null));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100001)]
    public async Task CreateSellOrderAsync_InvalidQuantity_ThrowsArgumentException(uint quantity)
    {
        SetupValidatorAsInvalid("Quantity");

        var request = CreateValidSellRequest(quantity: quantity);

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateSellOrderAsync(request));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100001)]
    public async Task CreateSellOrderAsync_InvalidPrice_ThrowsArgumentException(double price)
    {
        SetupValidatorAsInvalid("Price");

        var request = CreateValidSellRequest(price: price);


        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateSellOrderAsync(request));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task CreateSellOrderAsync_NullOrEmptyStockSymbol_ThrowsArgumentException(string stockSymbol)
    {
        SetupValidatorAsInvalid("StockSymbol");


        var request = CreateValidSellRequest(stockSymbol: stockSymbol);


        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateSellOrderAsync(request));
    }

    [Fact]
    public async Task CreateSellOrderAsync_InvalidDateAndTimeOfOrder_ThrowsArgumentException()
    {
        SetupValidatorAsInvalid("DateAndTimeOfOrder");

        var request = CreateValidSellRequest(date: new DateTime(1999, 12, 31));

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateSellOrderAsync(request));
    }

    [Fact]
    public async Task CreateSellOrderAsync_ValidSellOrderRequest_ThrowsArgumentException()
    {
        SetupValidatorAsValid();


        var request = CreateValidSellRequest();

        var result = await _sut.CreateSellOrderAsync(request);
        var allSellOrders = await _sut.GetSellOrdersAsync();

        Assert.NotEqual(result.SellOrderID, Guid.Empty);
        Assert.Contains(result, allSellOrders);
    }

    #endregion


    #region  GetBuyOrdersAsync

    [Fact]
    public async Task GetBuyOrdersAsync_Empty_ReturnEmptyList()
    {
        var result = await _sut.GetBuyOrdersAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBuyOrdersAsync_AfterSeeding_ReturnAllList()
    {
        SetupValidatorAsValid();

        var expected = await SeedBuyOrdersAsync();
        var result = await _sut.GetBuyOrdersAsync();

        Assert.Equal(expected.Count, result.Count);

        foreach (var person in expected)
        {
            Assert.Contains(person, result);
        }
    }

    #endregion

    #region  GetSellOrdersAsync

    [Fact]
    public async Task GetSellOrdersAsync_Empty_ReturnEmptyList()
    {
        var result = await _sut.GetSellOrdersAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSellOrdersAsync_AfterSeeding_ReturnAllList()
    {
        SetupValidatorAsValid();

        var expected = await SeedSellOrdersAsync();
        var result = await _sut.GetSellOrdersAsync();

        Assert.Equal(expected.Count, result.Count);

        foreach (var person in expected)
        {
            Assert.Contains(person, result);
        }
    }
    #endregion
}
