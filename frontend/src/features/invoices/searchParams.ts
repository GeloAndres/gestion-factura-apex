import type { InvoiceSearchParams } from '../../api/invoicesApi'
import { isInvoiceStatus, isPaymentStatus } from '../../lib/status'

// Los filtros viven en la URL (?numero=30&estadoFactura=Partial...) para que se puedan
// compartir y para que el botón "atrás" del navegador funcione.

export const PAGE_SIZES = [10, 20, 50] as const
const DEFAULT_PAGE_SIZE = 10

export type InvoiceFilters = Pick<InvoiceSearchParams, 'number' | 'invoiceStatus' | 'paymentStatus'>

export function readSearchParams(params: URLSearchParams): InvoiceSearchParams {
  const number = Number(params.get('numero'))
  const invoiceStatus = params.get('estadoFactura')
  const paymentStatus = params.get('estadoPago')
  const page = Number(params.get('pagina'))
  const pageSize = Number(params.get('porPagina'))

  return {
    number: Number.isInteger(number) && number > 0 ? number : undefined,
    invoiceStatus: isInvoiceStatus(invoiceStatus) ? invoiceStatus : undefined,
    paymentStatus: isPaymentStatus(paymentStatus) ? paymentStatus : undefined,
    page: Number.isInteger(page) && page > 0 ? page : 1,
    pageSize: (PAGE_SIZES as readonly number[]).includes(pageSize) ? pageSize : DEFAULT_PAGE_SIZE,
  }
}

export function toSearchParams(search: InvoiceSearchParams): URLSearchParams {
  const params = new URLSearchParams()
  if (search.number) params.set('numero', String(search.number))
  if (search.invoiceStatus) params.set('estadoFactura', search.invoiceStatus)
  if (search.paymentStatus) params.set('estadoPago', search.paymentStatus)
  if (search.page > 1) params.set('pagina', String(search.page))
  if (search.pageSize !== DEFAULT_PAGE_SIZE) params.set('porPagina', String(search.pageSize))
  return params
}
