using Core.DTOs;

namespace ServiceContracts.FinnhubServices
{
    /// <summary>
    /// Represents a service that provides company related data.
    /// </summary>
    public interface IFinnhubCompanyProfileService
    {
        /// <summary>
        /// Returns detailed information about a company, such as country, currency, exchange, IPO date, logo, market capitalization, name, and phone number.
        /// </summary>
        /// <param name="stockSymbol">Stock symbol to search, e.g., AAPL</param>
        /// <returns>Company profile information or null if not found.</returns>
        Task<CompanyProfileResponse?> GetCompanyProfileAsync(string stockSymbol);
    }
}
