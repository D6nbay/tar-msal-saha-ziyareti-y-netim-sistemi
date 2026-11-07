using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SahaZiyaret.Api.Data;

namespace SahaZiyaret.Tests;

/* Uygulamayı "Testing" ortamında ve sahte (InMemory) veritabanıyla başlatır. */
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = "ApiTestDb-" + Guid.NewGuid();
    // Veritabanı adını fabrika oluşurken bir kez belirliyoruz, tüm istekler aynı sahte veritabanını görsün

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        // Program.cs bu ortamda migration ve seed'i atlıyor

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<SahaZiyaretDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
                // Gerçek SQL Server ayarını kaldırıyoruz
            }

            services.AddDbContext<SahaZiyaretDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
            // Yerine RAM'de çalışan veritabanını koyuyoruz, adı sabit olduğu için istekler arasında veri korunur
        });
    }
}