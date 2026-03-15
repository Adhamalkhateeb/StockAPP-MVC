using Core.DTOs;

namespace ServiceContracts.FinnhubServices
{
    /// <summary>
    /// Represents a service that provides stocks prices related data.
    /// </summary>
    public interface IFinnhubStockPriceQuoteService
    {
        /// <summary>
        /// Returns real-time stock quote information including current price, daily change, percent change, daily high/low, open price, and previous close.
        /// </summary>
        /// <param name="stockSymbol">Stock symbol to search, e.g., AAPL</param>
        /// <returns>Stock quote information or null if not found.</returns>
        Task<StockQuoteResponse?> GetStockPriceQuoteAsync(string stockSymbol);
    }
}
