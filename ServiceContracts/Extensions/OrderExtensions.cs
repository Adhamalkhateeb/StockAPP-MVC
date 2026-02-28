using System;
using Entities;
using ServiceContracts.DTOs;

namespace ServiceContracts.Extensions;

public static class OrderExtensions
{
    public static BuyOrderResponse ToBuyOrderResponse(this BuyOrder buyOrder)
    {
        return new BuyOrderResponse
        {
            BuyOrderID = buyOrder.Id,
            StockSymbol = buyOrder.StockSymbol,
            StockName = buyOrder.StockName,
            Quantity = buyOrder.Quantity,
            Price = buyOrder.Price,
            DateAndTimeOfOrder = buyOrder.DateAndTimeOfOrder,
            TradeAmount = buyOrder.Price * buyOrder.Quantity,
            TypeOfOrder = OrderType.BuyOrder
        };
    }


    public static SellOrderResponse ToSellOrderResponse(this SellOrder sellOrder)
    {
        return new SellOrderResponse
        {
            SellOrderID = sellOrder.Id,
            StockSymbol = sellOrder.StockSymbol,
            StockName = sellOrder.StockName,
            Quantity = sellOrder.Quantity,
            Price = sellOrder.Price,
            DateAndTimeOfOrder = sellOrder.DateAndTimeOfOrder,
            TradeAmount = sellOrder.Price * sellOrder.Quantity,
            TypeOfOrder = OrderType.SellOrder
        };
    }
}
