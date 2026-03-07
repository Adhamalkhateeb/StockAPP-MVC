using System.Text.Json.Serialization;

namespace Core.DTOs;

/// <summary>
/// Represents general company information returned from Finnhub API.
/// Includes company details such as industry, exchange, IPO date,
/// market capitalization, and contact information.
/// </summary>
public class CompanyProfileResponse
{
    /// <summary>
    /// Gets or sets the country where the company headquarters is located.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Gets or sets the currency used in the company's financial filings.
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Gets or sets the stock exchange where the company is listed.
    /// </summary>
    public string? Exchange { get; set; }

    /// <summary>
    /// Gets or sets the initial public offering (IPO) date of the company.
    /// </summary>
    public DateTime? Ipo { get; set; }

    /// <summary>
    /// Gets or sets the total market capitalization of the company.
    /// </summary>
    public decimal MarketCapitalization { get; set; }

    /// <summary>
    /// Gets or sets the official name of the company.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the contact phone number of the company.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the number of outstanding shares in the market.
    /// </summary>
    public decimal ShareOutstanding { get; set; }

    /// <summary>
    /// Gets or sets the stock ticker symbol used on the exchange.
    /// </summary>
    public string? Ticker { get; set; }

    /// <summary>
    /// Gets or sets the official website URL of the company.
    /// </summary>
    public string? Weburl { get; set; }

    /// <summary>
    /// Gets or sets the URL of the company logo.
    /// </summary>
    public string? Logo { get; set; }

    /// <summary>
    /// Gets or sets the industry classification provided by Finnhub.
    /// </summary>
    public string? FinnhubIndustry { get; set; }
}
