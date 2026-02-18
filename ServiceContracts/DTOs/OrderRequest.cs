using System;

namespace ServiceContracts.DTOs;


public abstract class OrderRequest
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
    public double Price { get; set; }
}

