using System;
using Entities;
using FluentValidation;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs;
using ServiceContracts.Extensions;
using ServiceContracts.Interfaces;
using Services.Helpers;

namespace Services;

public class StockService : IStocksService
{

    private readonly StockMarketDbContext _context;

    public StockService(StockMarketDbContext context)
    {
        _context = context;
    }
    public async Task<BuyOrderResponse> CreateBuyOrderAsync(BuyOrderRequest request)
    {
        await ValidateRequest(request);

        var newBuyOrder = request.ToBuyOrder();
        newBuyOrder.Id = Guid.NewGuid();

        _context.BuyOrders.Add(newBuyOrder);
        await _context.SaveChangesAsync();

        return newBuyOrder.ToBuyOrderResponse();

    }

    public async Task<SellOrderResponse> CreateSellOrderAsync(SellOrderRequest request)
    {
        await ValidateRequest(request);


        var newSellOrder = request.ToSellOrder();
        newSellOrder.Id = Guid.NewGuid();

        _context.SellOrders.Add(newSellOrder);
        await _context.SaveChangesAsync();

        return newSellOrder.ToSellOrderResponse();

    }

    public async Task<List<BuyOrderResponse>> GetBuyOrdersAsync()
    {
        var buyOrders = await _context.BuyOrders
            .AsNoTracking()
            .OrderByDescending(bo => bo.DateAndTimeOfOrder)
            .Select(buyOrder => new BuyOrderResponse
            {
                BuyOrderID = buyOrder.Id,
                StockSymbol = buyOrder.StockSymbol,
                StockName = buyOrder.StockName,
                Quantity = buyOrder.Quantity,
                Price = buyOrder.Price,
                DateAndTimeOfOrder = buyOrder.DateAndTimeOfOrder,
                TradeAmount = buyOrder.Price * buyOrder.Quantity,
                TypeOfOrder = OrderType.BuyOrder
            })
            .ToListAsync();


        return buyOrders;
    }

    public async Task<List<SellOrderResponse>> GetSellOrdersAsync()
    {
        var sellOrders = await _context.SellOrders
            .AsNoTracking()
            .OrderByDescending(bo => bo.DateAndTimeOfOrder)
            .Select(sellOrder => new SellOrderResponse
            {
                SellOrderID = sellOrder.Id,
                StockSymbol = sellOrder.StockSymbol,
                StockName = sellOrder.StockName,
                Quantity = sellOrder.Quantity,
                Price = sellOrder.Price,
                DateAndTimeOfOrder = sellOrder.DateAndTimeOfOrder,
                TradeAmount = sellOrder.Price * sellOrder.Quantity,
                TypeOfOrder = OrderType.SellOrder
            })
            .ToListAsync();

        return sellOrders;
    }

    private async Task ValidateRequest(OrderRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidationHelper.ModelValidation(request);
    }
}
