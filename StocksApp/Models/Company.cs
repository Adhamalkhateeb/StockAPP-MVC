using Core.DTOs;

namespace StocksApp.Models;

public class Company
{
    public string? RequestedStockSymbol { get; set; }
    public CompanyProfileResponse? CompanyProfile { get; set; }
    public decimal? CurrentStockPrice { get; set; }
    public bool IsLiveDataAvailable { get; set; }
    public string? DataUnavailableMessage { get; set; }
}
