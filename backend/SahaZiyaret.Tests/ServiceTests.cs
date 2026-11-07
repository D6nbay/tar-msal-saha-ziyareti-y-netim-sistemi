using Microsoft.EntityFrameworkCore;
using SahaZiyaret.Api.Data;
using SahaZiyaret.Api.Domain.Entities;
using SahaZiyaret.Api.Domain.Enums;
using SahaZiyaret.Api.Dtos;
using SahaZiyaret.Api.Exceptions;
using SahaZiyaret.Api.Services;
using SahaZiyaret.Api.Validators;

namespace SahaZiyaret.Tests;

public class ServiceTests
{
    private static SahaZiyaretDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SahaZiyaretDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SahaZiyaretDbContext(options);
    }

    private static Farmer NewFarmer(string name)
    {
        return new Farmer
        {
            FullName = name,
            NationalId = "20000000001",
            PhoneNumber = "05000000001",
            Region = "Konya",
            District = "Çumra",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCorrectPage()
    {
        using var context = CreateContext();
        for (var i = 1; i <= 25; i++)
        {
            context.Farmers.Add(NewFarmer($"Çiftçi {i}"));
        }
        await context.SaveChangesAsync();

        var service = new FarmerService(context, new FarmerRequestValidator());

        var result = await service.GetAllAsync(null, 2, 10);

        Assert.Equal(10, result.Items.Count);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal("Çiftçi 11", result.Items[0].FullName);
        Assert.Equal("Çiftçi 20", result.Items[9].FullName);
    }

    [Fact]
    public async Task DeleteAsync_FarmerWithVisits_ThrowsBusinessRuleException()
    {
        using var context = CreateContext();
        var farmer = NewFarmer("Ziyaretli Çiftçi");
        context.Farmers.Add(farmer);
        context.FarmVisits.Add(new FarmVisit
        {
            Farmer = farmer,
            VisitDate = DateTime.Today,
            VisitType = VisitType.Routine,
            Status = VisitStatus.Cancelled,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new FarmerService(context, new FarmerRequestValidator());

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.DeleteAsync(farmer.Id));
    }

    [Fact]
    public async Task CreateAsync_SameFarmerDayAndType_ThrowsBusinessRuleException()
    {
        using var context = CreateContext();
        var farmer = NewFarmer("Çakışma Çiftçisi");
        context.Farmers.Add(farmer);
        await context.SaveChangesAsync();

        var service = new FarmVisitService(context, new FarmVisitRequestValidator());
        var day = DateTime.Today.AddDays(3);

        var first = new FarmVisitRequest
        {
            FarmerId = farmer.Id,
            VisitDate = day.AddHours(9),
            VisitType = VisitType.Routine,
            Status = VisitStatus.Planned
        };
        await service.CreateAsync(first);
        // İlk ziyaret sorunsuz olsun

        var second = new FarmVisitRequest
        {
            FarmerId = farmer.Id,
            VisitDate = day.AddHours(14),
            VisitType = VisitType.Routine,
            Status = VisitStatus.Planned
        };

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.CreateAsync(second));
    }
}