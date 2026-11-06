using FluentValidation;
using SahaZiyaret.Api.Dtos;

namespace SahaZiyaret.Api.Validators;

/* FarmerRequest için biçim kontrollerini (boşluk, uzunluk, telefon ve kimlik formatı) yapar. */
public class FarmerRequestValidator : AbstractValidator<FarmerRequest>
{
    public FarmerRequestValidator()
    {
        // Bir kural zincirinde ilk hata bulununca diğerlerine bakma (aynı alan için tek mesaj görünsün)
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad soyad boş bırakılamaz.")
            .MaximumLength(100).WithMessage("Ad soyad en fazla 100 karakter olabilir.");

        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("Kimlik numarası boş bırakılamaz.")
            .Matches(@"^[1-9][0-9]{10}$")
            .WithMessage("Kimlik numarası 11 haneli olmalı, sadece rakamlardan oluşmalı ve 0 ile başlamamalıdır.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon numarası boş bırakılamaz.")
            .Matches(@"^(\+90|0)?5[0-9]{9}$")
            .WithMessage("Geçerli bir cep telefonu girin (örn. 05321234567, 5321234567 veya +905321234567).");

        RuleFor(x => x.Region)
            .NotEmpty().WithMessage("Bölge boş bırakılamaz.")
            .MaximumLength(100).WithMessage("Bölge en fazla 100 karakter olabilir.");

        RuleFor(x => x.District)
            .NotEmpty().WithMessage("İlçe boş bırakılamaz.")
            .MaximumLength(100).WithMessage("İlçe en fazla 100 karakter olabilir.");
    }
}