using ServiceContracts;
using System.Text.Json;
using Microsoft.Extensions.Configuration;



public class FinnhubService : IFinnhubService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    public FinnhubService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _config = configuration;
    }

    public async Task<Dictionary<string, object>?> GetCompanyProfile(string stockSymbol)
    {
        using var httpClient = _httpClientFactory.CreateClient();

        var responseMessage = await httpClient.GetAsync(
            $"https://finnhub.io/api/v1/stock/profile2?symbol={stockSymbol}&token={_config["FinnhubToken"]}"
        );

        responseMessage.EnsureSuccessStatusCode();

        var response = await responseMessage.Content.ReadAsStringAsync();

        var responseDict = JsonSerializer.Deserialize<Dictionary<string, object>?>(response) ??
        throw new InvalidOperationException("No response returned from Finnhub server");

        if (responseDict.ContainsKey("error"))
            throw new InvalidOperationException(Convert.ToString(responseDict["error"]));

        return responseDict;
    }

    public async Task<Dictionary<string, object>?> GetStockPriceQuote(string stockSymbol)
    {
        using var httpClient = _httpClientFactory.CreateClient();

        var responseMessage = await httpClient.GetAsync(
            $"https://finnhub.io/api/v1/quote?symbol={stockSymbol}&token={_config["FinnhubToken"]}"
        );

        responseMessage.EnsureSuccessStatusCode();

        var response = await responseMessage.Content.ReadAsStringAsync();

        var responseDict = JsonSerializer.Deserialize<Dictionary<string, object>?>(response)
                            ?? throw new InvalidOperationException("No response returned from Finnhub server");

        if (responseDict.ContainsKey("error"))
            throw new InvalidOperationException(Convert.ToString(responseDict["error"]));

        return responseDict;
    }
}
