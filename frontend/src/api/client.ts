import { sessionStore } from '../auth/session'
import type { ProblemDetails } from './types'

const API_URL = (import.meta.env.VITE_API_URL ?? '').replace(/\/$/, '')

export class ApiError extends Error {
  readonly status: number
  readonly code: string | undefined
  readonly fieldErrors: Record<string, string[]>

  constructor(status: number, message: string, code?: string, fieldErrors: Record<string, string[]> = {}) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.code = code
    this.fieldErrors = fieldErrors
  }
}

type QueryValue = string | number | null | undefined

interface RequestOptions {
  method?: 'GET' | 'POST'
  query?: Record<string, QueryValue>
  body?: unknown
  formData?: FormData
  signal?: AbortSignal
  auth?: boolean
}

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const url = new URL(`${API_URL}${path}`, window.location.origin)
  for (const [key, value] of Object.entries(options.query ?? {})) {
    if (value !== undefined && value !== null && value !== '') url.searchParams.set(key, String(value))
  }

  const headers = new Headers({ Accept: 'application/json' })
  const token = options.auth === false ? null : (sessionStore.get()?.token ?? null)
  if (token) headers.set('Authorization', `Bearer ${token}`)

  let body: BodyInit | undefined
  if (options.formData) {
    body = options.formData
  } else if (options.body !== undefined) {
    headers.set('Content-Type', 'application/json')
    body = JSON.stringify(options.body)
  }

  let response: Response
  try {
    response = await fetch(url, { method: options.method ?? 'GET', headers, body, signal: options.signal })
  } catch (error) {
    if (error instanceof DOMException && error.name === 'AbortError') throw error
    throw new ApiError(0, 'No se pudo conectar con la API. Revisa que esté corriendo en http://localhost:5080.')
  }

  if (response.status === 401 && token) {
    sessionStore.set(null)
  }

  if (!response.ok) throw await toApiError(response)
  if (response.status === 204) return undefined as T
  return (await response.json()) as T
}

async function toApiError(response: Response): Promise<ApiError> {
  let problem: ProblemDetails | null = null
  try {
    problem = (await response.json()) as ProblemDetails
  } catch {
  }

  const fieldErrors = problem?.errors ?? {}
  const firstFieldError = Object.values(fieldErrors).flat()[0]
  const message = problem?.detail ?? firstFieldError ?? problem?.title ?? defaultMessage(response.status)
  return new ApiError(response.status, message, problem?.code, fieldErrors)
}

function defaultMessage(status: number): string {
  if (status === 401) return 'Tu sesión expiró. Vuelve a iniciar sesión.'
  if (status === 404) return 'No se encontró el recurso solicitado.'
  if (status >= 500) return 'La API tuvo un error inesperado. Intenta nuevamente.'
  return `La solicitud falló (HTTP ${status}).`
}

export function errorMessage(error: unknown): string {
  if (error instanceof Error) return error.message
  return 'Ocurrió un error inesperado.'
}
