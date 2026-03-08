using Core.DTOs;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<FinnhubService> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="FinnhubService"/>.
        /// </summary>
        public FinnhubService(IFinnhubRepository finnhubRepository, ILogger<FinnhubService> logger)
        {
            _finnhubRepository = finnhubRepository;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<CompanyProfileResponse?> GetCompanyProfileAsync(string stockSymbol)
        {
            _logger.LogInformation(
                "Fetching company profile for symbol {StockSymbol}",
                stockSymbol
            );

            var result = await _finnhubRepository.GetCompanyProfileAsync(stockSymbol);

            if (result == null)
            {
                _logger.LogWarning(
                    "Company profile not found for symbol {StockSymbol}",
                    stockSymbol
                );
                return null;
            }

            _logger.LogInformation("Company profile fetched for symbol {StockSymbol}", stockSymbol);

            return result;
        }

        /// <inheritdoc/>
        public async Task<StockQuoteResponse?> GetStockPriceQuoteAsync(string stockSymbol)
        {
            _logger.LogInformation("Fetching quote for symbol {StockSymbol}", stockSymbol);

            var result = await _finnhubRepository.GetStockPriceQuoteAsync(stockSymbol);

            if (result == null)
            {
                _logger.LogWarning("Quote not found for symbol {StockSymbol}", stockSymbol);
                return null;
            }

            _logger.LogInformation("Quote fetched for symbol {StockSymbol}", stockSymbol);

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
            _logger.LogInformation("Fetching stocks list for exchange {Exchange}", exchange);

            var result = await _finnhubRepository.GetStocksAsync(
                exchange,
                mic,
                securityType,
                currency
            );

            if (result == null || result.Count == 0)
            {
                _logger.LogWarning("No stocks returned for exchange {Exchange}", exchange);
                return null;
            }

            _logger.LogInformation(
                "Fetched {StocksCount} stocks for exchange {Exchange}",
                result.Count,
                exchange
            );

            return result;
        }

        /// <inheritdoc/>
        public async Task<SymbolLookupResultDto?> SearchStocksAsync(
            string query,
            string? exchange = null
        )
        {
            _logger.LogInformation(
                "Searching stocks with query {Query} and exchange {Exchange}",
                query,
                exchange
            );

            var result = await _finnhubRepository.SearchStocksAsync(query, exchange);

            if (result == null || result.Result == null || result.Result.Count == 0)
            {
                _logger.LogWarning("No stock search results found for query {Query}", query);
                return null;
            }

            _logger.LogInformation(
                "Stock search returned {ResultsCount} results for query {Query}",
                result.Result.Count,
                query
            );

            return result;
        }
    }
}
