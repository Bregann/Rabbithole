export interface GetBlockedArtistsDto {
  artists: BlockedArtistItem[]
}

export interface BlockedArtistItem {
  id: number
  artistName: string
  deezerArtistId: number | null
  createdAt: string
}
