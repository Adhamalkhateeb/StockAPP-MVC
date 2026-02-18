using System;
using Entities;
using FluentValidation;
using Microsoft.AspNetCore.Server.HttpSys;
using ServiceContracts.DTOs;
using ServiceContracts.Extensions;
using ServiceContracts.Interfaces;

namespace Services;

public class StockService : IStockService
{
    private readonly List<BuyOrder> _buyOrders;
    private readonly List<SellOrder> _sellOrders;
    private readonly IValidator<OrderRequest> _validator;

    public StockService(IValidator<OrderRequest> validator)
    {
        _buyOrders = new List<BuyOrder>();
        _sellOrders = new List<SellOrder>();
        _validator = validator;
    }
    public async Task<BuyOrderResponse> CreateBuyOrderAsync(BuyOrderRequest? request)
    {
        await ValidateRequest(request);

        var newBuyOrder = request!.ToBuyOrder();
        newBuyOrder.BuyOrderID = Guid.NewGuid();

        _buyOrders.Add(newBuyOrder);

        return newBuyOrder.ToBuyOrderResponse();

    }

    public async Task<SellOrderResponse> CreateSellOrderAsync(SellOrderRequest? request)
    {
        await ValidateRequest(request);


        var newSellOrder = request!.ToSellOrder();
        newSellOrder.SellOrderID = Guid.NewGuid();

        _sellOrders.Add(newSellOrder);

        return newSellOrder.ToSellOrderResponse();

    }

    public Task<List<BuyOrderResponse>> GetBuyOrdersAsync()
    {
        var result = _buyOrders
            .OrderByDescending(bo => bo.DateAndTimeOfOrder)
            .Select(x => x.ToBuyOrderResponse())
            .ToList();

        return Task.FromResult(result);
    }

    public Task<List<SellOrderResponse>> GetSellOrdersAsync()
    {
        var result = _sellOrders
            .OrderByDescending(bo => bo.DateAndTimeOfOrder)
            .Select(x => x.ToSellOrderResponse())
            .ToList();

        return Task.FromResult(result);
    }

    private async Task ValidateRequest(OrderRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = await _validator.ValidateAsync(request);

        if (!validationResult.IsValid)
            throw new ArgumentException(validationResult.Errors.First().ErrorMessage);
    }
}
