import type { InvoiceStatus, PaymentStatus } from '../api/types'

export const statusColors = {
  good: '#0ca30c',
  warning: '#fab219',
  critical: '#d03b3b',
} as const

interface StatusMeta<T extends string> {
  value: T
  label: string
  plural: string
  description: string
}

export const invoiceStatuses: StatusMeta<InvoiceStatus>[] = [
  { value: 'Issued', label: 'Emitida', plural: 'Emitidas', description: 'Sin notas de crédito' },
  { value: 'Partial', label: 'Parcial', plural: 'Parciales', description: 'Notas de crédito por menos del total' },
  { value: 'Cancelled', label: 'Anulada', plural: 'Anuladas', description: 'Notas de crédito por el total' },
]

export const paymentStatuses: StatusMeta<PaymentStatus>[] = [
  { value: 'Paid', label: 'Pagada', plural: 'Pagadas', description: 'Tienen un pago registrado' },
  { value: 'Pending', label: 'Pendiente', plural: 'Pendientes', description: 'Sin pago y dentro del plazo' },
  { value: 'Overdue', label: 'Vencida', plural: 'Vencidas', description: 'Sin pago y con el vencimiento ya pasado' },
]

export const isInvoiceStatus = (value: string | null): value is InvoiceStatus =>
  invoiceStatuses.some((item) => item.value === value)

export const isPaymentStatus = (value: string | null): value is PaymentStatus =>
  paymentStatuses.some((item) => item.value === value)

export const paymentStatusColor: Record<PaymentStatus, string> = {
  Paid: statusColors.good,
  Pending: statusColors.warning,
  Overdue: statusColors.critical,
}

export const invoiceStatusMeta = (status: InvoiceStatus) =>
  invoiceStatuses.find((item) => item.value === status) ?? invoiceStatuses[0]

export const paymentStatusMeta = (status: PaymentStatus) =>
  paymentStatuses.find((item) => item.value === status) ?? paymentStatuses[0]
