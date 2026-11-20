export const formatDuration = (totalSeconds: number | null | undefined): string => {
  if (totalSeconds === null || totalSeconds === undefined || totalSeconds <= 0) {
    return ''
  }

  const hours = Math.floor(totalSeconds / 3600)
  const minutes = Math.floor((totalSeconds % 3600) / 60)
  const seconds = Math.floor(totalSeconds % 60)

  if (hours > 0) {
    return `${hours}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`
  }

  return `${minutes}:${seconds.toString().padStart(2, '0')}`
}

export const formatFanCount = (fans: number | null | undefined): string => {
  if (fans === null || fans === undefined) {
    return ''
  }

  if (fans >= 1_000_000) {
    return `${(fans / 1_000_000).toFixed(1)}M fans`
  }

  if (fans >= 1_000) {
    return `${Math.round(fans / 1_000)}K fans`
  }

  return `${fans} fans`
}

export const formatBecauseOf = (artists: string[]): string => {
  if (artists.length === 0) {
    return ''
  }

  if (artists.length <= 2) {
    return `Because you listen to ${artists.join(' and ')}`
  }

  return `Because you listen to ${artists.slice(0, 2).join(', ')} and ${artists.length - 2} more`
}
