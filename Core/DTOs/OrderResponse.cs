namespace Core.DTOs;

using Core.Enums;

public abstract class OrderResponse
{
    /// <summary>
    /// The unique symbol of the stock
    /// </summary>
    public string? StockSymbol { get; set; }

    /// <summary>
    /// The Company Name of the stock
    /// </summary>
    public string? StockName { get; set; }

    /// <summary>
    /// Date and time of order, when it is placed by the user
    /// </summary>
    public DateTime DateAndTimeOfOrder { get; set; }

    /// <summary>
    /// The number of stocks (shares) to buy
    /// </summary>
    public uint Quantity { get; set; }

    /// <summary>
    /// The price of each stock (share)
    /// </summary>
    public decimal Price { get; set; }

    public decimal TradeAmount { get; set; }

    public OrderType TypeOfOrder { get; set; }

    /// <summary>
    /// Returns an int value that represents unique stock id of the current object
    /// </summary>
    /// <returns>unique int value</returns>
    public override int GetHashCode()
    {
        return StockSymbol!.GetHashCode();
    }
}
