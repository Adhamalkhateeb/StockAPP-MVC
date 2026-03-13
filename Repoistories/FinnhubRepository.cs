using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Core.DTOs;
using Core.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RepositoryContracts;

namespace Repositories
{
    /// <summary>
    /// Repository implementation for Finnhub API.
    /// Provides stock quotes, company profiles, stock lists, and symbol search.
    /// </summary>
    public class FinnhubRepository : IFinnhubRepository
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly string _token;
        private readonly ILogger<FinnhubRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FinnhubRepository"/> class.
        /// </summary>
        public FinnhubRepository(
            IHttpClientFactory clientFactory,
            IConfiguration config,
            IMemoryCache memoryCache,
            ILogger<FinnhubRepository> logger
        )
        {
            _clientFactory = clientFactory;
            _memoryCache = memoryCache;
            _logger = logger;
            _token =
                config["FinnhubToken"]
                ?? throw new InvalidOperationException("Finnhub token cannot be empty");
        }

        /// <inheritdoc/>
        public async Task<CompanyProfileResponse?> GetCompanyProfileAsync(string stockSymbol)
        {
            _logger.LogInformation(
                "Repository fetching company profile for symbol {StockSymbol}",
                stockSymbol
            );

            string cacheKey = $"finnhub:company-profile:{stockSymbol.ToUpperInvariant()}";
            return await GetOrSetCacheAsync(
                cacheKey,
                TimeSpan.FromMinutes(15),
                () =>
                    GetFinnhubApiResultAsync<CompanyProfileResponse>(
                        $"https://finnhub.io/api/v1/stock/profile2?symbol={stockSymbol}&token={_token}"
                    )
            );
        }

        /// <inheritdoc/>
        public async Task<StockQuoteResponse?> GetStockPriceQuoteAsync(string stockSymbol)
        {
            _logger.LogInformation(
                "Repository fetching quote for symbol {StockSymbol}",
                stockSymbol
            );

            string cacheKey = $"finnhub:quote:{stockSymbol.ToUpperInvariant()}";
            return await GetOrSetCacheAsync(
                cacheKey,
                TimeSpan.FromSeconds(30),
                () =>
                    GetFinnhubApiResultAsync<StockQuoteResponse>(
                        $"https://finnhub.io/api/v1/quote?symbol={stockSymbol}&token={_token}"
                    )
            );
        }

        /// <inheritdoc/>
        public async Task<List<StockSymbolResponse>?> GetStocksAsync(
            string exchange,
            string? mic = null,
            string? securityType = null,
            string? currency = null
        )
        {
            _logger.LogInformation("Repository fetching stocks for exchange {Exchange}", exchange);

            string url =
                $"https://finnhub.io/api/v1/stock/symbol?exchange={exchange}&token={_token}";

            if (!string.IsNullOrWhiteSpace(mic))
                url += $"&mic={mic}";
            if (!string.IsNullOrWhiteSpace(securityType))
                url += $"&securityType={securityType}";
            if (!string.IsNullOrWhiteSpace(currency))
                url += $"&currency={currency}";

            string cacheKey = $"finnhub:stocks:{exchange.ToUpperInvariant()}";
            return await GetOrSetCacheAsync(
                cacheKey,
                TimeSpan.FromHours(12),
                () => GetFinnhubApiResultAsync<List<StockSymbolResponse>>(url)
            );
        }

        /// <inheritdoc/>
        public async Task<SymbolLookupResultDto> SearchStocksAsync(
            string query,
            string? exchange = null
        )
        {
            _logger.LogInformation("Repository searching stocks for query {Query}", query);

            string url = $"https://finnhub.io/api/v1/search?q={query}&token={_token}";
            if (!string.IsNullOrWhiteSpace(exchange))
                url += $"&exchange={exchange}";

            string cacheKey = $"finnhub:search:{query.Trim().ToUpperInvariant()}";

            SymbolLookupResultDto? result = await GetOrSetCacheAsync(
                cacheKey,
                TimeSpan.FromMinutes(10),
                () => GetFinnhubApiResultAsync<SymbolLookupResultDto>(url)
            );

            if (result == null)
            {
                _logger.LogWarning("Finnhub search returned null for query {Query}", query);
                result = new SymbolLookupResultDto();
            }
            return result;
        }

        /// <summary>
        /// Sends HTTP request to Finnhub API and deserializes the JSON response into the specified DTO.
        /// </summary>
        private async Task<T?> GetFinnhubApiResultAsync<T>(string apiUrl)
        {
            _logger.LogDebug("Calling Finnhub endpoint {Endpoint}", apiUrl);

            var client = _clientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(40);

            HttpResponseMessage responseMessage;

            try
            {
                responseMessage = await client.GetAsync(
                    apiUrl,
                    HttpCompletionOption.ResponseHeadersRead
                );
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Finnhub request timed out for {ApiUrl}", apiUrl);
                throw new InvalidOperationException(
                    $"Finnhub request timed out for URL: {apiUrl}",
                    ex
                );
            }

            if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
            {
                _logger.LogWarning("Finnhub returned 403 for {ApiUrl}", apiUrl);
                throw new FinnhubAccessDeniedException(apiUrl);
            }

            if (!responseMessage.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Finnhub returned non-success status {StatusCode} for {ApiUrl}",
                    (int)responseMessage.StatusCode,
                    apiUrl
                );
                throw new InvalidOperationException(
                    $"Finnhub returned HTTP {(int)responseMessage.StatusCode} for URL: {apiUrl}"
                );
            }

            _logger.LogDebug(
                "Finnhub response received with status code {StatusCode}",
                (int)responseMessage.StatusCode
            );

            var json = await responseMessage.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(json) || json.Trim() == "{}")
            {
                _logger.LogWarning("Finnhub returned empty response for {ApiUrl}", apiUrl);
                throw new InvalidOperationException("No response returned from Finnhub server");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System
                    .Text
                    .Json
                    .Serialization
                    .JsonNumberHandling
                    .AllowReadingFromString,
            };

            T? result;
            try
            {
                result = JsonSerializer.Deserialize<T>(json, options);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize Finnhub response for {ApiUrl}", apiUrl);
                throw new InvalidOperationException(
                    "Failed to parse response from Finnhub server",
                    ex
                );
            }

            if (result == null)
                throw new InvalidOperationException("No response returned from Finnhub server");

            return result;
        }

        private Task<T?> GetOrSetCacheAsync<T>(
            string cacheKey,
            TimeSpan absoluteExpiration,
            Func<Task<T?>> factory
        )
        {
            return _memoryCache.GetOrCreateAsync(
                    cacheKey,
                    async entry =>
                    {
                        entry.AbsoluteExpirationRelativeToNow = absoluteExpiration;
                        _logger.LogDebug("Cache miss for key {CacheKey}", cacheKey);
                        var value = await factory();
                        _logger.LogDebug("Cache populated for key {CacheKey}", cacheKey);
                        return value;
                    }
                ) ?? throw new InvalidOperationException("Unable to store data in cache");
        }
    }
}
