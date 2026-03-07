using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using StocksApp.Models;

public class SelectedStockViewComponent : ViewComponent
{
    private readonly IFinnhubService _finnhubService;

    /// <summary>
    /// Constructor for TradeController that executes when a new object is created for the class
    /// </summary>
    /// <param name="finnhubService">Injecting FinnhubService</param>
    public SelectedStockViewComponent(IFinnhubService finnhubService)
    {
        _finnhubService = finnhubService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string? stockSymbol)
    {
        var company = new Company();

        if (!string.IsNullOrEmpty(stockSymbol))
        {
            var companyProfile = await _finnhubService.GetCompanyProfileAsync(stockSymbol);
            var stockPrice = await _finnhubService.GetStockPriceQuoteAsync(stockSymbol);
            if (stockPrice != null && companyProfile != null)
            {
                company.currentStockPrice = stockPrice.CurrentPrice.GetValueOrDefault();
                company.companyProfile = companyProfile;
            }
        }

        if (company != null && company.companyProfile.Logo != null)
            return View(company);
        else
            return Content("");
    }
}
