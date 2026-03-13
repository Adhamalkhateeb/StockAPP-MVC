using Core.DTOs;
using Core.Enums;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using ServiceContracts;
using Services.Helpers;

namespace Services;

public class StockService : IStocksService
{
    private readonly IStocksRepository _stocksRepository;
    private readonly ILogger<StockService> _logger;

    public StockService(IStocksRepository stocksRepository, ILogger<StockService> logger)
    {
        _stocksRepository = stocksRepository;
        _logger = logger;
    }

    public async Task<BuyOrderResponse> CreateBuyOrderAsync(BuyOrderRequest? request)
    {
        _logger.LogInformation(
            "Create buy order requested for symbol {StockSymbol}, quantity {Quantity}",
            request?.StockSymbol,
            request?.Quantity
        );

        ValidateRequest(request);

        var buyOrder = request!.ToBuyOrder();
        buyOrder.Id = Guid.NewGuid();

        buyOrder = await _stocksRepository.CreateBuyOrderAsync(buyOrder);

        _logger.LogInformation(
            "Buy order persisted with id {BuyOrderId} for symbol {StockSymbol}",
            buyOrder.Id,
            buyOrder.StockSymbol
        );

        return buyOrder.ToBuyOrderResponse();
    }

    public async Task<SellOrderResponse> CreateSellOrderAsync(SellOrderRequest? request)
    {
        _logger.LogInformation(
            "Create sell order requested for symbol {StockSymbol}, quantity {Quantity}",
            request?.StockSymbol,
            request?.Quantity
        );

        ValidateRequest(request);

        var sellOrder = request!.ToSellOrder();
        sellOrder.Id = Guid.NewGuid();

        sellOrder = await _stocksRepository.CreateSellOrderAsync(sellOrder);

        _logger.LogInformation(
            "Sell order persisted with id {SellOrderId} for symbol {StockSymbol}",
            sellOrder.Id,
            sellOrder.StockSymbol
        );

        return sellOrder.ToSellOrderResponse();
    }

    public async Task<List<BuyOrderResponse>> GetBuyOrdersAsync()
    {
        var buyOrders = (await _stocksRepository.GetBuyOrdersAsync())
            .Select(buyOrder => new BuyOrderResponse
            {
                BuyOrderID = buyOrder.Id,
                StockSymbol = buyOrder.StockSymbol,
                StockName = buyOrder.StockName,
                Quantity = buyOrder.Quantity,
                Price = buyOrder.Price,
                DateAndTimeOfOrder = buyOrder.DateAndTimeOfOrder,
                TradeAmount = buyOrder.Price * buyOrder.Quantity,
                TypeOfOrder = OrderType.BuyOrder,
            })
            .ToList();

        _logger.LogInformation("Retrieved {BuyOrdersCount} buy orders", buyOrders.Count);

        return buyOrders;
    }

    public async Task<List<SellOrderResponse>> GetSellOrdersAsync()
    {
        var sellOrders = (await _stocksRepository.GetSellOrdersAsync())
            .Select(sellOrder => new SellOrderResponse
            {
                SellOrderID = sellOrder.Id,
                StockSymbol = sellOrder.StockSymbol,
                StockName = sellOrder.StockName,
                Quantity = sellOrder.Quantity,
                Price = sellOrder.Price,
                DateAndTimeOfOrder = sellOrder.DateAndTimeOfOrder,
                TradeAmount = sellOrder.Price * sellOrder.Quantity,
                TypeOfOrder = OrderType.SellOrder,
            })
            .ToList();

        _logger.LogInformation("Retrieved {SellOrdersCount} sell orders", sellOrders.Count);

        return sellOrders;
    }

    // FIX 1: Was async with a pointless await Task.CompletedTask at the end.
    // Validation is purely synchronous — no I/O, no async work.
    private void ValidateRequest(OrderRequest? request)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(request);
            ValidationHelper.Validate(request);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Order request validation failed for symbol {StockSymbol}",
                request?.StockSymbol
            );
            throw;
        }
    }
}
