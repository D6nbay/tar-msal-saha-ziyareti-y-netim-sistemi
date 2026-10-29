using SahaZiyaret.Api.Domain.Enums;

namespace SahaZiyaret.Api.Domain.Entities;

public class FarmVisit
{
    public int Id { get; set; }
    public int FarmerId { get; set; }
    public Farmer Farmer { get; set; } = null!;
    public DateTime VisitDate { get; set; }
    public VisitType VisitType { get; set; }
    public string? Notes { get; set; }
    public VisitStatus Status { get; set; }
    public DateTime? NextVisitDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}