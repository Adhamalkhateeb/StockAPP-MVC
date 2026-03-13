using Core.Validators;
using Entities.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;
using ServiceContracts;
using Services;
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
        services.AddScoped<IFinnhubService, FinnhubService>();
        services.AddScoped<IStocksService, StockService>();

        services.AddTransient<CreateOrderActionFilter>();

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
