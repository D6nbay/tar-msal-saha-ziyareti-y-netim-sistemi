/* Bu sınıf çiftçilerle ilgili işlemleri ve veritabanı işlemlerini yürütür. */
using Microsoft.EntityFrameworkCore;
using SahaZiyaret.Api.Data;
using SahaZiyaret.Api.Domain.Entities;
using SahaZiyaret.Api.Dtos;

namespace SahaZiyaret.Api.Services;

public class FarmerService
{
    private readonly SahaZiyaretDbContext _context;

    public FarmerService(SahaZiyaretDbContext context)
    {
        _context = context;
    }

    /* Verilen Idye sahip çiftçiyi veritabanında arar. Bulursa Farmerdto, bulamazsa null döndürür. */
    public async Task<FarmerDto?> GetByIdAsync(int id)
    {
        var farmer = await _context.Farmers.FindAsync(id);
        //Findasync primary key'e göre arar
        if(farmer == null){ //Failed
            return null;
        }
        return ToDto(farmer);
    }

    /* İstemciden gelen bilgilerle yeni çiftçi oluşturur, CreatedAt’e gerçek saati atar, kaydeder ve oluşan kaydı Farmerdto olarak döndürür. */
    public async Task<FarmerDto> CreateAsync(FarmerRequest request)
    {
        var farmer = new Farmer{
            //Db id verecek
            FullName = request.FullName,
            NationalId = request.NationalId,
            PhoneNumber = request.PhoneNumber,
            Region = request.Region,
            District = request.District,
            CreatedAt = DateTime.UtcNow//realtime
        };
        _context.Farmers.Add(farmer);//eklemeyi bu kısım yapmaz, sıraya koyar
        await _context.SaveChangesAsync();//burası yapar(efcore ile)
        return ToDto(farmer);
    }

    /* Veritabanı sınıfı olan Farmer’ı, istemciye dönecek Farmerdtoya çevirir. */
    private static FarmerDto ToDto(Farmer farmer)
    {
        // Farmer'ın yedi alanını yeni bir FarmerDto'ya kopyala ve döndür
        //static yaptım çünkü db sınıfı istemciye verilmez
        return new FarmerDto{
            Id = farmer.Id,
            FullName = farmer.FullName,
            NationalId = farmer.NationalId,
            PhoneNumber = farmer.PhoneNumber,
            Region = farmer.Region,
            District = farmer.District,
            CreatedAt = farmer.CreatedAt
        };
    }

    /* Çiftçileri isim, bölge veya telefona göre arayıp sayfa sayfa listeler. Sayfadaki kayıtları, toplam kayıt sayısını, sayfa numarasını ve sayfa boyutunu PagedResult olarak döndürür. */
    public async Task<PagedResult<FarmerDto>> GetAllAsync(string? search, int page, int pageSize)
{
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 10;
    if (pageSize > 100) pageSize = 100;
    //Sayfa boyut sınırları

    var query = _context.Farmers.AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(farmer => farmer.FullName.Contains(search) || farmer.Region.Contains(search) || farmer.PhoneNumber.Contains(search));
    }

    var totalCount = await query.CountAsync(); //Tarife uyan satır sayısı

    var farmers = await query.OrderBy(farmer => farmer.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                                                            //baştan kayıt atla                      //çalıştır ve list döndür
    return new PagedResult<FarmerDto>
    {
        Items = farmers.Select(ToDto).ToList(), //her farmerı ToDto metodundan geçirir
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize
    };
}

    /* Verilen id deki çiftçinin bilgilerini istemciden gelen değerlerle günceller, çiftçi bulunamazsa false güncellendiyse true döndürür. */
    public async Task<bool> UpdateAsync(int id, FarmerRequest request)
    {
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

        await _context.SaveChangesAsync();
        return true;
    }
}