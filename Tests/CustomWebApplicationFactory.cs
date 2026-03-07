using Entities.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<StockMarketDbContext>>();
            services.RemoveAll<StockMarketDbContext>();

            services.AddDbContext<StockMarketDbContext>(options =>
                options.UseInMemoryDatabase("testDb")
            );
        });
    }
}
