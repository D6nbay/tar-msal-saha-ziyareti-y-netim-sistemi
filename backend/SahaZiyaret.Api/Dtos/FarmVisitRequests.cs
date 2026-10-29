using SahaZiyaret.Api.Domain.Enums;

namespace SahaZiyaret.Api.Dtos;

public class FarmVisitRequest
{
    public int FarmerId { get; set; }
    public DateTime VisitDate { get; set; }
    public VisitType VisitType { get; set; }
    public string? Notes { get; set; }
    public VisitStatus Status { get; set; }
    public DateTime? NextVisitDate { get; set; }
}