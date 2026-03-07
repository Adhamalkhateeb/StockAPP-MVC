using Core.DTOs;

namespace RepositoryContracts
{
    /// <summary>
    /// Represents a repository that makes HTTP requests to Finnhub API.
    /// Provides stock, company, and search related data.
    /// </summary>
    public interface IFinnhubRepository
    {
        /// <summary>
        /// Returns company details such as country, currency, exchange, IPO date, logo, market capitalization, name, phone, etc.
        /// </summary>
        /// <param name="stockSymbol">Stock symbol to search, e.g., AAPL</param>
        /// <returns>Company profile information or null if not found.</returns>
        Task<CompanyProfileResponse?> GetCompanyProfileAsync(string stockSymbol);

        /// <summary>
        /// Returns real-time stock price details such as current price, change, percent change, daily high/low, open price, and previous close.
        /// </summary>
        /// <param name="stockSymbol">Stock symbol to search, e.g., AAPL</param>
        /// <returns>Stock quote information or null if not found.</returns>
        Task<StockQuoteResponse?> GetStockPriceQuoteAsync(string stockSymbol);

        /// <summary>
        /// Returns a list of all stocks supported by a given exchange.
        /// </summary>
        /// <param name="exchange">Exchange code, e.g., US</param>
        /// <param name="mic">Optional MIC filter for the exchange</param>
        /// <param name="securityType">Optional security type filter, e.g., Common Stock</param>
        /// <param name="currency">Optional currency filter, e.g., USD</param>
        /// <returns>List of supported stock symbols or null if none found.</returns>
        Task<List<StockSymbolResponse>?> GetStocksAsync(
            string exchange,
            string? mic = null,
            string? securityType = null,
            string? currency = null
        );

        /// <summary>
        /// Searches for stocks that match the given query, using symbol, company name, ISIN, or CUSIP.
        /// </summary>
        /// <param name="query">Query text, e.g., 'Apple' or 'US0378331005'</param>
        /// <param name="exchange">Optional exchange filter</param>
        /// <returns>Search results containing matching symbols.</returns>
        Task<SymbolLookupResultDto?> SearchStocksAsync(string query, string? exchange = null);
    }
}
