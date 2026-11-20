import type { LibrarySongItem } from '@/interfaces/api/shared/LibrarySongItem'
import type { SuggestedTrackItem } from '@/interfaces/api/shared/SuggestedTrackItem'

export interface GetPlaylistSuggestionsDto {
  usedAudioMuse: boolean
  fromYourLibrary: LibrarySongItem[]
  newToYou: SuggestedTrackItem[]
}
