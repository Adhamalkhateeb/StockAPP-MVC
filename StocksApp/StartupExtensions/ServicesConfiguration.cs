using Core.Validators;
using Entities.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.FinnhubServices;
using ServiceContracts.StocksServices;
using Services;
using Services.StocksService;
using StockMarketSolution.Middleware;
using StocksApp;

public static class ServicesConfiguration
{
    public static IServiceCollection AddServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddControllersWithViews();

        services.Configure<TradingOptions>(configuration.GetSection("TradingOptions"));

        services.AddScoped<IFinnhubRepository, FinnhubRepository>();
        services.AddScoped<IStocksRepository, StocksRepository>();
        services.AddScoped<IFinnhubCompanyProfileService, FinnhubCompanyProfileService>();
        services.AddScoped<IFinnhubSearchStocksService, FinnhubSearchStocksService>();
        services.AddScoped<IFinnhubStockPriceQuoteService, FinnhubStockPriceQuoteService>();
        services.AddScoped<IFinnhubStocksService, FinnhubStocksService>();
        services.AddScoped<IBuyOrderService, StocksBuyOrderService>();
        services.AddScoped<ISellOrderService, StocksSellOrderService>();

        services.AddTransient<CreateOrderActionFilter>();
        services.AddTransient<ExceptionHandlingMiddleware>();

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<OrderRequestValidator>();

        services.AddHttpClient();
        services.AddMemoryCache();

        return services;
    }

    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<StockMarketDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
        );

        return services;
    }
}
