import { RequestStatus } from '@/interfaces/api/requests/RequestStatus'

const labels: Record<RequestStatus, string> = {
  [RequestStatus.Pending]: 'Queued',
  [RequestStatus.WaitingForLidarr]: 'Adding to Lidarr',
  [RequestStatus.Searching]: 'Searching',
  [RequestStatus.Available]: 'Downloaded',
  [RequestStatus.Failed]: 'Failed',
}

const colours: Record<RequestStatus, string> = {
  [RequestStatus.Pending]: 'gray',
  [RequestStatus.WaitingForLidarr]: 'blue',
  [RequestStatus.Searching]: 'yellow',
  [RequestStatus.Available]: 'green',
  [RequestStatus.Failed]: 'red',
}

export const getRequestStatusLabel = (status: RequestStatus): string => labels[status]

export const getRequestStatusColour = (status: RequestStatus): string => colours[status]

export const isRequestActive = (status: RequestStatus | null): boolean =>
  status !== null && status !== RequestStatus.Failed
