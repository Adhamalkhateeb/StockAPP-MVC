using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServiceContracts;
using ServiceContracts.Interfaces;
using StocksApp.Models;


namespace StocksApp.Controllers
{

    [Route("Trade")]
    public class TradeController : Controller
    {
        private readonly IFinnhubService _finnhubService;
        private readonly IStockService _stockService;
        private readonly TradingOptions _tradingOptions;
        private readonly IConfiguration _configuration;


        public TradeController(IFinnhubService finnhubService, IStockService stockService, IOptions<TradingOptions> tradingOptions, IConfiguration configuration)
        {
            _finnhubService = finnhubService;
            _stockService = stockService;

            _tradingOptions = tradingOptions.Value;
            _configuration = configuration;
        }

        [Route("/")]
        [Route("[action]")]
        [Route("~/[controller]")]
        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(_tradingOptions.DefaultStockSymbol))
                _tradingOptions.DefaultStockSymbol = "MSFT";

            var companyProfile = await _finnhubService.GetCompanyProfile(_tradingOptions.DefaultStockSymbol);

            var stockQuote = await _finnhubService.GetStockPriceQuote(_tradingOptions.DefaultStockSymbol!);

            StockTrade stockTrade = new StockTrade() { StockSymbol = _tradingOptions.DefaultStockSymbol };


            if (companyProfile is not null && stockQuote is not null)
            {
                stockTrade.StockSymbol = Convert.ToString(companyProfile["ticker"].ToString());
                stockTrade.StockName = Convert.ToString(companyProfile["name"].ToString());
                stockTrade.Price = Convert.ToDouble(stockQuote["c"].ToString());
            }

            ViewBag.FinnhubToken = _configuration["FinnhubToken"];

            return View(stockTrade);
        }

    }
}
