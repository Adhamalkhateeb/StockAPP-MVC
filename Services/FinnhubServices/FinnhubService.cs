using Core.DTOs;
using Core.Exceptions;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.FinnhubServices;

namespace Services
{
    public class FinnhubStocksService : IFinnhubStocksService
    {
        private readonly IFinnhubRepository _finnhubRepository;
        private readonly ILogger<FinnhubStocksService> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="FinnhubStocksService"/>.
        /// </summary>
        public FinnhubStocksService(
            IFinnhubRepository finnhubRepository,
            ILogger<FinnhubStocksService> logger
        )
        {
            _finnhubRepository = finnhubRepository;
            _logger = logger;
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
    }
}
