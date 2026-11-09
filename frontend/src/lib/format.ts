const currency = new Intl.NumberFormat('es-CL', { style: 'currency', currency: 'CLP', maximumFractionDigits: 0 })
const integer = new Intl.NumberFormat('es-CL', { maximumFractionDigits: 0 })
const percent = new Intl.NumberFormat('es-CL', { style: 'percent', maximumFractionDigits: 1 })

/** Monto en pesos chilenos: 265019 → "$265.019". */
export const formatCLP = (amount: number): string => currency.format(amount)

/** Diferencia con signo antes del símbolo: −137165 → "−$137.165"; 1000 → "+$1.000". */
export const formatSignedCLP = (amount: number): string =>
  amount === 0 ? formatCLP(0) : `${amount < 0 ? '−' : '+'}${formatCLP(Math.abs(amount))}`

export const formatInteger = (value: number): string => integer.format(value)

/** Recibe un porcentaje de 0 a 100: 27.1 → "27,1 %". */
export const formatPercent = (value: number): string => percent.format(value / 100)

/**
 * Fecha ISO "2025-03-01" → "01-03-2025". Se arma a mano para no pasar por Date,
 * que interpreta la fecha en UTC y en Chile la mostraría un día antes.
 */
export function formatDate(isoDate: string): string {
  const [year, month, day] = isoDate.split('-')
  return year && month && day ? `${day}-${month}-${year}` : isoDate
}

/**
 * Interpreta un monto escrito por el usuario. Acepta separador de miles con punto
 * y el signo $ ("$12.000"); rechaza decimales. Devuelve null si no es un entero válido.
 */
export function parseAmount(input: string): number | null {
  const digits = input.replace(/[\s$.]/g, '')
  if (!/^\d+$/.test(digits)) return null
  const amount = Number(digits)
  return Number.isSafeInteger(amount) ? amount : null
}
