import type { TrackItem } from '@/interfaces/api/shared/TrackItem'
import type { LibraryState } from '@/interfaces/api/status/LibraryState'

export interface GetWeeklyPicksDto {
  isListenBrainzLinked: boolean
  playlistTitle: string | null
  picks: WeeklyPickItem[]
}

export interface WeeklyPickItem {
  title: string
  artistName: string
  libraryState: LibraryState
  track: TrackItem | null
}
