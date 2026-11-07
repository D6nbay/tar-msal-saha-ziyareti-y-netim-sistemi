using SahaZiyaret.Api.Data;
using Microsoft.EntityFrameworkCore;
using SahaZiyaret.Api.Services;
using System.Text.Json.Serialization;
using SahaZiyaret.Api.Exceptions;
using FluentValidation;
using SahaZiyaret.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options => 
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }
);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<SahaZiyaretDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<FarmerService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddValidatorsFromAssemblyContaining<FarmerRequestValidator>();
builder.Services.AddScoped<FarmVisitService>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();
// Test ortamında gerçek veritabanına gitmiyoruz, bu yüzden migration ve seed'i atlarız
if (!app.Environment.IsEnvironment("Testing"))
{
    // Uygulama açılırken bekleyen migration'ları uygular ve veritabanı boşsa örnek veri ekler
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<SahaZiyaretDbContext>();
        await db.Database.MigrateAsync();
        await Seed.SeedAsync(db);
    }
}
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Test projesinin uygulamayı başlatabilmesi için Program sınıfını erişilebilir yapar
public partial class Program { }