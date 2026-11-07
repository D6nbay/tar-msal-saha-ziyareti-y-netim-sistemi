namespace SahaZiyaret.Api.Dtos;

public class DashboardSummaryDto
{
    public int TotalFarmers { get; set; }
    public int TotalVisits { get; set; }
    public int PlannedVisits { get; set; }
    public int CompletedVisits { get; set; }
    public List<FarmVisitDto> UpcomingVisits { get; set; } = new List<FarmVisitDto>();
}