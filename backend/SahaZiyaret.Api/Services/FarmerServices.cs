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

    public async Task<FarmerDto?> GetByIdAsync(int id)
    {
        var farmer = await _context.Farmers.FindAsync(id);
        if(farmer == null){ //Failed
            return null;
        }
        return ToDto(farmer);
    }

    public async Task<FarmerDto> CreateAsync(FarmerRequest request)
    {
        var farmer = new Farmer{
            //Db id verecek
            FullName = request.FullName,
            NationalId = request.NationalId,
            PhoneNumber = request.PhoneNumber,
            Region = request.Region,
            District = request.District,
            CreatedAt = DateTime.UtcNow
        };
        _context.Farmers.Add(farmer);//eklemeyi bu kısım yapmaz
        await _context.SaveChangesAsync();//burası yapar
        return ToDto(farmer);
    }

    private static FarmerDto ToDto(Farmer farmer)
    {
        // Farmer'ın yedi alanını yeni bir FarmerDto'ya kopyala ve döndür
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
}