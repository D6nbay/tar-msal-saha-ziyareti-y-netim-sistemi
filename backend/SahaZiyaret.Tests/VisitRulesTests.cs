using SahaZiyaret.Api.Domain.Enums;
using SahaZiyaret.Api.Dtos;
using SahaZiyaret.Api.Exceptions;
using SahaZiyaret.Api.Services;

namespace SahaZiyaret.Tests;

/* VisitRules sınıfındaki veritabanı gerektirmeyen iş kurallarını test eder. */
public class VisitRulesTests
{
    private static FarmVisitRequest ValidRequest()
    {
        return new FarmVisitRequest
        {
            FarmerId = 1,
            VisitDate = DateTime.Today.AddDays(2),
            VisitType = VisitType.Routine,
            Status = VisitStatus.Planned,
            Notes = null,
            NextVisitDate = null
        };
    }

    [Fact]
    public void Validate_PlannedVisitInThePast_Throws()
    {
        var request = ValidRequest();
        request.VisitDate = DateTime.Today.AddDays(-1);
        request.Status = VisitStatus.Planned;

        Assert.Throws<BusinessRuleException>(() => VisitRules.Validate(request));
    }

    [Fact]
    public void Validate_CompletedVisitWithoutNotes_Throws()
    {
        var request = ValidRequest();
        request.VisitDate = DateTime.Today.AddDays(-1);
        request.Status = VisitStatus.Completed;
        request.Notes = "   ";

        Assert.Throws<BusinessRuleException>(() => VisitRules.Validate(request));
    }

    [Fact]
    public void Validate_NextVisitBeforeVisitDate_Throws()
    {
        var request = ValidRequest();
        request.NextVisitDate = request.VisitDate.AddDays(-1);

        Assert.Throws<BusinessRuleException>(() => VisitRules.Validate(request));
    }

    [Fact]
    public void Validate_ValidRequests_DoNotThrow()
    {
        var planned = ValidRequest();
        planned.VisitDate = DateTime.Today;

        var completed = ValidRequest();
        completed.VisitDate = DateTime.Today.AddDays(-3);
        completed.Status = VisitStatus.Completed;
        completed.Notes = "Ziyaret tamamlandı.";
        completed.NextVisitDate = DateTime.Today.AddDays(10);

        Assert.Null(Record.Exception(() => VisitRules.Validate(planned)));
        Assert.Null(Record.Exception(() => VisitRules.Validate(completed)));
        // Record.Exception hata yakalarsa onu döndürür, hata yoksa null döner
    }
}