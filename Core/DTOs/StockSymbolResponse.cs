namespace Core.DTOs
{
    /// <summary>
    /// Represents a stock symbol listed on a stock exchange.
    /// Returned from the Finnhub API /stock/symbol endpoint.
    /// </summary>
    public class StockSymbolResponse
    {
        /// <summary>
        /// Gets or sets the unique symbol used to identify the stock.
        /// Example: AAPL
        /// </summary>
        public string? Symbol { get; set; }

        /// <summary>
        /// Gets or sets the human-readable description of the company or security.
        /// Example: Apple Inc
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the display symbol name.
        /// Usually matches Symbol but can differ in some exchanges.
        /// </summary>
        public string? DisplaySymbol { get; set; }

        /// <summary>
        /// Gets or sets the currency in which the stock price is denominated.
        /// Example: USD
        /// </summary>
        public string? Currency { get; set; }

        /// <summary>
        /// Gets or sets the FIGI (Financial Instrument Global Identifier) for the stock.
        /// </summary>
        public string? Figi { get; set; }

        /// <summary>
        /// Gets or sets the ISIN (International Securities Identification Number).
        /// Only available for EU stocks and selected Asian markets.
        /// </summary>
        public string? Isin { get; set; }

        /// <summary>
        /// Gets or sets the MIC (Market Identifier Code) of the primary exchange.
        /// Example: XNAS for NASDAQ.
        /// </summary>
        public string? Mic { get; set; }

        /// <summary>
        /// Gets or sets the Share Class FIGI.
        /// </summary>
        public string? ShareClassFIGI { get; set; }

        /// <summary>
        /// Gets or sets the alternative ticker for exchanges with multiple tickers for a single stock.
        /// </summary>
        public string? Symbol2 { get; set; }

        /// <summary>
        /// Gets or sets the type of security.
        /// Examples: Common Stock, ETF, ADR
        /// </summary>
        public string? Type { get; set; }
    }
}
