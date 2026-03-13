namespace Core.Exceptions;

public class FinnhubAccessDeniedException(string apiUrl)
    : InvalidOperationException("Finnhub rejected the request for live market data.")
{
    public string ApiUrl { get; } = apiUrl;
}