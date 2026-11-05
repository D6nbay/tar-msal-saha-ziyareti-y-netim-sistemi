/*Kuralları servis içinde implemente etseydik test etmek için veritabanı gerekirdi. Ayrı ve static bir sınıfta olunca kurallar hiçbir bağlantı olmadan test edilebilir. */
/* Ziyaret isteğine uygulanan, veritabanı gerektirmeyen iş kurallarını kontrol eder. */
using SahaZiyaret.Api.Domain.Enums;
using SahaZiyaret.Api.Dtos;
using SahaZiyaret.Api.Exceptions;

namespace SahaZiyaret.Api.Services;

public static class VisitRules
{
    public static void Validate(FarmVisitRequest request)
    {
        //Geçmiş tarihli ziyaret Planned olmuş mu kontrolü
        if (request.Status == VisitStatus.Planned && request.VisitDate.Date < DateTime.Today)
        {
            throw new BusinessRuleException("Geçmiş bir tarih için planlı ziyaret oluşturulamaz.");
        }

        //Completed ziyarette Notes boş olamaz kontrolü
        if (request.Status == VisitStatus.Completed && string.IsNullOrWhiteSpace(request.Notes))
        {
            throw new BusinessRuleException("Tamamlanan ziyaretin notları boş bırakılamaz.");
        }

        //NextVisitDate, VisitDate'ten önce olamaz kontrolü
        if (request.NextVisitDate.HasValue && request.NextVisitDate.Value.Date < request.VisitDate.Date)
        {
            throw new BusinessRuleException("Sonraki ziyaret tarihi, ziyaret tarihinden önce olamaz.");
        }
    }
}