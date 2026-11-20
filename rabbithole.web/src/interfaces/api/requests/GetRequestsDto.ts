import type { RequestStatus } from '@/interfaces/api/requests/RequestStatus'

export interface GetRequestsDto {
  requests: RequestItem[]
  weeklyLimit: number | null
  remainingThisWeek: number | null
}

export interface RequestItem {
  id: number
  artistName: string
  albumTitle: string
  coverUrl: string | null
  status: RequestStatus
  statusMessage: string | null
  requestedAt: string
  completedAt: string | null
  pendingPlaylistTracks: number
}
