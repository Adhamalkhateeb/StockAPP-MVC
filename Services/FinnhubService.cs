using Core.DTOs;
using RepositoryContracts;
using ServiceContracts;

namespace Services
{
    /// <summary>
    /// Implementation of <see cref="IFinnhubService"/> that wraps <see cref="IFinnhubRepository"/>
    /// and provides stock, company, and search services.
    /// </summary>
    public class FinnhubService : IFinnhubService
    {
        private readonly IFinnhubRepository _finnhubRepository;

        /// <summary>
        /// Initializes a new instance of <see cref="FinnhubService"/>.
        /// </summary>
        public FinnhubService(IFinnhubRepository finnhubRepository)
        {
            _finnhubRepository = finnhubRepository;
        }

        /// <inheritdoc/>
        public async Task<CompanyProfileResponse?> GetCompanyProfileAsync(string stockSymbol)
        {
            var result = await _finnhubRepository.GetCompanyProfileAsync(stockSymbol);

            if (result == null)
                return null;

            return result;
        }

        /// <inheritdoc/>
        public async Task<StockQuoteResponse?> GetStockPriceQuoteAsync(string stockSymbol)
        {
            var result = await _finnhubRepository.GetStockPriceQuoteAsync(stockSymbol);

            if (result == null)
                return null;

            return result;
        }

        /// <inheritdoc/>
        public async Task<List<StockSymbolResponse>?> GetStocksAsync(
            string exchange,
            string? mic = null,
            string? securityType = null,
            string? currency = null
        )
        {
            var result = await _finnhubRepository.GetStocksAsync(
                exchange,
                mic,
                securityType,
                currency
            );

            if (result == null || result.Count == 0)
                return null;

            return result;
        }

        /// <inheritdoc/>
        public async Task<SymbolLookupResultDto?> SearchStocksAsync(
            string query,
            string? exchange = null
        )
        {
            var result = await _finnhubRepository.SearchStocksAsync(query, exchange);

            if (result == null || result.Result == null || result.Result.Count == 0)
                return null;

            return result;
        }
    }
}
