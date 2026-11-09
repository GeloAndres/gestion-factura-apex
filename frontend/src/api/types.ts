export type InvoiceStatus = 'Issued' | 'Partial' | 'Cancelled'
export type PaymentStatus = 'Pending' | 'Overdue' | 'Paid'
export type ImportIssueSeverity = 'Warning' | 'Inconsistency'

export type IsoDate = string

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
}

export interface InvoiceListItem {
  invoiceNumber: number
  invoiceDate: IsoDate
  paymentDueDate: IsoDate
  customerName: string
  customerRun: string
  totalAmount: number
  creditNotesTotal: number
  pendingBalance: number
  invoiceStatus: InvoiceStatus
  paymentStatus: PaymentStatus
}

export interface InvoiceDetail {
  invoiceNumber: number
  invoiceDate: IsoDate
  daysToDue: number
  paymentDueDate: IsoDate
  totalAmount: number
  creditNotesTotal: number
  pendingBalance: number
  invoiceStatus: InvoiceStatus
  paymentStatus: PaymentStatus
  daysOverdue: number | null
  asOf: IsoDate
  customer: { run: string; name: string; email: string | null }
  payment: { method: string | null; paymentDate: IsoDate } | null
  lines: { productName: string; unitPrice: number; quantity: number; subtotal: number }[]
  creditNotes: { number: number; date: IsoDate; amount: number }[]
  importIssues: { code: string; severity: ImportIssueSeverity; message: string }[]
}

export interface CreditNoteCreated {
  invoiceNumber: number
  number: number
  date: IsoDate
  amount: number
  pendingBalance: number
  invoiceStatus: InvoiceStatus
}

export interface OverdueReport {
  asOf: IsoDate
  minDaysOverdue: number
  count: number
  totalAmount: number
  items: {
    invoiceNumber: number
    invoiceDate: IsoDate
    paymentDueDate: IsoDate
    daysOverdue: number
    customerName: string
    customerRun: string
    totalAmount: number
  }[]
}

export interface PaymentStatusSummary {
  asOf: IsoDate
  totalInvoices: number
  items: { status: PaymentStatus; count: number; percentage: number }[]
}

export interface InconsistentInvoice {
  invoiceNumber: number
  invoiceDate: IsoDate
  customerName: string
  customerRun: string
  declaredTotal: number
  linesTotal: number
  difference: number
  lines: {
    productName: string
    unitPrice: number
    quantity: number
    subtotal: number
    expectedSubtotal: number
    subtotalMatches: boolean
  }[]
}

export interface ImportResult {
  source: string
  totalRecords: number
  imported: number
  importedInconsistent: number
  inconsistentInvoiceNumbers: number[]
  skipped: { position: number; invoiceNumber: number | null; reason: string }[]
  issues: { invoiceNumber: number; code: string; severity: ImportIssueSeverity; message: string }[]
}

export interface LoginResponse {
  accessToken: string
  tokenType: string
  expiresAt: string
  username: string
}

export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  code?: string
  errors?: Record<string, string[]>
}
