import { createContext, useContext, useEffect, useRef, useState, type ReactNode } from 'react'
import { IconX } from '@tabler/icons-react'
import { doGet } from '@/helpers/apiClient'
import { getCoverArtUrl, getStreamUrl } from '@/helpers/coverArtHelper'
import notificationHelper from '@/helpers/notificationHelper'
import type { GetPreviewDto } from '@/interfaces/api/player/GetPreviewDto'
import type { LibrarySongItem } from '@/interfaces/api/shared/LibrarySongItem'
import type { TrackItem } from '@/interfaces/api/shared/TrackItem'

// One audio element for the whole app: Deezer previews and full songs from Navidrome share it
export interface NowPlaying {
  key: string
  title: string
  artistName: string
  coverUrl: string | null
  isPreview: boolean
}

interface PlayerContextValue {
  nowPlaying: NowPlaying | null
  isPlaying: boolean
  isLoading: boolean
  progress: number
  duration: number
  playPreview: (_track: TrackItem) => Promise<void>
  playSong: (_song: LibrarySongItem) => Promise<void>
  togglePlay: () => void
  seek: (_seconds: number) => void
  stop: () => void
}

const PlayerContext = createContext<PlayerContextValue | undefined>(undefined)

export const getPreviewKey = (deezerTrackId: number): string => `preview:${deezerTrackId}`

export const getSongKey = (songId: string): string => `song:${songId}`

interface PlayerProviderProps {
  children: ReactNode
}

export function PlayerProvider({ children }: PlayerProviderProps) {
  const audioRef = useRef<HTMLAudioElement | null>(null)
  const [nowPlaying, setNowPlaying] = useState<NowPlaying | null>(null)
  const [isPlaying, setIsPlaying] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [progress, setProgress] = useState(0)
  const [duration, setDuration] = useState(0)

  useEffect(() => {
    const audio = new Audio()
    audio.preload = 'none'
    audioRef.current = audio

    const onTimeUpdate = () => setProgress(audio.currentTime)
    const onDuration = () => setDuration(Number.isFinite(audio.duration) ? audio.duration : 0)
    const onPlay = () => setIsPlaying(true)
    const onPause = () => setIsPlaying(false)
    const onEnded = () => {
      setIsPlaying(false)
      setProgress(0)
    }

    audio.addEventListener('timeupdate', onTimeUpdate)
    audio.addEventListener('durationchange', onDuration)
    audio.addEventListener('play', onPlay)
    audio.addEventListener('pause', onPause)
    audio.addEventListener('ended', onEnded)

    return () => {
      audio.pause()
      audio.removeEventListener('timeupdate', onTimeUpdate)
      audio.removeEventListener('durationchange', onDuration)
      audio.removeEventListener('play', onPlay)
      audio.removeEventListener('pause', onPause)
      audio.removeEventListener('ended', onEnded)
    }
  }, [])

  const start = async (track: NowPlaying, source: string) => {
    const audio = audioRef.current

    if (audio === null) {
      return
    }

    setNowPlaying(track)
    setProgress(0)
    setDuration(0)
    audio.src = source

    try {
      await audio.play()
    } catch {
      notificationHelper.showErrorNotification('Playback', 'Couldn\'t play that track', 3000, <IconX />)
    }
  }

  const playPreview = async (track: TrackItem) => {
    const key = getPreviewKey(track.deezerTrackId)

    if (nowPlaying?.key === key) {
      togglePlay()
      return
    }

    setIsLoading(true)

    // Preview URLs expire, so always ask for a fresh one
    const res = await doGet<GetPreviewDto>(`/api/player/GetPreview?deezerTrackId=${track.deezerTrackId}`)
    setIsLoading(false)

    if (!res.ok || !res.data?.previewUrl) {
      notificationHelper.showErrorNotification('No preview', 'Deezer doesn\'t have a preview for this one', 3000, <IconX />)
      return
    }

    await start({
      key,
      title: track.title,
      artistName: track.artistName,
      coverUrl: track.coverUrl,
      isPreview: true
    }, res.data.previewUrl)
  }

  const playSong = async (song: LibrarySongItem) => {
    const key = getSongKey(song.songId)

    if (nowPlaying?.key === key) {
      togglePlay()
      return
    }

    // The <audio> request can't refresh an expired session itself, so make sure it's fresh first
    await doGet('/api/auth/GetCurrentUser')

    await start({
      key,
      title: song.title,
      artistName: song.artistName ?? 'Unknown artist',
      coverUrl: getCoverArtUrl(song.coverArtId, 120),
      isPreview: false
    }, getStreamUrl(song.songId))
  }

  const togglePlay = () => {
    const audio = audioRef.current

    if (audio === null || nowPlaying === null) {
      return
    }

    if (audio.paused) {
      void audio.play()
    } else {
      audio.pause()
    }
  }

  const seek = (seconds: number) => {
    if (audioRef.current !== null) {
      audioRef.current.currentTime = seconds
    }
  }

  const stop = () => {
    audioRef.current?.pause()
    setNowPlaying(null)
    setProgress(0)
  }

  return (
    <PlayerContext.Provider value={{ nowPlaying, isPlaying, isLoading, progress, duration, playPreview, playSong, togglePlay, seek, stop }}>
      {children}
    </PlayerContext.Provider>
  )
}

export function usePlayer(): PlayerContextValue {
  const context = useContext(PlayerContext)

  if (context === undefined) {
    throw new Error('usePlayer must be used inside PlayerProvider')
  }

  return context
}
