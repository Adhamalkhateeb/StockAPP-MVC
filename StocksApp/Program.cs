using FluentValidation;
using ServiceContracts;
using ServiceContracts.Interfaces;
using ServiceContracts.Validators;
using Services;
using StocksApp;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IFinnhubService, FinnhubService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddValidatorsFromAssemblyContaining<OrderRequestValidator>();


builder.Services.Configure<TradingOptions>(builder.Configuration.GetSection("TradingOptions"));


var app = builder.Build();

app.UseStaticFiles();
app.MapControllers();

app.Run();
