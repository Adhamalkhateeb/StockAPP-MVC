using Core.DTOs;
using Core.Enums;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using ServiceContracts;
using ServiceContracts.StocksServices;
using Services.Helpers;

namespace Services.StocksService;

public class StocksBuyOrderService : IBuyOrderService
{
    private readonly IStocksRepository _stocksRepository;
    private readonly ILogger<StocksBuyOrderService> _logger;

    public StocksBuyOrderService(
        IStocksRepository stocksRepository,
        ILogger<StocksBuyOrderService> logger
    )
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

        ArgumentNullException.ThrowIfNull(request);
        ValidationHelper.Validate(request);

        var buyOrder = request.ToBuyOrder();
        buyOrder.Id = Guid.NewGuid();

        buyOrder = await _stocksRepository.CreateBuyOrderAsync(buyOrder);

        _logger.LogInformation(
            "Buy order persisted with id {BuyOrderId} for symbol {StockSymbol}",
            buyOrder.Id,
            buyOrder.StockSymbol
        );

        return buyOrder.ToBuyOrderResponse();
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
}
