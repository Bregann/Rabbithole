import type { TrackItem } from '@/interfaces/api/shared/TrackItem'
import type { LibraryState } from '@/interfaces/api/status/LibraryState'

export interface SuggestedTrackItem {
  track: TrackItem
  libraryState: LibraryState
  becauseOf: string[]
}
