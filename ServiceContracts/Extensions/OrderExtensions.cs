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
            BuyOrderID = buyOrder.BuyOrderID,
            StockSymbol = buyOrder.StockSymbol,
            StockName = buyOrder.StockName,
            Quantity = buyOrder.Quantity,
            Price = buyOrder.Price,
            DateAndTimeOfOrder = buyOrder.DateAndTimeOfOrder,
            TradeAmount = buyOrder.Price * buyOrder.Quantity
        };
    }


    public static SellOrderResponse ToSellOrderResponse(this SellOrder sellOrder)
    {
        return new SellOrderResponse
        {
            SellOrderID = sellOrder.SellOrderID,
            StockSymbol = sellOrder.StockSymbol,
            StockName = sellOrder.StockName,
            Quantity = sellOrder.Quantity,
            Price = sellOrder.Price,
            DateAndTimeOfOrder = sellOrder.DateAndTimeOfOrder,
            TradeAmount = sellOrder.Price * sellOrder.Quantity
        };
    }
}
