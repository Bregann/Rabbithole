import { useQuery } from '@tanstack/react-query'
import { doQueryGet } from '@/helpers/apiClient'
import { QueryKeys } from '@/helpers/QueryKeys'
import type { GetPlaylistsDto, PlaylistItem } from '@/interfaces/api/playlists/GetPlaylistsDto'

// Playlists the signed-in user can add songs to
export function useOwnPlaylists(): PlaylistItem[] {
  const { data } = useQuery({
    queryKey: [QueryKeys.Playlists],
    queryFn: async () => await doQueryGet<GetPlaylistsDto>('/api/playlists/GetPlaylists')
  })

  return (data?.playlists ?? []).filter(p => p.isOwnedByMe)
}
