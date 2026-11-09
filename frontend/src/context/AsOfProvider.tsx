import { useCallback, useMemo, useState, type ReactNode } from 'react'
import type { IsoDate } from '../api/types'
import { AsOfContext } from './asOfContext'

const STORAGE_KEY = 'invoices.asOf'

function readStoredAsOf(): IsoDate | null {
  try {
    const value = sessionStorage.getItem(STORAGE_KEY)
    return value && /^\d{4}-\d{2}-\d{2}$/.test(value) ? value : null
  } catch {
    return null
  }
}

export function AsOfProvider({ children }: { children: ReactNode }) {
  const [asOf, setAsOfState] = useState<IsoDate | null>(readStoredAsOf)

  const setAsOf = useCallback((value: IsoDate | null) => {
    setAsOfState(value)
    try {
      if (value) sessionStorage.setItem(STORAGE_KEY, value)
      else sessionStorage.removeItem(STORAGE_KEY)
    } catch {
    }
  }, [])

  const value = useMemo(() => ({ asOf, setAsOf }), [asOf, setAsOf])
  return <AsOfContext.Provider value={value}>{children}</AsOfContext.Provider>
}
