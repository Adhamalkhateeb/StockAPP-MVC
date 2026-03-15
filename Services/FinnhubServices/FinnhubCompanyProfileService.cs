using Core.DTOs;
using Core.Exceptions;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts.FinnhubServices;

namespace Services
{
    public class FinnhubCompanyProfileService : IFinnhubCompanyProfileService
    {
        private readonly IFinnhubRepository _finnhubRepository;
        private readonly ILogger<FinnhubCompanyProfileService> _logger;

        public FinnhubCompanyProfileService(
            IFinnhubRepository finnhubRepository,
            ILogger<FinnhubCompanyProfileService> logger
        )
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
    }
}
