import type { PlaylistItem } from '@/interfaces/api/playlists/GetPlaylistsDto'
import type { LibrarySongItem } from '@/interfaces/api/shared/LibrarySongItem'

export interface GetPlaylistDto {
  playlist: PlaylistItem
  songs: LibrarySongItem[]
}
