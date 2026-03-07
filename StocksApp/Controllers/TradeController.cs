using Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;
using ServiceContracts;
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
        public TradeController(
            IOptions<TradingOptions> tradingOptions,
            IStocksService stocksService,
            IFinnhubService finnhubService,
            IConfiguration configuration
        )
        {
            _tradingOptions = tradingOptions.Value;
            _stocksService = stocksService;
            _finnhubService = finnhubService;
            _configuration = configuration;
        }

        [Route("[action]/{stockSymbol?}")]
        [Route("~/[controller]/{stockSymbol?}")]
        public async Task<IActionResult> Index(string? stockSymbol)
        {
            if (string.IsNullOrEmpty(stockSymbol))
                stockSymbol = "MSFT";

            var companyProfile = await _finnhubService.GetCompanyProfileAsync(stockSymbol);
            var stockQuote = await _finnhubService.GetStockPriceQuoteAsync(stockSymbol);

            var stockTrade = new StockTrade();

            if (companyProfile != null && stockQuote != null)
            {
                stockTrade.StockSymbol = stockSymbol;
                stockTrade.StockName = companyProfile.Name;
                stockTrade.Quantity = _tradingOptions.DefaultOrderQuantity ?? 0;
                stockTrade.Price = stockQuote.CurrentPrice.GetValueOrDefault();
            }
            ;

            ViewBag.FinnhubToken = _configuration["FinnhubToken"];

            return View(stockTrade);
        }

        [Route("[action]")]
        public async Task<IActionResult> Orders()
        {
            var orders = new Orders
            {
                BuyOrders = await _stocksService.GetBuyOrdersAsync(),
                SellOrders = await _stocksService.GetSellOrdersAsync(),
            };

            return View(orders);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> BuyOrder(BuyOrderRequest request)
        {
            request.DateAndTimeOfOrder = DateTime.UtcNow;

            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState
                    .Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                StockTrade stockTrade = new StockTrade()
                {
                    StockName = request.StockName,
                    Quantity = request.Quantity,
                    StockSymbol = request.StockSymbol,
                    Price = request.Price,
                };

                return View("Index", stockTrade);
            }

            var buyOrderResponse = await _stocksService.CreateBuyOrderAsync(request);

            return RedirectToAction(nameof(Orders));
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> SellOrder(SellOrderRequest request)
        {
            request.DateAndTimeOfOrder = DateTime.UtcNow;

            if (!ModelState.IsValid)
            {
                ViewBag.Errors = ModelState
                    .Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                StockTrade stockTrade = new StockTrade()
                {
                    StockName = request.StockName,
                    Quantity = request.Quantity,
                    StockSymbol = request.StockSymbol,
                    Price = request.Price,
                };

                return View("Index", stockTrade);
            }

            var sellOrderResponse = await _stocksService.CreateSellOrderAsync(request);

            return RedirectToAction(nameof(Orders));
        }

        [Route("OrdersPDF")]
        public async Task<IActionResult> OrdersPDF()
        {
            var orders = new List<OrderResponse>();

            orders.AddRange(await _stocksService.GetBuyOrdersAsync());
            orders.AddRange(await _stocksService.GetSellOrdersAsync());

            orders = orders.OrderByDescending(o => o.DateAndTimeOfOrder).ToList();

            return new ViewAsPdf("OrdersPDF", orders, ViewData)
            {
                PageMargins = new Margins
                {
                    Right = 20,
                    Bottom = 20,
                    Left = 20,
                    Top = 20,
                },
                PageOrientation = Orientation.Landscape,
            };
        }
    }
}
