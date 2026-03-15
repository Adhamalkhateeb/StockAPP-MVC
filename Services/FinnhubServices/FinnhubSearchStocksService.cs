using Core.DTOs;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts.FinnhubServices;

namespace Services
{
    public class FinnhubSearchStocksService : IFinnhubSearchStocksService
    {
        private readonly IFinnhubRepository _finnhubRepository;
        private readonly ILogger<FinnhubSearchStocksService> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="FinnhubSearchStocksService"/>.
        /// </summary>
        public FinnhubSearchStocksService(
            IFinnhubRepository finnhubRepository,
            ILogger<FinnhubSearchStocksService> logger
        )
        {
            _finnhubRepository = finnhubRepository;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<SymbolLookupResultDto> SearchStocksAsync(
            string query,
            string? exchange = null
        )
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("Stock search attempted with empty query");
                return new SymbolLookupResultDto();
            }

            _logger.LogInformation(
                "Searching stocks with query {Query} and exchange {Exchange}",
                query,
                exchange
            );

            try
            {
                SymbolLookupResultDto result = await _finnhubRepository.SearchStocksAsync(
                    query,
                    exchange
                );

                if (result.Result?.Any() != true)
                {
                    _logger.LogInformation(
                        "No stock search results found for query {Query}",
                        query
                    );
                    return result ?? new SymbolLookupResultDto();
                }

                _logger.LogInformation(
                    "Stock search returned {ResultsCount} results for query {Query}",
                    result.Result.Count,
                    query
                );

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search stocks for query {Query}", query);
                return new SymbolLookupResultDto();
            }
        }
    }
}
