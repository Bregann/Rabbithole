import type { RequestStatus } from '@/interfaces/api/requests/RequestStatus'
import type { ArtistSummaryItem } from '@/interfaces/api/shared/ArtistSummaryItem'
import type { TrackItem } from '@/interfaces/api/shared/TrackItem'

export interface GetArtistDto {
  artist: ArtistSummaryItem
  topTracks: TrackItem[]
  albums: AlbumItem[]
  similarArtists: ArtistSummaryItem[]
}

export interface AlbumItem {
  deezerAlbumId: number
  title: string
  coverUrl: string | null
  releaseYear: number | null
  recordType: string | null
  requestStatus: RequestStatus | null
}
