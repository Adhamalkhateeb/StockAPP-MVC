using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServiceContracts;
using StocksApp;

public class StocksController : Controller
{
    private readonly IFinnhubService _finnhubService;
    private readonly TradingOptions _tradingOptions;

    private readonly ILogger<StocksController> _logger;

    public StocksController(
        ILogger<StocksController> logger,
        IOptions<TradingOptions> options,
        IFinnhubService finnhubService
    )
    {
        _logger = logger;
        _finnhubService = finnhubService;
        _tradingOptions = options.Value;
    }

    [Route("/")]
    [Route("[action]/{stock:alpha?}")]
    public async Task<IActionResult> Explore(string? stock, bool showAll = false)
    {
        _logger.LogInformation(
            "Explore requested for stock {StockSymbol}. ShowAll={ShowAll}",
            stock,
            showAll
        );

        var apiStocks = await _finnhubService.GetStocksAsync("US");
        var stocks = showAll
            ? apiStocks?.ToList()
            : apiStocks
                ?.Where(s => s.Symbol != null && _tradingOptions.PopularStocks.Contains(s.Symbol))
                .Take(25)
                .ToList();

        _logger.LogInformation("Explore returning {StocksCount} stocks", stocks?.Count ?? 0);

        ViewBag.Stock = stock;
        return View(
            stocks?.Select(s => new Stock { StockSymbol = s.Symbol, StockName = s.Description })
        );
    }
}
