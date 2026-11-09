export interface Session {
  token: string
  username: string
  expiresAt: string
}

const STORAGE_KEY = 'invoices.session'
const listeners = new Set<() => void>()

function loadSession(): Session | null {
  try {
    const raw = globalThis.localStorage?.getItem(STORAGE_KEY)
    if (!raw) return null
    const session = JSON.parse(raw) as Session
    return new Date(session.expiresAt).getTime() > Date.now() ? session : null
  } catch {
    return null
  }
}

let current: Session | null = loadSession()

export const sessionStore = {
  get: (): Session | null => current,

  set(session: Session | null): void {
    current = session
    try {
      if (session) globalThis.localStorage?.setItem(STORAGE_KEY, JSON.stringify(session))
      else globalThis.localStorage?.removeItem(STORAGE_KEY)
    } catch {
    }
    listeners.forEach((listener) => listener())
  },

  subscribe(listener: () => void): () => void {
    listeners.add(listener)
    return () => {
      listeners.delete(listener)
    }
  },
}
