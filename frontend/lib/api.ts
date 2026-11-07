import axios from "axios";
import type { PagedResult, ProblemDetails } from "@/types/common";
import type { Farmer, FarmerRequest } from "@/types/farmer";
import type {
  DashboardSummary,
  FarmVisit,
  FarmVisitQuery,
  FarmVisitRequest,
} from "@/types/farmVisit";

const http = axios.create({ baseURL: "/api" });
// Tüm istekler /api ile başlar, next.config.ts'teki proxy bunları backend'e iletir

/* Hatayı ekranda gösterilecek şekle çevirir: genel mesaj ve alan bazlı hatalar. */
export interface ApiError {
  message: string;
  fieldErrors: Record<string, string[]>;
}

export function toApiError(error: unknown): ApiError {
  if (axios.isAxiosError(error)) {
    if (error.response) {
      const data = error.response.data as ProblemDetails | undefined;
      // Backend cevap verdi (400, 404, 500 gibi), cevaptaki detail ya da title alanını kullanıyoruz
      return {
        message: data?.detail ?? data?.title ?? "İşlem sırasında bir hata oluştu.",
        fieldErrors: data?.errors ?? {},
      };
    }
    return {
      message: "Sunucuya ulaşılamıyor. Lütfen bağlantınızı kontrol edin.",
      fieldErrors: {},
    };
    // Cevap hiç gelmediyse (backend kapalı gibi) buraya düşer
  }
  return { message: "Beklenmeyen bir hata oluştu.", fieldErrors: {} };
}

/* Çiftçi isteklerinin parametreleri. */
export interface FarmerQuery {
  search?: string;
  page?: number;
  pageSize?: number;
}

/* Çiftçileri arama ve sayfalama ile getirir. */
export async function getFarmers(query: FarmerQuery = {}): Promise<PagedResult<Farmer>> {
  const response = await http.get<PagedResult<Farmer>>("/farmers", { params: query });
  // params boş (undefined) olanları adrese eklemez
  return response.data;
}

export async function getFarmer(id: number): Promise<Farmer> {
  const response = await http.get<Farmer>(`/farmers/${id}`);
  return response.data;
}

export async function createFarmer(body: FarmerRequest): Promise<Farmer> {
  const response = await http.post<Farmer>("/farmers", body);
  return response.data;
}

export async function updateFarmer(id: number, body: FarmerRequest): Promise<void> {
  await http.put(`/farmers/${id}`, body);
}

export async function deleteFarmer(id: number): Promise<void> {
  await http.delete(`/farmers/${id}`);
}

/* Ziyaretleri filtre ve sayfalama ile getirir. */
export async function getFarmVisits(query: FarmVisitQuery = {}): Promise<PagedResult<FarmVisit>> {
  const response = await http.get<PagedResult<FarmVisit>>("/farm-visits", { params: query });
  return response.data;
}

export async function getFarmVisit(id: number): Promise<FarmVisit> {
  const response = await http.get<FarmVisit>(`/farm-visits/${id}`);
  return response.data;
}

export async function createFarmVisit(body: FarmVisitRequest): Promise<FarmVisit> {
  const response = await http.post<FarmVisit>("/farm-visits", body);
  return response.data;
}

export async function updateFarmVisit(id: number, body: FarmVisitRequest): Promise<void> {
  await http.put(`/farm-visits/${id}`, body);
}

export async function deleteFarmVisit(id: number): Promise<void> {
  await http.delete(`/farm-visits/${id}`);
}

/* Dashboard özetini getirir. */
export async function getDashboardSummary(): Promise<DashboardSummary> {
  const response = await http.get<DashboardSummary>("/dashboard/summary");
  return response.data;
}