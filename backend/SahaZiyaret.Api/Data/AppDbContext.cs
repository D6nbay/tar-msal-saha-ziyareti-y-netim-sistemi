using Microsoft.EntityFrameworkCore;
using SahaZiyaret.Api.Domain.Entities;

namespace SahaZiyaret.Api.Data;

public class SahaZiyaretDbContext : DbContext
{
    public SahaZiyaretDbContext(DbContextOptions<SahaZiyaretDbContext> options) : base(options)
    {
    }

    public DbSet<Farmer> Farmers { get; set; } = null!;

    public DbSet<FarmVisit> FarmVisits { get; set; } = null!;
}