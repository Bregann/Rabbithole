// All API calls go through here. URLs are relative (/api/...): in production the API serves the SPA
// from the same origin, and in development the Vite proxy forwards /api to the API.

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'

export interface FetchResponse<T> {
  data?: T
  status: number
  ok: boolean
  statusMessage?: string
}

interface ProblemDetails {
  status?: number
  title?: string
  detail?: string
}

interface RequestOptions {
  headers?: HeadersInit
  body?: unknown
  retry?: boolean
}

const MAX_RETRIES = 3

async function refreshSession(): Promise<boolean> {
  const res = await fetch('/api/auth/RefreshToken', {
    method: 'POST',
    credentials: 'include'
  })

  return res.ok
}

function redirectToLogin() {
  if (window.location.pathname !== '/login') {
    window.location.href = '/login'
  }
}

function getErrorMessage(text: string): string | undefined {
  try {
    const problemDetails: ProblemDetails = JSON.parse(text)
    return problemDetails.detail || problemDetails.title || text
  } catch {
    return text || undefined
  }
}

async function parseResponse<T>(res: Response): Promise<FetchResponse<T>> {
  const text = await res.text()

  if (!res.ok) {
    return { status: res.status, ok: false, statusMessage: getErrorMessage(text) }
  }

  if (text === '') {
    return { status: res.status, ok: true }
  }

  try {
    return { data: JSON.parse(text) as T, status: res.status, ok: true }
  } catch {
    console.warn('Failed to parse JSON response')
    return { status: res.status, ok: true }
  }
}

async function doRequest<T>(method: HttpMethod, endpoint: string, options: RequestOptions = {}): Promise<FetchResponse<T>> {
  const { body, headers = {}, retry = true } = options

  for (let attempt = 1; attempt <= MAX_RETRIES; attempt++) {
    try {
      const res = await fetch(endpoint, {
        method,
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json',
          ...headers
        },
        body: body !== undefined ? JSON.stringify(body) : undefined
      })

      // Unauthorised: refresh the session and retry once
      if (res.status === 401 && retry) {
        if (await refreshSession()) {
          return doRequest<T>(method, endpoint, { body, headers, retry: false })
        }

        redirectToLogin()
        return { status: 401, ok: false, statusMessage: 'Session expired. Please log in again.' }
      }

      return await parseResponse<T>(res)
    } catch (error) {
      console.error(`${method} ${endpoint} failed (attempt ${attempt})`, error)
    }
  }

  return { status: 500, ok: false, statusMessage: 'Could not reach Rabbithole. Is the server running?' }
}

export const doGet = <T>(endpoint: string, options?: RequestOptions) =>
  doRequest<T>('GET', endpoint, options)

export const doPost = <T>(endpoint: string, options?: RequestOptions) =>
  doRequest<T>('POST', endpoint, options)

export const doPut = <T>(endpoint: string, options?: RequestOptions) =>
  doRequest<T>('PUT', endpoint, options)

export const doPatch = <T>(endpoint: string, options?: RequestOptions) =>
  doRequest<T>('PATCH', endpoint, options)

export const doDelete = <T>(endpoint: string, options?: RequestOptions) =>
  doRequest<T>('DELETE', endpoint, options)

// For TanStack Query: throws so the query goes into its error state
export async function doQueryGet<T>(endpoint: string, options?: RequestOptions): Promise<T> {
  const res = await doGet<T>(endpoint, options)

  if (!res.ok) {
    throw new Error(res.statusMessage ?? `Failed to fetch: ${endpoint}`)
  }

  return res.data as T
}
