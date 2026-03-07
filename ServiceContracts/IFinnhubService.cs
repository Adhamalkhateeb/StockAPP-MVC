using Core.DTOs;

namespace ServiceContracts
{
    /// <summary>
    /// Represents a service that provides stock, company, and search related data.
    /// Wraps FinnhubRepository and can contain business logic or transformations.
    /// </summary>
    public interface IFinnhubService
    {
        /// <summary>
        /// Returns detailed information about a company, such as country, currency, exchange, IPO date, logo, market capitalization, name, and phone number.
        /// </summary>
        /// <param name="stockSymbol">Stock symbol to search, e.g., AAPL</param>
        /// <returns>Company profile information or null if not found.</returns>
        Task<CompanyProfileResponse?> GetCompanyProfileAsync(string stockSymbol);

        /// <summary>
        /// Returns real-time stock quote information including current price, daily change, percent change, daily high/low, open price, and previous close.
        /// </summary>
        /// <param name="stockSymbol">Stock symbol to search, e.g., AAPL</param>
        /// <returns>Stock quote information or null if not found.</returns>
        Task<StockQuoteResponse?> GetStockPriceQuoteAsync(string stockSymbol);

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
        /// Searches for stocks that match the query text using symbol, company name, ISIN, or CUSIP.
        /// </summary>
        /// <param name="query">Query text to search for, e.g., 'Apple' or 'US0378331005'</param>
        /// <param name="exchange">Optional exchange filter, e.g., US</param>
        /// <returns>Search results containing matching symbols as DTOs.</returns>
        Task<SymbolLookupResultDto?> SearchStocksAsync(string query, string? exchange = null);
    }
}
