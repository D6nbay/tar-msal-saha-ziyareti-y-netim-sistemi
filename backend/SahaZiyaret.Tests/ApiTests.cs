using System.Net;
using System.Net.Http.Json;
using SahaZiyaret.Api.Dtos;

namespace SahaZiyaret.Tests;

/* Uygulamayı baştan sona (HTTP isteğinden veritabanına kadar) test eder. */
public class ApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    // IClassFixture sayesinde uygulama bir kez başlatılır ve bu sınıftaki testler aynı uygulamayı kullanır
    public ApiTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static object ValidFarmerBody(string name)
    {
        return new
        {
            fullName = name,
            nationalId = "20000000001",
            phoneNumber = "05000000001",
            region = "Konya",
            district = "Çumra"
        };
    }

    /* Çiftçi oluşturulunca 201 dönmeli ve liste/arama endpoint'inde görünmeli. */
    [Fact]
    public async Task CreateFarmer_ThenSearch_ReturnsCreatedFarmer()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/Farmers", ValidFarmerBody("Entegrasyon Çiftçisi"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var listResponse = await _client.GetAsync("/api/Farmers?search=Entegrasyon");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var page = await listResponse.Content.ReadFromJsonAsync<PagedResult<FarmerDto>>();
        Assert.NotNull(page);
        Assert.Contains(page!.Items, f => f.FullName == "Entegrasyon Çiftçisi");
        Assert.Equal(1, page.TotalCount);
    }

    /* Hatalı verilerde 400 dönmeli ve cevap alan bazlı hataları içermeli. */
    [Fact]
    public async Task CreateFarmer_InvalidData_ReturnsBadRequestWithFieldErrors()
    {
        var body = new
        {
            fullName = "",
            nationalId = "123",
            phoneNumber = "abc",
            region = "Konya",
            district = "Çumra"
        };

        var response = await _client.PostAsJsonAsync("/api/Farmers", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        Assert.Contains("fullName", text);
        Assert.Contains("nationalId", text);
        Assert.Contains("phoneNumber", text);
        // Cevapta üç alan için de hata bulunmalı
    }

    /* Kural 1: Geçmiş tarihli Planned ziyaret API'den de 400 ile reddedilmeli. */
    [Fact]
    public async Task CreateVisit_PlannedInThePast_ReturnsBadRequest()
    {
        var farmerResponse = await _client.PostAsJsonAsync("/api/Farmers", ValidFarmerBody("Ziyaret Çiftçisi"));
        var farmer = await farmerResponse.Content.ReadFromJsonAsync<FarmerDto>();
        Assert.NotNull(farmer);

        var visitBody = new
        {
            farmerId = farmer!.Id,
            visitDate = DateTime.Today.AddDays(-2),
            visitType = "Routine",
            status = "Planned"
        };

        var response = await _client.PostAsJsonAsync("/api/farm-visits", visitBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        Assert.Contains("planlı ziyaret", text);
        // Cevabın detail alanında kuralın Türkçe mesajı olmalı
    }
}