using Entities;
using Entities.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class StocksRepository : IStocksRepository
{
    private readonly StockMarketDbContext _context;
    private readonly ILogger<StocksRepository> _logger;

    public StocksRepository(StockMarketDbContext context, ILogger<StocksRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<BuyOrder> CreateBuyOrderAsync(BuyOrder buyOrder)
    {
        _logger.LogInformation(
            "Persisting buy order for symbol {StockSymbol}, quantity {Quantity}",
            buyOrder.StockSymbol,
            buyOrder.Quantity
        );

        _context.BuyOrders.Add(buyOrder);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Buy order persisted with id {BuyOrderId}", buyOrder.Id);

        return buyOrder;
    }

    public async Task<SellOrder> CreateSellOrderAsync(SellOrder sellOrder)
    {
        _logger.LogInformation(
            "Persisting sell order for symbol {StockSymbol}, quantity {Quantity}",
            sellOrder.StockSymbol,
            sellOrder.Quantity
        );

        _context.SellOrders.Add(sellOrder);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Sell order persisted with id {SellOrderId}", sellOrder.Id);

        return sellOrder;
    }

    public async Task<List<BuyOrder>> GetBuyOrdersAsync()
    {
        var orders = await _context
            .BuyOrders.AsNoTracking()
            .OrderByDescending(x => x.DateAndTimeOfOrder)
            .ToListAsync();

        _logger.LogInformation("Loaded {BuyOrdersCount} buy orders from database", orders.Count);

        return orders;
    }

    public async Task<List<SellOrder>> GetSellOrdersAsync()
    {
        var orders = await _context
            .SellOrders.AsNoTracking()
            .OrderByDescending(x => x.DateAndTimeOfOrder)
            .ToListAsync();

        _logger.LogInformation("Loaded {SellOrdersCount} sell orders from database", orders.Count);

        return orders;
    }
}
