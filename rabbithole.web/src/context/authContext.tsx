import { useQuery, useQueryClient } from '@tanstack/react-query'
import { createContext, useContext, type ReactNode } from 'react'
import { doPost, doQueryGet } from '@/helpers/apiClient'
import { QueryKeys } from '@/helpers/QueryKeys'
import type { GetCurrentUserDto } from '@/interfaces/api/auth/GetCurrentUserDto'
import type { LoginRequest } from '@/interfaces/api/auth/LoginRequest'

interface AuthContextValue {
  user: GetCurrentUserDto | null
  isLoading: boolean
  login: (_request: LoginRequest) => Promise<string | null>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

interface AuthProviderProps {
  children: ReactNode
}

export function AuthProvider({ children }: AuthProviderProps) {
  const queryClient = useQueryClient()

  const { data: user, isLoading } = useQuery({
    queryKey: [QueryKeys.CurrentUser],
    queryFn: async () => {
      try {
        return await doQueryGet<GetCurrentUserDto>('/api/auth/GetCurrentUser')
      } catch {
        return null
      }
    },
    retry: false,
    staleTime: Infinity
  })

  // Returns an error message, or null on success
  const login = async (request: LoginRequest): Promise<string | null> => {
    const res = await doPost<GetCurrentUserDto>('/api/auth/Login', { body: request, retry: false })

    if (!res.ok || res.data === undefined) {
      return res.statusMessage ?? 'Sign-in failed'
    }

    queryClient.setQueryData([QueryKeys.CurrentUser], res.data)
    return null
  }

  const logout = async () => {
    await doPost('/api/auth/Logout', { retry: false })
    queryClient.clear()
    queryClient.setQueryData([QueryKeys.CurrentUser], null)
  }

  return (
    <AuthContext.Provider value={{ user: user ?? null, isLoading, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)

  if (context === undefined) {
    throw new Error('useAuth must be used inside AuthProvider')
  }

  return context
}
