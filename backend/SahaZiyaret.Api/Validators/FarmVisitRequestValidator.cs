using FluentValidation;
using SahaZiyaret.Api.Dtos;

namespace SahaZiyaret.Api.Validators;

/* FarmVisitRequest için biçim kontrollerini yapar. Tarih ve durum arasındaki iş kuralları VisitRules'ta. */
public class FarmVisitRequestValidator : AbstractValidator<FarmVisitRequest>
{
    public FarmVisitRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.FarmerId)
            .GreaterThan(0).WithMessage("Geçerli bir çiftçi seçilmelidir.");

        RuleFor(x => x.VisitDate)
            .NotEmpty().WithMessage("Ziyaret tarihi zorunludur.");

        RuleFor(x => x.VisitType)
            .IsInEnum().WithMessage("Geçersiz ziyaret türü.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Geçersiz ziyaret durumu.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notlar en fazla 1000 karakter olabilir.");
    }
}