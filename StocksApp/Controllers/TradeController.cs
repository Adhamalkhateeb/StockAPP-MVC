using System.Formats.Asn1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServiceContracts;
using ServiceContracts.DTOs;
using ServiceContracts.Interfaces;
using StocksApp.Models;


namespace StocksApp.Controllers
{

    [Route("[controller]")]
    public class TradeController : Controller
    {
        private readonly TradingOptions _tradingOptions;

        private readonly IFinnhubService _finnhubService;
        private readonly IStocksService _stocksService;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor for TradeController that executes when a new object is created for the class
        /// </summary>
        /// <param name="tradingOptions">Injecting TradeOptions config through Options pattern</param>
        /// <param name="stocksService">Injecting StocksService</param>
        /// <param name="finnhubService">Injecting FinnhubService</param>
        /// <param name="configuration">Injecting IConfiguration</param>
        public TradeController(IOptions<TradingOptions> tradingOptions, IStocksService stocksService, IFinnhubService finnhubService, IConfiguration configuration)
        {
            _tradingOptions = tradingOptions.Value;
            _stocksService = stocksService;
            _finnhubService = finnhubService;
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


            if (companyProfile != null && stockQuote != null)
            {
                stockTrade = new StockTrade()
                {
                    StockSymbol = companyProfile["ticker"].ToString(),
                    StockName = companyProfile["name"].ToString(),
                    Quantity = _tradingOptions.DefaultOrderQuantity ?? 0,
                    Price = Convert.ToDouble(stockQuote["c"].ToString())
                };
            }

            ViewBag.FinnhubToken = _configuration["FinnhubToken"];

            return View(stockTrade);
        }

        [Route("[action]")]
        public async Task<IActionResult> Orders()
        {
            var orders = new Orders
            {
                BuyOrders = await _stocksService.GetBuyOrdersAsync(),
                SellOrders = await _stocksService.GetSellOrdersAsync()
            };
            ViewBag.TradingOptions = _tradingOptions;

            return View(orders);
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> BuyOrder(BuyOrderRequest request)
        {

            request.DateAndTimeOfOrder = DateTime.UtcNow;

            ModelState.Clear();
            TryValidateModel(request);

            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage)
                                    .ToList();

                StockTrade stockTrade = new StockTrade()
                {
                    StockName = request.StockName,
                    Quantity = request.Quantity,
                    StockSymbol = request.StockSymbol
                };

                return View("Index", stockTrade);
            }

            var buyOrderResponse = await _stocksService.CreateBuyOrderAsync(request);

            return RedirectToAction(nameof(Orders));


        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> SellOrder(SellOrderRequest request)
        {
            request.DateAndTimeOfOrder = DateTime.UtcNow;

            ModelState.Clear();
            TryValidateModel(request);

            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage)
                                    .ToList();

                StockTrade stockTrade = new StockTrade()
                {
                    StockName = request.StockName,
                    Quantity = request.Quantity,
                    StockSymbol = request.StockSymbol
                };

                return View("Index", stockTrade);
            }

            var sellOrderResponse = await _stocksService.CreateSellOrderAsync(request);

            return RedirectToAction(nameof(Orders));
        }

    }
}
