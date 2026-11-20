import type { ArtistSummaryItem } from '@/interfaces/api/shared/ArtistSummaryItem'

export interface GetSimilarArtistsDto {
  artist: ArtistSummaryItem
  similarArtists: ArtistSummaryItem[]
}
