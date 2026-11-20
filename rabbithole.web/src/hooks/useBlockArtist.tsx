import { useQueryClient } from '@tanstack/react-query'
import { IconEyeOff, IconX } from '@tabler/icons-react'
import { useMutationPost } from '@/helpers/mutations/useMutationPost'
import notificationHelper from '@/helpers/notificationHelper'
import { QueryKeys } from '@/helpers/QueryKeys'
import type { BlockArtistRequest } from '@/interfaces/api/feedback/BlockArtistRequest'

// "Not for me": hides an artist from every suggestion
export function useBlockArtist() {
  const queryClient = useQueryClient()

  const mutation = useMutationPost<BlockArtistRequest, number>({
    url: '/api/feedback/BlockArtist',
    queryKey: [QueryKeys.BlockedArtists],
    invalidateQuery: true,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: [QueryKeys.DiscoverFeed] })
      void queryClient.invalidateQueries({ queryKey: [QueryKeys.Artist] })
      void queryClient.invalidateQueries({ queryKey: [QueryKeys.SimilarArtists] })
      notificationHelper.showSuccessNotification('Hidden', 'You won\'t see them in suggestions any more', 3000, <IconEyeOff />)
    },
    onError: error => notificationHelper.showErrorNotification('Couldn\'t hide artist', error.message, 4000, <IconX />)
  })

  return {
    blockArtist: (artistName: string, deezerArtistId: number | null) => mutation.mutate({ artistName, deezerArtistId }),
    isBlocking: mutation.isPending
  }
}
