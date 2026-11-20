import type { ArtistSummaryItem } from '@/interfaces/api/shared/ArtistSummaryItem'

export interface GetDiscoverFeedDto {
  seedSource: string
  seeds: string[]
  artists: DiscoverArtistItem[]
}

export interface DiscoverArtistItem {
  artist: ArtistSummaryItem
  becauseOf: string[]
}
