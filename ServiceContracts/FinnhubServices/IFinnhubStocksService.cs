using Core.DTOs;

namespace ServiceContracts.FinnhubServices
{
    /// <summary>
    /// Represents a service that provides stock, company, and search related data.
    /// </summary>
    public interface IFinnhubStocksService
    {
        /// <summary>
        /// Returns a list of all supported stocks for a given exchange.
        /// </summary>
        /// <param name="exchange">Exchange code, e.g., US</param>
        /// <param name="mic">Optional MIC code for exchange</param>
        /// <param name="securityType">Optional security type filter, e.g., Common Stock</param>
        /// <param name="currency">Optional currency filter, e.g., USD</param>
        /// <returns>List of supported stocks as DTOs.</returns>
        Task<List<StockSymbolResponse>?> GetStocksAsync(
            string exchange,
            string? mic = null,
            string? securityType = null,
            string? currency = null
        );

        /// <summary>
        /// Returns a company profile and quote together with availability metadata for UI fallbacks.
        /// </summary>
        /// <param name="stockSymbol">Stock symbol to search, e.g., AAPL</param>
        /// <returns>Combined live market data result for the requested stock.</returns>
        Task<StockSnapshotResponse> GetStockSnapshotAsync(string stockSymbol);
    }
}
