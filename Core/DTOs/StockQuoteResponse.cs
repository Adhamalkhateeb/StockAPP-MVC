using System.Text.Json.Serialization;

namespace Core.DTOs;

/// <summary>
/// Represents stock price quote information returned from Finnhub API.
/// Contains real-time pricing details such as current price,
/// price change, percentage change, and daily high/low values.
/// </summary>
public class StockQuoteResponse
{
    /// <summary>
    /// Gets or sets the current trading price of the stock.
    /// </summary>
    [JsonPropertyName("c")]
    public decimal? CurrentPrice { get; set; }

    /// <summary>
    /// Gets or sets the absolute change in price compared to the previous close.
    /// </summary>
    [JsonPropertyName("d")]
    public decimal? Change { get; set; }

    /// <summary>
    /// Gets or sets the percentage change in price compared to the previous close.
    /// </summary>
    [JsonPropertyName("dp")]
    public decimal? PercentChange { get; set; }

    /// <summary>
    /// Gets or sets the highest trading price of the stock during the current trading day.
    /// </summary>
    [JsonPropertyName("h")]
    public decimal? HighPrice { get; set; }

    /// <summary>
    /// Gets or sets the lowest trading price of the stock during the current trading day.
    /// </summary>
    [JsonPropertyName("l")]
    public decimal? LowPrice { get; set; }

    /// <summary>
    /// Gets or sets the opening price of the stock when the market opened.
    /// </summary>
    [JsonPropertyName("o")]
    public decimal? OpenPrice { get; set; }

    /// <summary>
    /// Gets or sets the previous trading day's closing price.
    /// </summary>
    [JsonPropertyName("pc")]
    public decimal? PreviousClosePrice { get; set; }
}
