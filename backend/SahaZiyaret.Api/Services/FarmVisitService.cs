using Microsoft.EntityFrameworkCore;
using SahaZiyaret.Api.Data;
using SahaZiyaret.Api.Domain.Entities;
using SahaZiyaret.Api.Dtos;

namespace SahaZiyaret.Api.Services;

public class FarmVisitService
{
    private readonly SahaZiyaretDbContext _context;

    public FarmVisitService(SahaZiyaretDbContext context)
    {
        _context = context;
    }

    /* Verilen IDye sahip ziyareti çiftçi bilgisiyle birlikte getirir. Bulunamazsa null döndürür. */
    public async Task<FarmVisitDto?> GetByIdAsync(int id)
    {
        var visit = await _context.FarmVisits
            .Include(v => v.Farmer) 
            .FirstOrDefaultAsync(v => v.Id == id);

        if (visit == null)
        {
            return null;
        }
        return ToDto(visit);
    }

    /* İstemciden gelen bilgilerle yeni ziyaret oluşturur. Çiftçi bulunamazsa null, aksi halde oluşan kaydı FarmVisitDto olarak döndürür. */
    public async Task<FarmVisitDto?> CreateAsync(FarmVisitRequest request)
    {
        var farmer = await _context.Farmers.FindAsync(request.FarmerId);
        if (farmer == null)
        {
            return null;
        }

        var visit = new FarmVisit
        {
            Farmer = farmer,
            VisitDate = request.VisitDate,
            VisitType = request.VisitType,
            Notes = request.Notes,
            Status = request.Status,
            NextVisitDate = request.NextVisitDate,
            CreatedAt = DateTime.UtcNow
        };

        _context.FarmVisits.Add(visit);
        await _context.SaveChangesAsync();
        return ToDto(visit);
    }

    /* FarmVisit sınıfını istemciye dönecek FarmVisitDto’ya çevirir. */
    private static FarmVisitDto ToDto(FarmVisit visit)
    {
        return new FarmVisitDto{
            Id = visit.Id,
            FarmerId = visit.FarmerId,
            FarmerName = visit.Farmer.FullName,
            //Farmer = visit.Farmer, zaten FarmerName gidiyor, gereksiz
            VisitDate = visit.VisitDate,
            VisitType = visit.VisitType,
            Notes = visit.Notes,
            Status = visit.Status,
            NextVisitDate = visit.NextVisitDate,
            CreatedAt = visit.CreatedAt,
            UpdatedAt = visit.UpdatedAt
        };
    }
}