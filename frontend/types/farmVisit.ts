export type VisitStatus = "Planned" | "Completed" | "Cancelled";
export type VisitType = "Routine" | "Training" | "Inspection" | "Emergency";

/* API'nin döndürdüğü ziyaret. */
export interface FarmVisit {
  id: number;
  farmerId: number;
  farmerName: string;
  visitDate: string;
  visitType: VisitType;
  notes: string | null;
  status: VisitStatus;
  nextVisitDate: string | null;
  createdAt: string;
  updatedAt: string | null;
  // "string | null": değer ya metin ya da boş olabilir
}

/* Ziyaret eklerken ve güncellerken API'ye gönderilen bilgiler. */
export interface FarmVisitRequest {
  farmerId: number;
  visitDate: string;
  visitType: VisitType;
  notes: string | null;
  status: VisitStatus;
  nextVisitDate: string | null;
}

// Ziyaret listesi filtreleri.
export interface FarmVisitQuery {
  page?: number;
  pageSize?: number;
  farmerId?: number;
  status?: VisitStatus;
  startDate?: string;
  endDate?: string;
}

/* Dashboard özeti. */
export interface DashboardSummary {
  totalFarmers: number;
  totalVisits: number;
  plannedVisits: number;
  completedVisits: number;
  upcomingVisits: FarmVisit[];
}