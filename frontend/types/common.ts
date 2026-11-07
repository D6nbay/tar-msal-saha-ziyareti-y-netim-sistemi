/* Sayfalı liste cevabı, backenddeki ile aynı fomat */
export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
}

/* Backend'in hata cevabı */
export interface ProblemDetails {
    title?: string;
    status?: number;
    detail?: string;
    errors?: Record<string, string[]>; 
}