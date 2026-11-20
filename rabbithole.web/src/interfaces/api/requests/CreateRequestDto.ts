import type { RequestStatus } from '@/interfaces/api/requests/RequestStatus'

export interface CreateRequestDto {
  requestId: number
  status: RequestStatus
  statusMessage: string | null
}
