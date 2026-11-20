import { ActionIcon, Tooltip } from '@mantine/core'
import { IconPlayerPauseFilled, IconPlayerPlayFilled } from '@tabler/icons-react'
import { getSongKey, usePlayer } from '@/context/playerContext'
import type { LibrarySongItem } from '@/interfaces/api/shared/LibrarySongItem'

interface SongPlayButtonProps {
  song: LibrarySongItem
}

export default function SongPlayButton({ song }: SongPlayButtonProps) {
  const { nowPlaying, isPlaying, playSong } = usePlayer()
  const isCurrent = nowPlaying?.key === getSongKey(song.songId)

  return (
    <Tooltip label={isCurrent && isPlaying ? 'Pause' : 'Play from your library'}>
      <ActionIcon variant={isCurrent ? 'filled' : 'light'} color="green" radius="xl" onClick={() => void playSong(song)}>
        {isCurrent && isPlaying ? <IconPlayerPauseFilled size={16} /> : <IconPlayerPlayFilled size={16} />}
      </ActionIcon>
    </Tooltip>
  )
}
