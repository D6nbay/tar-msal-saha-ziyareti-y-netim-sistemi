import type { VisitStatus, VisitType } from "@/types/farmVisit";

//Status ve type görünümleri
export const visitStatusLabels: Record<VisitStatus, string> = {
    Planned: "Planlandı",
    Completed: "Tamamlandı",
    Cancelled: "İptal edildi"
};

export const visitTypeLabels: Record<VisitType, string> = {
    Routine: "Rutin",
  Training: "Eğitim",
  Inspection: "Denetim",
  Emergency: "Acil",
}