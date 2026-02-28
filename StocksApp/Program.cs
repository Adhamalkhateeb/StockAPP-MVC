using Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.Interfaces;
using ServiceContracts.Validators;
using Services;
using StocksApp;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllersWithViews();
builder.Services.Configure<TradingOptions>(builder.Configuration.GetSection("TradingOptions"));
builder.Services.AddScoped<IFinnhubService, FinnhubService>();
builder.Services.AddScoped<IStocksService, StockService>();
builder.Services.AddDbContext<StockMarketDbContext>(
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));
// builder.Services.AddValidatorsFromAssemblyContaining<OrderRequestValidator>();
builder.Services.AddHttpClient();





var app = builder.Build();


RotativaConfiguration.Setup("wwwroot", "Rotativa");
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
