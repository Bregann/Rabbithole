export interface TrackItem {
  deezerTrackId: number
  title: string
  artistName: string
  deezerArtistId: number | null
  albumTitle: string | null
  deezerAlbumId: number | null
  coverUrl: string | null
  durationSeconds: number
  hasPreview: boolean
}
