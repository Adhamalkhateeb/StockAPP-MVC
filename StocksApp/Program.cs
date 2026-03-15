using Rotativa.AspNetCore;
using Serilog;
using StockMarketSolution.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, services, loggingConfig) =>
    {
        loggingConfig.ReadFrom.Configuration(builder.Configuration).ReadFrom.Services(services);
    }
);

builder.Services.AddServices(builder.Configuration);

if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddDatabase(builder.Configuration);
    RotativaConfiguration.Setup("wwwroot", "Rotativa");
}

builder.Services.AddHttpClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseExceptionHandlingMiddleware();
}

app.UseSerilogRequestLogging();
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.MapGet(
    "/api/history/{symbol}",
    async (string symbol, HttpClient http) =>
    {
        var url =
            $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?interval=1m&range=1d";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", "Mozilla/5.0");

        var response = await http.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        return Results.Content(json, "application/json");
    }
);

app.Run();

public partial class Program { }
