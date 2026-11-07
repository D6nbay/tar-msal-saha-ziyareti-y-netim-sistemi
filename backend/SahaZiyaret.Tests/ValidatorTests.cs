using SahaZiyaret.Api.Dtos;
using SahaZiyaret.Api.Validators;

namespace SahaZiyaret.Tests;

/* FarmerRequestValidator'ın biçim kontrollerini test eder. */
public class ValidatorTests
{
    private static FarmerRequest ValidFarmer()
    {
        return new FarmerRequest
        {
            FullName = "Test Çiftçi",
            NationalId = "20000000001",
            PhoneNumber = "05000000001",
            Region = "Konya",
            District = "Çumra"
        };
    }

    /* Geçerli çiftçi bilgileri doğrulamadan geçmeli. */
    [Fact]
    public void FarmerRequest_Valid_PassesValidation()
    {
        var result = new FarmerRequestValidator().Validate(ValidFarmer());

        Assert.True(result.IsValid);
    }

    /* Hatalı telefon ve kimlik numarası için ilgili alanlarda hata üretmeli. */
    [Fact]
    public void FarmerRequest_InvalidPhoneAndNationalId_ReturnsFieldErrors()
    {
        var request = ValidFarmer();
        request.PhoneNumber = "abc";
        request.NationalId = "123";

        var result = new FarmerRequestValidator().Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(FarmerRequest.PhoneNumber));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(FarmerRequest.NationalId));
        // Hata listesinde bu iki alana ait en az birer hata olmalı
    }
}