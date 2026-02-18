using System;

namespace ServiceContracts.DTOs;


/// <summary>
/// DTO class that represents a sell order - that can be used as return type of Stocks service
/// </summary>
public class SellOrderResponse : OrderResponse, IEquatable<SellOrderResponse>
{
    /// <summary>
    /// The unique Id of sell order
    /// </summary>
    public Guid SellOrderID { get; set; }


    /// <summary>
    /// Checks if the current object and other (parameter) object values match
    /// </summary>
    /// <param name="obj">Other object of SellOrderResponse class, to compare</param>
    /// <returns>True or false determines whether current object and other objects match</returns>
    public bool Equals(SellOrderResponse? other)
    {
        if (other is null)
            return false;

        return SellOrderID == other.SellOrderID &&
                StockSymbol == other.StockSymbol &&
                StockName == other.StockName &&
                Price == other.Price &&
                Quantity == other.Quantity &&
                DateAndTimeOfOrder == other.DateAndTimeOfOrder;
    }

    public override string ToString()
    {
        return $"""

        Sell Order ID: {SellOrderID}
        Stock Symbol: {StockSymbol}
        Stock Name: {StockName}
        Date and Time of Sell Order: {DateAndTimeOfOrder.ToString("dd MMM yyyy hh:mm ss tt")}
        Quantity: {Quantity}
        Buy Price: {Price}
        Trade Amount: {TradeAmount}
        
        """;
    }

}
