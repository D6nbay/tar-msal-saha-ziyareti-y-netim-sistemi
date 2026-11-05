using Microsoft.EntityFrameworkCore;
using SahaZiyaret.Api.Data;
using SahaZiyaret.Api.Domain.Entities;
using SahaZiyaret.Api.Dtos;
using SahaZiyaret.Api.Domain.Enums;
using SahaZiyaret.Api.Exceptions;


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

    /* İstemciden gelen bilgilerle yeni ziyaret oluşturur. İş kurallarını kontrol eder, çiftçi yoksa ya da aynı gün aynı türde ziyaret varsa BusinessRuleException fırlatır. */
    public async Task<FarmVisitDto> CreateAsync(FarmVisitRequest request)
    {
        VisitRules.Validate(request);
        var farmer = await _context.Farmers.FindAsync(request.FarmerId);
        if (farmer == null)
        {
            throw new BusinessRuleException("Belirtilen çiftçi bulunamadı.");
        }

        await CheckDuplicate(request.FarmerId, request, 0);

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

    /* Ziyaretleri çiftçi, durum ve tarih aralığına göre filtreleyip sayfa sayfa listeler. En yeni ziyaret en üstte olacak şekilde sıralar. */
    public async Task<PagedResult<FarmVisitDto>> GetAllAsync(int? farmerid, VisitStatus? status, DateTime? startTime, DateTime? endTime, int page, int pageSize){
            if(page < 1) page = 1;
            if(pageSize <= 0) pageSize = 10;
            if(pageSize > 100) pageSize = 100;

            var query = _context.FarmVisits.Include(v=> v.Farmer).AsQueryable();

            if(farmerid.HasValue) query = query.Where(v=>v.FarmerId == farmerid.Value);

            if (status.HasValue) query = query.Where(v => v.Status == status.Value);

            if (startTime.HasValue) query = query.Where(v => v.VisitDate >= startTime.Value.Date);    

            if (endTime.HasValue) query = query.Where(v => v.VisitDate < endTime.Value.Date.AddDays(1));

            var totalCount = await query.CountAsync();

            var visits = await query
                .OrderByDescending(v => v.VisitDate)
                .ThenBy(v => v.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<FarmVisitDto>
            {
                Items = visits.Select(ToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
                
        }

    
    
    /* Ziyareti günceller. İş kurallarını kontrol eder: tamamlanan ziyaret tekrar planlanamaz ve aynı gün aynı türde başka ziyaret olamaz. Ziyaret bulunamazsa false döndürür. */
    public async Task<bool> UpdateAsync(int id, FarmVisitRequest request)
    {
        VisitRules.Validate(request);
        var visit = await _context.FarmVisits.FindAsync(id);
        if (visit == null)
        {
        return false;
        }

        if (visit.Status == VisitStatus.Completed && request.Status == VisitStatus.Planned)
        {
            throw new BusinessRuleException("Tamamlanan ziyaret tekrar planlı duruma alınamaz.");
        }

        await CheckDuplicate(visit.FarmerId, request, 0);

        visit.VisitDate = request.VisitDate;
        visit.VisitType = request.VisitType;
        visit.Notes = request.Notes;
        visit.Status = request.Status;
        visit.NextVisitDate = request.NextVisitDate;
        visit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    } 
     
    public async Task<bool> DeleteAsync(int id)
    {
        var visit = await _context.FarmVisits.FindAsync(id);
        if (visit == null)
        {
        return false;
        }
        _context.FarmVisits.Remove(visit);
        await _context.SaveChangesAsync();
        return true;
    }

    /* Aynı çiftçi, aynı gün ve aynı türde başka bir ziyaret varsa hata döndürür. excludeId ile verilen kayıt kontrolün dışında tutulur. */
    private async Task CheckDuplicate(int farmerId, FarmVisitRequest request, int excludeId)
{
    var day = request.VisitDate.Date;
    var nextDay = day.AddDays(1);

    var exists = await _context.FarmVisits.AnyAsync(v =>
        v.FarmerId == farmerId &&
        v.VisitType == request.VisitType &&
        v.VisitDate >= day &&
        v.VisitDate < nextDay &&
        v.Id != excludeId);
        //excludeId kendini hariç tutar, yoksa hata verir

    if (exists)
    {
        throw new BusinessRuleException("Bu çiftçi için aynı günde ve aynı türde bir ziyaret zaten var.");
    }
}

}