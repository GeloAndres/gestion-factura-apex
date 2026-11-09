import { apiRequest } from './client'
import type {
  CreditNoteCreated,
  ImportResult,
  InconsistentInvoice,
  InvoiceDetail,
  InvoiceListItem,
  InvoiceStatus,
  IsoDate,
  LoginResponse,
  OverdueReport,
  PagedResult,
  PaymentStatus,
  PaymentStatusSummary,
} from './types'

export interface InvoiceSearchParams {
  number?: number
  invoiceStatus?: InvoiceStatus
  paymentStatus?: PaymentStatus
  page: number
  pageSize: number
}

/** Días de vencimiento del reporte de vencidas (lo fija el enunciado). */
export const OVERDUE_REPORT_DAYS = 30

export const invoicesApi = {
  login: (username: string, password: string) =>
    apiRequest<LoginResponse>('/api/auth/login', { method: 'POST', body: { username, password }, auth: false }),

  search: (params: InvoiceSearchParams, asOf: IsoDate | null, signal?: AbortSignal) =>
    apiRequest<PagedResult<InvoiceListItem>>('/api/invoices', { query: { ...params, asOf }, signal }),

  getByNumber: (invoiceNumber: number, asOf: IsoDate | null, signal?: AbortSignal) =>
    apiRequest<InvoiceDetail>(`/api/invoices/${invoiceNumber}`, { query: { asOf }, signal }),

  addCreditNote: (invoiceNumber: number, amount: number) =>
    apiRequest<CreditNoteCreated>(`/api/invoices/${invoiceNumber}/credit-notes`, { method: 'POST', body: { amount } }),

  overdueReport: (asOf: IsoDate | null, signal?: AbortSignal) =>
    apiRequest<OverdueReport>('/api/reports/overdue-invoices', {
      query: { minDaysOverdue: OVERDUE_REPORT_DAYS, asOf },
      signal,
    }),

  paymentStatusSummary: (asOf: IsoDate | null, signal?: AbortSignal) =>
    apiRequest<PaymentStatusSummary>('/api/reports/payment-status-summary', { query: { asOf }, signal }),

  inconsistentInvoices: (signal?: AbortSignal) =>
    apiRequest<InconsistentInvoice[]>('/api/reports/inconsistent-invoices', { signal }),

  importFile: (file: File) => {
    const formData = new FormData()
    formData.append('file', file)
    return apiRequest<ImportResult>('/api/imports', { method: 'POST', formData })
  },
}

/** Claves de TanStack Query. Agrupadas para invalidar todo lo que depende de las facturas. */
export const queryKeys = {
  invoices: ['invoices'] as const,
  search: (params: InvoiceSearchParams, asOf: IsoDate | null) => ['invoices', 'search', params, asOf] as const,
  detail: (invoiceNumber: number, asOf: IsoDate | null) => ['invoices', 'detail', invoiceNumber, asOf] as const,
  reports: ['reports'] as const,
  overdue: (asOf: IsoDate | null) => ['reports', 'overdue', asOf] as const,
  summary: (asOf: IsoDate | null) => ['reports', 'summary', asOf] as const,
  inconsistent: ['reports', 'inconsistent'] as const,
}
