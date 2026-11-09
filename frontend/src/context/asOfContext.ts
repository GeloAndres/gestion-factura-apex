import { createContext, useContext } from 'react'
import type { IsoDate } from '../api/types'

/**
 * Fecha de referencia con la que la API calcula el estado de pago (Pending/Overdue).
 * null = hoy. Sirve para revisar los datos de ejemplo, que vencieron en 2025.
 */
export interface AsOfContextValue {
  asOf: IsoDate | null
  setAsOf: (value: IsoDate | null) => void
}

export const AsOfContext = createContext<AsOfContextValue | undefined>(undefined)

export function useAsOf(): AsOfContextValue {
  const context = useContext(AsOfContext)
  if (!context) throw new Error('useAsOf debe usarse dentro de <AsOfProvider>.')
  return context
}

/** Fecha sugerida para ver los tres estados de pago con los datos de ejemplo. */
export const SAMPLE_AS_OF: IsoDate = '2025-03-01'
