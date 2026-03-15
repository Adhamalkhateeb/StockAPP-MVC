using Core.DTOs;
using Core.Enums;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using ServiceContracts.StocksServices;
using Services.Helpers;

namespace Services.StocksService;

public class StocksSellOrderService : ISellOrderService
{
    private readonly IStocksRepository _stocksRepository;
    private readonly ILogger<StocksSellOrderService> _logger;

    public StocksSellOrderService(
        IStocksRepository stocksRepository,
        ILogger<StocksSellOrderService> logger
    )
    {
        _stocksRepository = stocksRepository;
        _logger = logger;
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

    public async Task<SellOrderResponse> CreateSellOrderAsync(SellOrderRequest? request)
    {
        _logger.LogInformation(
            "Create sell order requested for symbol {StockSymbol}, quantity {Quantity}",
            request?.StockSymbol,
            request?.Quantity
        );

        ArgumentNullException.ThrowIfNull(request);
        ValidationHelper.Validate(request);

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
}
