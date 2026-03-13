namespace Core.DTOs;

public class StockSnapshotResponse
{
    public string? StockSymbol { get; set; }
    public CompanyProfileResponse? CompanyProfile { get; set; }
    public StockQuoteResponse? StockQuote { get; set; }
    public bool IsLiveDataAvailable { get; set; }
    public bool IsAccessDenied { get; set; }
    public string? UserMessage { get; set; }
}