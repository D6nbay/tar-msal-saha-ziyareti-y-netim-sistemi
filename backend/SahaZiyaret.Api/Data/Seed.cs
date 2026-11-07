using Microsoft.EntityFrameworkCore;
using SahaZiyaret.Api.Domain.Entities;
using SahaZiyaret.Api.Domain.Enums;

namespace SahaZiyaret.Api.Data;

/* Veritabanı boşsa uygulamayı incelemek için örnek çiftçi ve ziyaret verisi ekler. Tüm bilgiler uydurmadır. */
public static class Seed
{
    public static async Task SeedAsync(SahaZiyaretDbContext context)
    {
        if (await context.Farmers.AnyAsync())
        {
            return;
            // Çiftçi tablosu doluysa dokunmayız, böylece her açılışta veri tekrar eklenmez
        }

        var now = DateTime.UtcNow;
        var today = DateTime.Today;
        // Ziyaret tarihlerini bugüne göre hesaplıyoruz, dashboard'un 7 günlük listesi hep dolu görünsün

        var farmers = new List<Farmer>
        {
            new Farmer { FullName = "Ahmet Soyupek", NationalId = "20000000001", PhoneNumber = "05000000001", Region = "Konya", District = "Çumra", CreatedAt = now },
            new Farmer { FullName = "Ayşe Selçuk", NationalId = "20000000002", PhoneNumber = "05000000002", Region = "Ankara", District = "Polatlı", CreatedAt = now },
            new Farmer { FullName = "Mehmet Yıldıırm", NationalId = "20000000003", PhoneNumber = "05000000003", Region = "Adana", District = "Ceyhan", CreatedAt = now },
            new Farmer { FullName = "Melih Çelik", NationalId = "20000000004", PhoneNumber = "05000000004", Region = "İzmir", District = "Ödemiş", CreatedAt = now },
            new Farmer { FullName = "Sami Altınok", NationalId = "20000000005", PhoneNumber = "05000000005", Region = "Bursa", District = "Karacabey", CreatedAt = now },
            new Farmer { FullName = "Zeynep Arslan", NationalId = "20000000006", PhoneNumber = "05000000006", Region = "Manisa", District = "Turgutlu", CreatedAt = now }
        };

        context.Farmers.AddRange(farmers);
        await context.SaveChangesAsync();

        // Ziyaret üretmeyi kısaltan helper: tarihler bugüne göre gün farkıyla verilir
        FarmVisit Visit(Farmer farmer, int dayOffset, VisitType type, VisitStatus status, string? notes, int? nextOffset)
        {
            return new FarmVisit
            {Farmer = farmer, VisitDate = today.AddDays(dayOffset).AddHours(9),
                // Her ziyaret sabah 09:00'da
                VisitType = type,
                Status = status,
                Notes = notes,
                NextVisitDate = nextOffset.HasValue ? today.AddDays(nextOffset.Value) : null,
                CreatedAt = now
            };
        }

        var visits = new List<FarmVisit>
        {
            Visit(farmers[0], -20, VisitType.Routine, VisitStatus.Completed, "Toprak analizi için numune alındı.", 5),
            Visit(farmers[0], 5, VisitType.Training, VisitStatus.Planned, null, null),

            Visit(farmers[1], -14, VisitType.Inspection, VisitStatus.Completed, "Sera ve sulama sistemi kontrol edildi.", 2),
            Visit(farmers[1], 0, VisitType.Training, VisitStatus.Planned, "Bugünkü eğitim oturumu planlandı.", null),
            Visit(farmers[1], 2, VisitType.Routine, VisitStatus.Planned, null, null),

            Visit(farmers[2], -10, VisitType.Emergency, VisitStatus.Completed, "Dolu zararı tespit edildi, rapor hazırlandı.", 3),
            Visit(farmers[2], 3, VisitType.Inspection, VisitStatus.Planned, null, null),

            Visit(farmers[3], -7, VisitType.Routine, VisitStatus.Cancelled, "Hava koşulları nedeniyle iptal edildi.", null),
            Visit(farmers[3], 1, VisitType.Routine, VisitStatus.Planned, "İptal edilen ziyaret yeniden planlandı.", null),

            Visit(farmers[4], -3, VisitType.Training, VisitStatus.Completed, "Budama eğitimi verildi.", 10),
            Visit(farmers[4], 10, VisitType.Routine, VisitStatus.Planned, null, null),
            // Bu ziyaret 7 günlük pencerenin dışında, dashboard listesinde görünmemeli

            Visit(farmers[5], -1, VisitType.Inspection, VisitStatus.Completed, "Zararlı kontrolü yapıldı.", 6),
            Visit(farmers[5], 6, VisitType.Routine, VisitStatus.Planned, null, null)
        };

        context.FarmVisits.AddRange(visits);
        await context.SaveChangesAsync();
    }
}