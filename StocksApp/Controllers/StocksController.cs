using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServiceContracts;
using StocksApp;

public class StocksController : Controller
{
    private readonly IFinnhubService _finnhubService;
    private readonly TradingOptions _tradingOptions;

    public StocksController(IOptions<TradingOptions> options, IFinnhubService finnhubService)
    {
        _finnhubService = finnhubService;
        _tradingOptions = options.Value;
    }

    [Route("/")]
    [Route("[action]/{stock:alpha?}")]
    public async Task<IActionResult> Explore(string? stock, bool showAll = false)
    {
        var apiStocks = await _finnhubService.GetStocksAsync("US");
        var stocks = showAll
            ? apiStocks?.ToList()
            : apiStocks
                ?.Where(s => s.Symbol != null && _tradingOptions.PopularStocks.Contains(s.Symbol))
                .Take(25)
                .ToList();

        ViewBag.Stock = stock;
        return View(
            stocks?.Select(s => new Stock { StockSymbol = s.Symbol, StockName = s.Description })
        );
    }
}
