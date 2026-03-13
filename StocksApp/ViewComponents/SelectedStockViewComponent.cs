using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using StocksApp.Models;

public class SelectedStockViewComponent : ViewComponent
{
    private readonly IFinnhubService _finnhubService;

    public SelectedStockViewComponent(IFinnhubService finnhubService)
    {
        _finnhubService = finnhubService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string? stockSymbol)
    {
        if (string.IsNullOrEmpty(stockSymbol))
            return Content(string.Empty);

        var company = new Company { RequestedStockSymbol = stockSymbol };

        try
        {
            var snapshot = await _finnhubService.GetStockSnapshotAsync(stockSymbol);

            company.IsLiveDataAvailable = snapshot.IsLiveDataAvailable;
            company.CompanyProfile = snapshot.CompanyProfile;
            company.CurrentStockPrice = snapshot.StockQuote?.CurrentPrice;
            company.DataUnavailableMessage = snapshot.UserMessage;
        }
        catch (Exception ex)
        {
            _ = ex;
            company.IsLiveDataAvailable = false;
            company.DataUnavailableMessage =
                "Unable to load stock data right now. Please refresh or try another stock.";
        }

        return View(company);
    }
}
