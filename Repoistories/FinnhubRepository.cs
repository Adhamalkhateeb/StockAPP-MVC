using System.Diagnostics;
using System.Text.Json;
using Core.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="FinnhubRepository"/> class.
        /// </summary>
        public FinnhubRepository(
            IHttpClientFactory clientFactory,
            IConfiguration config,
            IMemoryCache memoryCache
        )
        {
            _clientFactory = clientFactory;
            _memoryCache = memoryCache;
            _token =
                config["FinnhubToken"]
                ?? throw new InvalidOperationException("Finnhub token cannot be empty");
        }

        /// <inheritdoc/>
        public async Task<CompanyProfileResponse?> GetCompanyProfileAsync(string stockSymbol)
        {
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
        public async Task<SymbolLookupResultDto?> SearchStocksAsync(
            string query,
            string? exchange = null
        )
        {
            string url = $"https://finnhub.io/api/v1/search?q={query}&token={_token}";
            if (!string.IsNullOrWhiteSpace(exchange))
                url += $"&exchange={exchange}";

            string cacheKey = $"finnhub:search:{query.Trim().ToUpperInvariant()}";
            return await GetOrSetCacheAsync(
                cacheKey,
                TimeSpan.FromMinutes(10),
                () => GetFinnhubApiResultAsync<SymbolLookupResultDto>(url)
            );
        }

        /// <summary>
        /// Sends HTTP request to Finnhub API and deserializes the JSON response into the specified DTO.
        /// </summary>
        private async Task<T?> GetFinnhubApiResultAsync<T>(string api)
        {
            var client = _clientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(2);

            var responseMessage = await client.GetAsync(
                api,
                HttpCompletionOption.ResponseHeadersRead
            );
            responseMessage.EnsureSuccessStatusCode();

            var json = await responseMessage.Content.ReadAsStringAsync();
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
            var result =
                JsonSerializer.Deserialize<T>(json, options)
                ?? throw new InvalidOperationException("No response returned from Finnhub server");

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
                        return await factory();
                    }
                ) ?? throw new InvalidOperationException("Unable to store data in cache");
        }
    }
}
