const currency = new Intl.NumberFormat('es-CL', { style: 'currency', currency: 'CLP', maximumFractionDigits: 0 })
const integer = new Intl.NumberFormat('es-CL', { maximumFractionDigits: 0 })
const percent = new Intl.NumberFormat('es-CL', { style: 'percent', maximumFractionDigits: 1 })

export const formatCLP = (amount: number): string => currency.format(amount)

export const formatSignedCLP = (amount: number): string =>
  amount === 0 ? formatCLP(0) : `${amount < 0 ? '−' : '+'}${formatCLP(Math.abs(amount))}`

export const formatInteger = (value: number): string => integer.format(value)

export const formatPercent = (value: number): string => percent.format(value / 100)

export function formatDate(isoDate: string): string {
  const [year, month, day] = isoDate.split('-')
  return year && month && day ? `${day}-${month}-${year}` : isoDate
}

export function parseAmount(input: string): number | null {
  const digits = input.replace(/[\s$.]/g, '')
  if (!/^\d+$/.test(digits)) return null
  const amount = Number(digits)
  return Number.isSafeInteger(amount) ? amount : null
}
