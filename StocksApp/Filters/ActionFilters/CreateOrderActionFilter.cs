using Core.DTOs;
using Microsoft.AspNetCore.Mvc.Filters;
using StocksApp.Controllers;
using StocksApp.Models;

public class CreateOrderActionFilterFactory : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<CreateOrderActionFilter>();
    }
}

public class CreateOrderActionFilter(ILogger<CreateOrderActionFilter> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next
    )
    {
        if (context.Controller is TradeController controller)
        {
            if (!controller.ModelState.IsValid)
            {
                var errors = controller
                    .ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                controller.ViewData["Errors"] = errors;

                var request = context
                    .ActionArguments.Values.OfType<OrderRequest>()
                    .FirstOrDefault();

                if (request != null)
                {
                    logger.LogWarning(
                        "Order validation failed. Type={OrderType} Symbol={StockSymbol} Errors={ErrorCount}",
                        request.GetType().Name,
                        request.StockSymbol,
                        controller.ModelState.ErrorCount
                    );

                    var stockTrade = new StockTrade
                    {
                        StockName = request.StockName,
                        Quantity = request.Quantity,
                        StockSymbol = request.StockSymbol,
                        Price = request.Price,
                        CanTrade = true,
                    };

                    context.Result = controller.View(nameof(TradeController.Index), stockTrade);
                    return;
                }
            }
        }

        await next();
    }
}
