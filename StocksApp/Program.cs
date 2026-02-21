using FluentValidation;
using ServiceContracts;
using ServiceContracts.Interfaces;
using ServiceContracts.Validators;
using Services;
using StocksApp;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllersWithViews();
builder.Services.Configure<TradingOptions>(builder.Configuration.GetSection("TradingOptions"));
builder.Services.AddSingleton<IFinnhubService, FinnhubService>();
builder.Services.AddSingleton<IStocksService, StockService>();
// builder.Services.AddValidatorsFromAssemblyContaining<OrderRequestValidator>();
builder.Services.AddHttpClient();





var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
