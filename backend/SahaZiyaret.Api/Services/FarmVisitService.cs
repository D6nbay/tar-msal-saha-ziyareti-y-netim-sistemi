using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SahaZiyaret.Api.Data;
using SahaZiyaret.Api.Domain.Entities;
using SahaZiyaret.Api.Domain.Enums;
using SahaZiyaret.Api.Dtos;
using SahaZiyaret.Api.Exceptions;

namespace SahaZiyaret.Api.Services;

/* Saha ziyaretleriyle ilgili iş mantığını ve veritabanı işlemlerini yürütür. */
public class FarmVisitService
{
    private readonly SahaZiyaretDbContext _context;
    private readonly IValidator<FarmVisitRequest> _validator;

    public FarmVisitService(SahaZiyaretDbContext context, IValidator<FarmVisitRequest> validator)
    {
        _context = context;
        _validator = validator;
    }

    /* Ziyaretleri çiftçi, durum ve tarih aralığına göre filtreleyip sayfa sayfa listeler. En yeni ziyaret en üstte olacak şekilde sıralar. */
    public async Task<PagedResult<FarmVisitDto>> GetAllAsync(
        int? farmerId, VisitStatus? status, DateTime? startDate, DateTime? endDate,
        int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _context.FarmVisits.Include(v => v.Farmer).AsQueryable();
        // Include: ziyaretin yanında çiftçiyi de getirir (FarmerName için gerekli)

        if (farmerId.HasValue)
        {
            query = query.Where(v => v.FarmerId == farmerId.Value);
            // Filtre gönderildiyse sadece o çiftçinin ziyaretleri kalır
        }

        if (status.HasValue)
        {
            query = query.Where(v => v.Status == status.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(v => v.VisitDate >= startDate.Value.Date);
            // Başlangıç günü dahil: o günün 00:00'ından itibaren
        }

        if (endDate.HasValue)
        {
            query = query.Where(v => v.VisitDate < endDate.Value.Date.AddDays(1));
            // Bitiş günü dahil: ertesi günün 00:00'ından önce
        }

        var totalCount = await query.CountAsync();

        var visits = await query
            .OrderByDescending(v => v.VisitDate)
            .ThenBy(v => v.Id)
            // Aynı tarihli ziyaretlerde sıra Id'ye göre sabitlenir, sayfalar arasında tekrar olmaz
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

    /* Verilen Id'ye sahip ziyareti çiftçi bilgisiyle birlikte getirir. Bulunamazsa null döndürür. */
    public async Task<FarmVisitDto?> GetByIdAsync(int id)
    {
        var visit = await _context.FarmVisits
            .Include(v => v.Farmer)
            .FirstOrDefaultAsync(v => v.Id == id);
        // Şarta uyan ilk kaydı getirir, yoksa null

        if (visit == null)
        {
            return null;
        }

        return ToDto(visit);
    }

    /* İstemciden gelen bilgilerle yeni ziyaret oluşturur. Biçim ve iş kurallarını kontrol eder; çiftçi yoksa ya da aynı gün aynı türde ziyaret varsa hata fırlatır. */
    public async Task<FarmVisitDto> CreateAsync(FarmVisitRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);
        // FLUENTVALIDATION

        VisitRules.Validate(request);
        // kural 1, 3, 4

        var farmer = await _context.Farmers.FindAsync(request.FarmerId);
        if (farmer == null)
        {
            throw new BusinessRuleException("Belirtilen çiftçi bulunamadı.");
        }

        await CheckDuplicate(request.FarmerId, request, 0);
        // 3

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

    /* kural 1-4 kontrolü yap, tamamlanan ziyaretin tekrar planlanamaması ve aynı gün aynı türde başka ziyaret olmaması. Çiftçi değiştirilemez. Ziyaret bulunamazsa false. */
    public async Task<bool> UpdateAsync(int id, FarmVisitRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);
        VisitRules.Validate(request);

        var visit = await _context.FarmVisits.FindAsync(id);
        if (visit == null)
        {
            return false;
        }

        if (visit.Status == VisitStatus.Completed && request.Status == VisitStatus.Planned)
        {
            throw new BusinessRuleException("Tamamlanan ziyaret tekrar planlı duruma alınamaz.");
            // Kural 2: eski durum Completed, yeni durum Planned olamaz
        }

        await CheckDuplicate(visit.FarmerId, request, id);

        // Kontroller geçti, şimdi alanları değiştirebiliriz
        visit.VisitDate = request.VisitDate;
        visit.VisitType = request.VisitType;
        visit.Notes = request.Notes;
        visit.Status = request.Status;
        visit.NextVisitDate = request.NextVisitDate;
        visit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    /* Verilen Id'deki ziyareti siler. Ziyaret bulunamazsa false, silindiyse true döndürür. */
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
        //Güncellemede kaydın kendisi sayılmasın diye idsi hariç tutulur(excludeID)

        if (exists)
        {
            throw new BusinessRuleException("Bu çiftçi için aynı gün aynı türde bir ziyaret zaten var.");
        }
    }

    /* FarmVisit sınıfını istemciye dönecek FarmVisitDto'ya çevirir. */
    private static FarmVisitDto ToDto(FarmVisit visit)
    {
        return new FarmVisitDto
        {
            Id = visit.Id,
            FarmerId = visit.FarmerId,
            FarmerName = visit.Farmer.FullName,
            // Farmer'ın dolu olması için sorgularda Include kullan
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