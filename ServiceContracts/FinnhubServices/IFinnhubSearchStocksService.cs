using Core.DTOs;

namespace ServiceContracts.FinnhubServices
{
    /// <summary>
    /// Represents a service that provides stock search related data.
    /// </summary>
    public interface IFinnhubSearchStocksService
    {
        /// <summary>
        /// Searches for stocks that match the query text using symbol, company name, ISIN, or CUSIP.
        /// </summary>
        /// <param name="query">Query text to search for, e.g., 'Apple' or 'US0378331005'</param>
        /// <param name="exchange">Optional exchange filter, e.g., US</param>
        /// <returns>Search results containing matching symbols as DTOs.</returns>
        Task<SymbolLookupResultDto> SearchStocksAsync(string query, string? exchange = null);
    }
}
