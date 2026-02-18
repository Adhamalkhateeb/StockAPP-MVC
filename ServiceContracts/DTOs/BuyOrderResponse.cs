using System;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace ServiceContracts.DTOs;


/// <summary>
/// DTO class that represents a buy order - that can be used as return type of Stocks service
/// </summary>
public class BuyOrderResponse : OrderResponse, IEquatable<BuyOrderResponse>
{
    /// <summary>
    /// The unique Id of sell order
    /// </summary>
    public Guid BuyOrderID { get; set; }

    /// <summary>
    /// Checks if the current object and other (parameter) object values match
    /// </summary>
    /// <param name="obj">Other object of SellOrderResponse class, to compare</param>
    /// <returns>True or false determines whether current object and other objects match</returns>
    public bool Equals(BuyOrderResponse? other)
    {
        if (other is null)
            return false;

        return BuyOrderID == other.BuyOrderID &&
                StockSymbol == other.StockSymbol &&
                StockName == other.StockName &&
                Price == other.Price &&
                Quantity == other.Quantity &&
                DateAndTimeOfOrder == other.DateAndTimeOfOrder;
    }

    public override string ToString()
    {
        return $"""

        Buy Order ID: {BuyOrderID}
        Stock Symbol: {StockSymbol}
        Stock Name: {StockName}
        Date and Time of Buy Order: {DateAndTimeOfOrder.ToString("dd MMM yyyy hh:mm ss tt")}
        Quantity: {Quantity}
        Buy Price: {Price}
        Trade Amount: {TradeAmount}
        
        """;
    }
}
