namespace Core.DTOs
{
    /// <summary>
    /// Represents the search result from Finnhub's Symbol Lookup endpoint.
    /// Used when searching for stocks by symbol, name, ISIN, or CUSIP.
    /// </summary>
    public class SymbolLookupResultDto
    {
        /// <summary>
        /// Gets or sets the number of matching results.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the array of matching symbols.
        /// </summary>
        public List<SymbolLookupItemDto>? Result { get; set; }
    }

    /// <summary>
    /// Represents an individual stock search result.
    /// </summary>
    public class SymbolLookupItemDto
    {
        /// <summary>
        /// Gets or sets the symbol description or company name.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the display symbol name.
        /// </summary>
        public string? DisplaySymbol { get; set; }

        /// <summary>
        /// Gets or sets the unique symbol used to identify this stock in other endpoints.
        /// </summary>
        public string? Symbol { get; set; }

        /// <summary>
        /// Gets or sets the security type, e.g., Common Stock, REIT, ETF.
        /// </summary>
        public string? Type { get; set; }
    }
}
