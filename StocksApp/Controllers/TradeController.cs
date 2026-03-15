using Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;
using ServiceContracts;
using ServiceContracts.FinnhubServices;
using ServiceContracts.StocksServices;
using StocksApp.Models;

namespace StocksApp.Controllers
{
    [Route("[controller]")]
    public class TradeController : Controller
    {
        private readonly TradingOptions _tradingOptions;
        private readonly ILogger<TradeController> _logger;
        private readonly IFinnhubStocksService _stocksService;
        private readonly IBuyOrderService _buyOrderService;
        private readonly ISellOrderService _sellOrderService;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor for TradeController that executes when a new object is created for the class
        /// </summary>
        /// <param name="tradingOptions">Injecting TradeOptions config through Options pattern</param>
        /// <param name="stocksService">Injecting StocksService</param>
        /// <param name="finnhubService">Injecting FinnhubService</param>
        /// <param name="configuration">Injecting IConfiguration</param>
        public TradeController(
            ILogger<TradeController> logger,
            IOptions<TradingOptions> tradingOptions,
            IFinnhubStocksService stocksService,
            IBuyOrderService buyOrderService,
            ISellOrderService sellOrderService,
            IConfiguration configuration
        )
        {
            _logger = logger;
            _tradingOptions = tradingOptions.Value;
            _stocksService = stocksService;
            _buyOrderService = buyOrderService;
            _sellOrderService = sellOrderService;
            _configuration = configuration;
        }

        [Route("[action]/{stockSymbol?}")]
        [Route("~/[controller]/{stockSymbol?}")]
        public async Task<IActionResult> Index(string? stockSymbol)
        {
            _logger.LogInformation("Trade index requested for symbol {StockSymbol}", stockSymbol);

            if (string.IsNullOrEmpty(stockSymbol))
            {
                stockSymbol = "MSFT";
                _logger.LogInformation(
                    "No stock symbol supplied. Falling back to default symbol {StockSymbol}",
                    stockSymbol
                );
            }

            var snapshot = await _stocksService.GetStockSnapshotAsync(stockSymbol);

            var stockTrade = new StockTrade
            {
                StockSymbol = stockSymbol,
                StockName = snapshot.CompanyProfile?.Name ?? stockSymbol,
                Quantity = _tradingOptions.DefaultOrderQuantity ?? 0,
                Price = snapshot.StockQuote?.CurrentPrice ?? 0,
                CanTrade = snapshot.IsLiveDataAvailable,
                DataUnavailableMessage = snapshot.UserMessage,
            };

            if (
                snapshot.IsLiveDataAvailable
                && snapshot.CompanyProfile != null
                && snapshot.StockQuote != null
            )
            {
                _logger.LogInformation(
                    "Loaded trade model for symbol {StockSymbol} with company {StockName} and price {Price}",
                    stockTrade.StockSymbol,
                    stockTrade.StockName,
                    stockTrade.Price
                );
            }
            else
            {
                _logger.LogWarning(
                    "Could not fully load trade data for symbol {StockSymbol}. CompanyProfileFound={CompanyProfileFound}, StockQuoteFound={StockQuoteFound}, AccessDenied={AccessDenied}",
                    stockSymbol,
                    snapshot.CompanyProfile != null,
                    snapshot.StockQuote != null,
                    snapshot.IsAccessDenied
                );
            }

            ViewBag.FinnhubToken = _configuration["FinnhubToken"];

            return View(stockTrade);
        }

        [Route("[action]")]
        public async Task<IActionResult> Orders()
        {
            _logger.LogInformation("Orders page requested");

            var orders = new Orders
            {
                BuyOrders = await _buyOrderService.GetBuyOrdersAsync(),
                SellOrders = await _sellOrderService.GetSellOrdersAsync(),
            };

            _logger.LogInformation(
                "Orders page loaded with {BuyOrdersCount} buy orders and {SellOrdersCount} sell orders",
                orders.BuyOrders.Count,
                orders.SellOrders.Count
            );

            return View(orders);
        }

        [HttpPost]
        [Route("[action]")]
        [CreateOrderActionFilterFactory]
        public async Task<IActionResult> BuyOrder(BuyOrderRequest request)
        {
            _logger.LogInformation(
                "Buy order request received for symbol {StockSymbol}, quantity {Quantity}, price {Price}",
                request.StockSymbol,
                request.Quantity,
                request.Price
            );

            request.DateAndTimeOfOrder = DateTime.UtcNow;

            var buyOrderResponse = await _buyOrderService.CreateBuyOrderAsync(request);

            _logger.LogInformation(
                "Buy order created successfully. BuyOrderId={BuyOrderId}, Symbol={StockSymbol}, Quantity={Quantity}",
                buyOrderResponse.BuyOrderID,
                buyOrderResponse.StockSymbol,
                buyOrderResponse.Quantity
            );

            return RedirectToAction(nameof(Orders));
        }

        [HttpPost]
        [Route("[action]")]
        [CreateOrderActionFilterFactory]
        public async Task<IActionResult> SellOrder(SellOrderRequest request)
        {
            _logger.LogInformation(
                "Sell order request received for symbol {StockSymbol}, quantity {Quantity}, price {Price}",
                request.StockSymbol,
                request.Quantity,
                request.Price
            );

            request.DateAndTimeOfOrder = DateTime.UtcNow;

            var sellOrderResponse = await _sellOrderService.CreateSellOrderAsync(request);

            _logger.LogInformation(
                "Sell order created successfully. SellOrderId={SellOrderId}, Symbol={StockSymbol}, Quantity={Quantity}",
                sellOrderResponse.SellOrderID,
                sellOrderResponse.StockSymbol,
                sellOrderResponse.Quantity
            );

            return RedirectToAction(nameof(Orders));
        }

        [Route("OrdersPDF")]
        public async Task<IActionResult> OrdersPDF()
        {
            _logger.LogInformation("Orders PDF generation requested");

            var orders = new List<OrderResponse>();

            orders.AddRange(await _buyOrderService.GetBuyOrdersAsync());
            orders.AddRange(await _sellOrderService.GetSellOrdersAsync());

            orders = orders.OrderByDescending(o => o.DateAndTimeOfOrder).ToList();

            _logger.LogInformation(
                "Orders PDF prepared with {OrdersCount} total orders",
                orders.Count
            );

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
