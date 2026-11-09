import { createContext, useContext } from 'react'
import type { IsoDate } from '../api/types'

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

export const SAMPLE_AS_OF: IsoDate = '2025-03-01'
