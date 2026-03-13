namespace StocksApp.Models;

/// <summary>
/// Represents the model class to supply trade details (stock id, stock name, price and quantity etc.) to the Trade/Index view
/// </summary>
public class StockTrade
{
    public string? StockSymbol { get; set; }
    public string? StockName { get; set; }
    public decimal Price { get; set; } = 0;
    public uint Quantity { get; set; } = 0;
    public bool CanTrade { get; set; }
    public string? DataUnavailableMessage { get; set; }
}
