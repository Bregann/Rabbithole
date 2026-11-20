import type { LibraryState } from '@/interfaces/api/status/LibraryState'

export interface ArtistSummaryItem {
  deezerArtistId: number
  name: string
  imageUrl: string | null
  fanCount: number | null
  libraryState: LibraryState
  isBlocked: boolean
}
