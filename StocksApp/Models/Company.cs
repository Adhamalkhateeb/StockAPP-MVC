using Core.DTOs;

namespace StocksApp.Models;

public class Company
{
    public CompanyProfileResponse companyProfile { get; set; } = new();
    public decimal currentStockPrice { get; set; }
}
