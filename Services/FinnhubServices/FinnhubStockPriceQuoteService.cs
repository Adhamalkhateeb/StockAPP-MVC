using Core.DTOs;
using Core.Exceptions;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts.FinnhubServices;

namespace Services
{
    public class FinnhubStockPriceQuoteService : IFinnhubStockPriceQuoteService
    {
        private readonly IFinnhubRepository _finnhubRepository;
        private readonly ILogger<FinnhubStockPriceQuoteService> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="FinnhubStockPriceQuoteService"/>.
        /// </summary>
        public FinnhubStockPriceQuoteService(
            IFinnhubRepository finnhubRepository,
            ILogger<FinnhubStockPriceQuoteService> logger
        )
        {
            _finnhubRepository = finnhubRepository;
            _logger = logger;
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
    }
}
