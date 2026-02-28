namespace Entities;

/// <summary>
/// Represents a sell order to sell the stocks
/// </summary>
public class SellOrder
{
    /// <summary>
    /// The unique ID of the sell order
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The unique symbol of the stock
    /// </summary>
    public string? StockSymbol { get; set; }

    /// <summary>
    /// The company name of the stock
    /// </summary>
    public string? StockName { get; set; }

    /// <summary>
    /// Date and time of order, when it is placed by the user
    /// </summary>
    public DateTime DateAndTimeOfOrder { get; set; }

    /// <summary>
    /// The number of stocks (shares) to sell
    /// </summary>
    public uint Quantity { get; set; }

    /// <summary>
    /// The price of each stock (share)
    /// </summary>
    public double Price { get; set; }
}
