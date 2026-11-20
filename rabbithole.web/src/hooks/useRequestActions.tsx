import { useQueryClient } from '@tanstack/react-query'
import { IconCheck, IconX } from '@tabler/icons-react'
import { useMutationPost } from '@/helpers/mutations/useMutationPost'
import notificationHelper from '@/helpers/notificationHelper'
import { QueryKeys } from '@/helpers/QueryKeys'
import { getRequestStatusLabel } from '@/helpers/requestStatusHelper'
import type { CreateRequestDto } from '@/interfaces/api/requests/CreateRequestDto'
import type { RequestAlbumRequest } from '@/interfaces/api/requests/RequestAlbumRequest'
import type { RequestTrackRequest } from '@/interfaces/api/requests/RequestTrackRequest'

// Album and song requests to Lidarr, with notifications and cache refreshes in one place
export function useRequestActions() {
  const queryClient = useQueryClient()

  const onSuccess = (data: CreateRequestDto | undefined, message: string) => {
    void queryClient.invalidateQueries({ queryKey: [QueryKeys.Artist] })
    void queryClient.invalidateQueries({ queryKey: [QueryKeys.Requests] })

    const status = data !== undefined ? ` (${getRequestStatusLabel(data.status)})` : ''
    notificationHelper.showSuccessNotification('Requested', `${message}${status}`, 4000, <IconCheck />)
  }

  const onError = (error: Error) => {
    notificationHelper.showErrorNotification('Request failed', error.message, 5000, <IconX />)
  }

  const albumMutation = useMutationPost<RequestAlbumRequest, CreateRequestDto>({
    url: '/api/requests/RequestAlbum',
    queryKey: [QueryKeys.Requests],
    invalidateQuery: true,
    onSuccess: data => onSuccess(data, 'Sent to Lidarr'),
    onError
  })

  const trackMutation = useMutationPost<RequestTrackRequest, CreateRequestDto>({
    url: '/api/requests/RequestTrack',
    queryKey: [QueryKeys.Requests],
    invalidateQuery: true,
    onSuccess: (data) => onSuccess(data, 'The album is on its way'),
    onError
  })

  return {
    requestAlbum: (deezerAlbumId: number) => albumMutation.mutate({ deezerAlbumId }),
    requestTrack: (deezerTrackId: number, playlistId: string | null) => trackMutation.mutate({ deezerTrackId, playlistId }),
    pendingAlbumId: albumMutation.isPending ? albumMutation.variables?.deezerAlbumId ?? null : null,
    pendingTrackId: trackMutation.isPending ? trackMutation.variables?.deezerTrackId ?? null : null
  }
}
