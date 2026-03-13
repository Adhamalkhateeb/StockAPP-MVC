using Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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

    [HttpGet]
    [Route("/")]
    [Route("[action]/{stock?}")]
    public async Task<IActionResult> Explore(string? stock, bool showAll = false)
    {
        _logger.LogInformation(
            "Explore requested. Stock={Stock} ShowAll={ShowAll}",
            stock,
            showAll
        );

        IEnumerable<Stock> stocks;

        if (!string.IsNullOrWhiteSpace(stock) && !showAll)
        {
            var symbols = await _finnhubService.SearchStocksAsync(stock);

            stocks =
                symbols.Result?.Select(s => new Stock
                {
                    StockSymbol = s.Symbol,
                    StockName = s.Description,
                }) ?? Enumerable.Empty<Stock>();

            _logger.LogInformation(
                "Search returned {Count} results for {Stock}",
                stocks.Count(),
                stock
            );
        }
        else
        {
            var apiStocks = await _finnhubService.GetStocksAsync("US") ?? [];

            var filteredStocks = showAll
                ? apiStocks
                : apiStocks
                    .Where(s =>
                        s.Symbol != null && _tradingOptions.PopularStocks.Contains(s.Symbol)
                    )
                    .Take(25);

            stocks = filteredStocks.Select(s => new Stock
            {
                StockSymbol = s.Symbol,
                StockName = s.Description,
            });

            _logger.LogInformation("Explore returned {Count} stocks", stocks.Count());
        }

        ViewBag.Stock = stock;
        return View(stocks);
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> Search(string? stock)
    {
        _logger.LogInformation("Search requested for {Stock}", stock);

        if (string.IsNullOrWhiteSpace(stock))
            return RedirectToAction(nameof(Explore));

        return RedirectToAction(nameof(Explore), new { stock });
    }
}
