import { LibraryState } from '@/interfaces/api/status/LibraryState'

const labels: Record<LibraryState, string> = {
  [LibraryState.New]: 'New to you',
  [LibraryState.InLidarr]: 'In Lidarr',
  [LibraryState.InLibrary]: 'In your library',
}

const colours: Record<LibraryState, string> = {
  [LibraryState.New]: 'violet',
  [LibraryState.InLidarr]: 'yellow',
  [LibraryState.InLibrary]: 'green',
}

export const getLibraryStateLabel = (state: LibraryState): string => labels[state]

export const getLibraryStateColour = (state: LibraryState): string => colours[state]
