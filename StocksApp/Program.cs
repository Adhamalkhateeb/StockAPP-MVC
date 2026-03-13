using Rotativa.AspNetCore;
using Serilog;

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

app.UseStaticFiles();

if (!app.Environment.IsEnvironment("Test"))
{
    app.UseDeveloperExceptionPage();
    app.UseSerilogRequestLogging();
}

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
