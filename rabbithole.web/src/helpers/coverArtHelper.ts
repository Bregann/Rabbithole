// Navidrome cover art is proxied through the API so the browser never needs Navidrome's address
export const getCoverArtUrl = (coverArtId: string | null | undefined, size = 300): string | null =>
  coverArtId ? `/api/player/CoverArt?coverArtId=${encodeURIComponent(coverArtId)}&size=${size}` : null

export const getStreamUrl = (songId: string): string =>
  `/api/player/Stream?songId=${encodeURIComponent(songId)}`
