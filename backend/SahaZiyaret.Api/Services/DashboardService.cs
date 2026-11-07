using Microsoft.EntityFrameworkCore;
using SahaZiyaret.Api.Data;
using SahaZiyaret.Api.Domain.Enums;
using SahaZiyaret.Api.Dtos;

namespace SahaZiyaret.Api.Services;

/* Dashboard için özet sayıları ve önümüzdeki 7 günün planlı ziyaretlerini hazırlar. */
public class DashboardService
{
    private readonly SahaZiyaretDbContext _context;

    public DashboardService(SahaZiyaretDbContext context)
    {
        _context = context;
    }

    /* Toplam çiftçi ve ziyaret sayısını, planlanan ve tamamlanan ziyaret sayılarını ve bugünden itibaren 7 gün içindeki planlı ziyaretleri döndürür. */
    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var today = DateTime.Today;
        var weekEnd = today.AddDays(7);
        // Aralık: bugünün 00:00'ı dahil 7 gün sonrasının 00:00ı hariç

        var totalFarmers = await _context.Farmers.CountAsync();
        var totalVisits = await _context.FarmVisits.CountAsync();
        // DbContext aynı anda tek sorgu çalıştırabilir, await kullandım

        var plannedVisits = await _context.FarmVisits.CountAsync(v => v.Status == VisitStatus.Planned);
        var completedVisits = await _context.FarmVisits.CountAsync(v => v.Status == VisitStatus.Completed);
        // CountAsync'e şart verince sadece şartı sağlayanları sayar

        var upcoming = await _context.FarmVisits
            .Include(v => v.Farmer)
            .Where(v => v.Status == VisitStatus.Planned && v.VisitDate >= today && v.VisitDate < weekEnd)
            .OrderBy(v => v.VisitDate)
            .ThenBy(v => v.Id)
            .ToListAsync();
        // Planlı ve tarihi aralığa düşen ziyaretler, en yakın tarih en üstte

        return new DashboardSummaryDto
        {
            TotalFarmers = totalFarmers,
            TotalVisits = totalVisits,
            PlannedVisits = plannedVisits,
            CompletedVisits = completedVisits,
            UpcomingVisits = upcoming.Select(FarmVisitService.ToDto).ToList()
        };
    }
}