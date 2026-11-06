using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SahaZiyaret.Api.Data;
using SahaZiyaret.Api.Domain.Entities;
using SahaZiyaret.Api.Dtos;
using SahaZiyaret.Api.Exceptions;

namespace SahaZiyaret.Api.Services;

/* Çiftçilerle ilgili iş mantığını ve veritabanı işlemlerini yürütür*/
public class FarmerService
{
    private readonly SahaZiyaretDbContext _context;
    private readonly IValidator<FarmerRequest> _validator;

    // DbContext ve validator'ı ASP.NET kendisi üretip verir
    public FarmerService(SahaZiyaretDbContext context, IValidator<FarmerRequest> validator)
    {
        _context = context;
        _validator = validator;
    }

    /* Çiftçileri isim, bölge veya telefona göre arayıp sayfa sayfa listeler. Sayfadaki kayıtları, toplam kayıt sayısını, sayfa numarasını ve sayfa boyutunu PagedResult olarak döndürür. */
    public async Task<PagedResult<FarmerDto>> GetAllAsync(string? search, int page, int pageSize)
    {
        // İstemci ysaçma değerler gönderirse değiştir
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _context.Farmers.AsQueryable();
        // Henüz veritabanına gidilmez sadece tüm çiftçiler için sorgu tarifi hazırlanır

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(f => f.FullName.Contains(search)
                                   || f.Region.Contains(search)
                                   || f.PhoneNumber.Contains(search));
            // Arama metni isimde bölgede yada telefonda geçen kayıtlar kalır
        }

        var totalCount = await query.CountAsync();
        // Filtreye uyan toplam kayıt sayısı, sayfa kırpılmadan önce sayılır

        var farmers = await query
            .OrderBy(f => f.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        // Sırala, önceki sayfaların kayıtlarını atla, bir sayfa kadar al ve veritabanından getir

        return new PagedResult<FarmerDto>
        {
            Items = farmers.Select(ToDto).ToList(),
            // Her Farmerı Todtodan geçirip dto listesi yapar
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /* Verilen Idye sahip çiftçiyi veritabanında arar. Bulursa FarmerDto bulamazsa null döndürür. */
    public async Task<FarmerDto?> GetByIdAsync(int id)
    {
        var farmer = await _context.Farmers.FindAsync(id);
        // Birincil anahtara göre ara bulamazsa null döner

        if (farmer == null)
        {
            return null;
        }

        return ToDto(farmer);
    }

    
    public async Task<FarmerDto> CreateAsync(FarmerRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);
        // Biçim hatası varsa ValidationException fırlatır ve metot burada durur

        var farmer = new Farmer
        {
            FullName = request.FullName,
            NationalId = request.NationalId,
            PhoneNumber = request.PhoneNumber,
            Region = request.Region,
            District = request.District,
            CreatedAt = DateTime.UtcNow
            // Kayıt zamanını istemciden almıyoruz, sunucu belirliyor
        };

        _context.Farmers.Add(farmer);
        // Add sadece eklenecek diye işaretler ama henüz yazmaz

        await _context.SaveChangesAsync();
        // Burada veritabanına yazılır ve iD atanır

        return ToDto(farmer);
    }

    /* Verilen Id'deki çiftçinin bilgilerini istemciden gelen değerlerle günceller. Çiftçi bulunamazsa false, güncellendiyse true döndürür. Biçim hatalarında ValidationException fırlatır. */
    public async Task<bool> UpdateAsync(int id, FarmerRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);

        var farmer = await _context.Farmers.FindAsync(id);
        if (farmer == null)
        {
            return false;
        }

        farmer.FullName = request.FullName;
        farmer.NationalId = request.NationalId;
        farmer.PhoneNumber = request.PhoneNumber;
        farmer.Region = request.Region;
        farmer.District = request.District;
        // EF Core nesne üzerindeki değişiklikleri takip eder

        await _context.SaveChangesAsync();
        // Değişen sütunlar için UPDATE komutu burada gönderilir(öncesinde dokunulmaz)

        return true;
    }

    /* Verilen Id'deki çiftçiyi siler. */
    public async Task<bool> DeleteAsync(int id)
    {
        var farmer = await _context.Farmers.FindAsync(id);
        if (farmer == null)
        {
            return false;
        }

        var hasVisits = await _context.FarmVisits.AnyAsync(v => v.FarmerId == id);
        // AnyAsync: bu çiftçiye ait en az bir ziyaret var mı? sadece true/false

        if (hasVisits)
        {
            throw new BusinessRuleException("Ziyaret kaydı bulunan çiftçi silinemez.");
        }

        _context.Farmers.Remove(farmer);
        await _context.SaveChangesAsync();
        return true;
    }

    /* Veritabanı sınıfı olan Farmer'ı, istemciye dönecek FarmerDto'ya çevirir. */
    private static FarmerDto ToDto(Farmer farmer)
    {
        return new FarmerDto
        {
            Id = farmer.Id,
            FullName = farmer.FullName,
            NationalId = farmer.NationalId,
            PhoneNumber = farmer.PhoneNumber,
            Region = farmer.Region,
            District = farmer.District,
            CreatedAt = farmer.CreatedAt
        };
    }
}