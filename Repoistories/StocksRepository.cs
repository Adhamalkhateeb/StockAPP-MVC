using Entities;
using Entities.Data;
using Microsoft.EntityFrameworkCore;

public class StocksRepository : IStocksRepository
{
    private readonly StockMarketDbContext _context;

    public StocksRepository(StockMarketDbContext context)
    {
        _context = context;
    }

    public async Task<BuyOrder> CreateBuyOrderAsync(BuyOrder buyOrder)
    {
        _context.BuyOrders.Add(buyOrder);
        await _context.SaveChangesAsync();

        return buyOrder;
    }

    public async Task<SellOrder> CreateSellOrderAsync(SellOrder sellOrder)
    {
        _context.SellOrders.Add(sellOrder);
        await _context.SaveChangesAsync();

        return sellOrder;
    }

    public async Task<List<BuyOrder>> GetBuyOrdersAsync()
    {
        return await _context
            .BuyOrders.AsNoTracking()
            .OrderByDescending(x => x.DateAndTimeOfOrder)
            .ToListAsync();
    }

    public async Task<List<SellOrder>> GetSellOrdersAsync()
    {
        return await _context
            .SellOrders.AsNoTracking()
            .OrderByDescending(x => x.DateAndTimeOfOrder)
            .ToListAsync();
    }
}
