using Core.Validators;
using Entities.Data;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;
using Rotativa.AspNetCore;
using ServiceContracts;
using Services;
using StocksApp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.Configure<TradingOptions>(builder.Configuration.GetSection("TradingOptions"));
builder.Services.AddScoped<IFinnhubRepository, FinnhubRepository>();
builder.Services.AddScoped<IStocksRepository, StocksRepository>();
builder.Services.AddScoped<IFinnhubService, FinnhubService>();
builder.Services.AddScoped<IStocksService, StockService>();

if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddDbContext<StockMarketDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    );
}

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<OrderRequestValidator>();

builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

var app = builder.Build();

RotativaConfiguration.Setup("/usr/bin", "");

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();

public partial class Program { }
