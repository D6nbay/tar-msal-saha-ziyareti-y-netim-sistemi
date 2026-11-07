"use client";
// "use client": bu sayfa tarayıcıda çalışır. useState ve useEffect gibi React özellikleri için gerekli

import { useEffect, useState } from "react";
import { getDashboardSummary, toApiError } from "@/lib/api";
import type { DashboardSummary } from "@/types/farmVisit";

/* Geçici bağlantı testi: dashboard özetini çekip sayıları gösterir. */
export default function HomePage() {
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  // summary: gelen veri. Başta null, yani henüz yok
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getDashboardSummary()
      .then((data) => setSummary(data))
      .catch((err: unknown) => setError(toApiError(err).message));
  }, []);
  // useEffect: sayfa ilk açıldığında bir kez çalışır. [] bunun anlamı

  if (error) {
    return <p className="p-6 text-red-600">{error}</p>;
  }

  if (!summary) {
    return <p className="p-6">Yükleniyor...</p>;
  }

  return (
    <main className="p-6">
      <h1 className="text-2xl font-bold">Bağlantı testi</h1>
      <p>Toplam çiftçi: {summary.totalFarmers}</p>
      <p>Toplam ziyaret: {summary.totalVisits}</p>
      <p>Planlanan: {summary.plannedVisits}</p>
      <p>Tamamlanan: {summary.completedVisits}</p>
      <p>Önümüzdeki 7 gün: {summary.upcomingVisits.length} ziyaret</p>
    </main>
  );
}