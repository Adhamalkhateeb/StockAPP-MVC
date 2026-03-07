using Core.DTOs;
using Core.Enums;
using Core.Extensions;
using ServiceContracts;
using Services.Helpers;

namespace Services;

public class StockService : IStocksService
{
    private readonly IStocksRepository _stocksRepository;

    public StockService(IStocksRepository stocksRepository)
    {
        _stocksRepository = stocksRepository;
    }

    public async Task<BuyOrderResponse> CreateBuyOrderAsync(BuyOrderRequest? request)
    {
        await ValidateRequest(request);

        var buyOrder = request!.ToBuyOrder();
        buyOrder.Id = Guid.NewGuid();

        buyOrder = await _stocksRepository.CreateBuyOrderAsync(buyOrder);

        return buyOrder.ToBuyOrderResponse();
    }

    public async Task<SellOrderResponse> CreateSellOrderAsync(SellOrderRequest? request)
    {
        await ValidateRequest(request);

        var sellOrder = request!.ToSellOrder();
        sellOrder.Id = Guid.NewGuid();

        sellOrder = await _stocksRepository.CreateSellOrderAsync(sellOrder);

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

        return sellOrders;
    }

    private async Task ValidateRequest(OrderRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidationHelper.Validate(request);
    }
}
