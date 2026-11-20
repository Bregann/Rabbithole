export interface LibrarySongItem {
  songId: string
  title: string
  artistName: string | null
  albumTitle: string | null
  durationSeconds: number | null
  coverArtId: string | null
}
