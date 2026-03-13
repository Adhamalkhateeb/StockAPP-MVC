using Core.DTOs;
using Core.Exceptions;
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

            try
            {
                var result = await _finnhubRepository.GetCompanyProfileAsync(stockSymbol);

                if (result == null)
                    _logger.LogWarning(
                        "Company profile not found for symbol {StockSymbol}",
                        stockSymbol
                    );
                else
                    _logger.LogInformation(
                        "Company profile fetched for symbol {StockSymbol}",
                        stockSymbol
                    );

                return result;
            }
            catch (FinnhubAccessDeniedException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Live company profile data is unavailable for symbol {StockSymbol}",
                    stockSymbol
                );
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error fetching company profile for symbol {StockSymbol}",
                    stockSymbol
                );
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<StockQuoteResponse?> GetStockPriceQuoteAsync(string stockSymbol)
        {
            _logger.LogInformation("Fetching quote for symbol {StockSymbol}", stockSymbol);

            try
            {
                var result = await _finnhubRepository.GetStockPriceQuoteAsync(stockSymbol);

                if (result == null)
                    _logger.LogWarning("Quote not found for symbol {StockSymbol}", stockSymbol);
                else
                    _logger.LogInformation("Quote fetched for symbol {StockSymbol}", stockSymbol);

                return result;
            }
            catch (FinnhubAccessDeniedException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Live quote data is unavailable for symbol {StockSymbol}",
                    stockSymbol
                );
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error fetching quote for symbol {StockSymbol}",
                    stockSymbol
                );
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<StockSnapshotResponse> GetStockSnapshotAsync(string stockSymbol)
        {
            var snapshot = new StockSnapshotResponse { StockSymbol = stockSymbol };

            try
            {
                var profileTask = _finnhubRepository.GetCompanyProfileAsync(stockSymbol);
                var quoteTask = _finnhubRepository.GetStockPriceQuoteAsync(stockSymbol);

                await Task.WhenAll(profileTask, quoteTask);

                snapshot.CompanyProfile = profileTask.Result;
                snapshot.StockQuote = quoteTask.Result;
            }
            catch (FinnhubAccessDeniedException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Finnhub denied live market data for symbol {StockSymbol}",
                    stockSymbol
                );

                snapshot.CompanyProfile = null;
                snapshot.StockQuote = null;
                snapshot.IsAccessDenied = true;
                snapshot.IsLiveDataAvailable = false;
                snapshot.UserMessage =
                    "Live market data is temporarily unavailable for this symbol. Please try again shortly or pick another stock.";

                return snapshot;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error loading snapshot for symbol {StockSymbol}",
                    stockSymbol
                );

                snapshot.CompanyProfile = null;
                snapshot.StockQuote = null;
                snapshot.IsAccessDenied = false;
                snapshot.IsLiveDataAvailable = false;
                snapshot.UserMessage =
                    "An unexpected error occurred while loading market data. Please refresh or try another stock.";

                return snapshot;
            }

            snapshot.IsLiveDataAvailable =
                snapshot.CompanyProfile != null && snapshot.StockQuote != null;

            if (!snapshot.IsLiveDataAvailable)
            {
                snapshot.UserMessage =
                    "We could not load complete market data for this symbol right now. Please try another stock or refresh later.";
            }

            return snapshot;
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

            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch stocks for exchange {Exchange}", exchange);
                return null;
            }
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
