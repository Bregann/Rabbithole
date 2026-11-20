export interface GetPlaylistsDto {
  playlists: PlaylistItem[]
}

export interface PlaylistItem {
  id: string
  name: string
  songCount: number
  durationSeconds: number
  coverArtId: string | null
  isPublic: boolean
  owner: string | null
  isOwnedByMe: boolean
  isRabbitholeWeekly: boolean
}
