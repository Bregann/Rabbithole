import { ActionIcon, Loader, Tooltip } from '@mantine/core'
import { IconPlayerPauseFilled, IconPlayerPlayFilled } from '@tabler/icons-react'
import { getPreviewKey, usePlayer } from '@/context/playerContext'
import type { TrackItem } from '@/interfaces/api/shared/TrackItem'

interface PreviewButtonProps {
  track: TrackItem
}

export default function PreviewButton({ track }: PreviewButtonProps) {
  const { nowPlaying, isPlaying, isLoading, playPreview } = usePlayer()
  const isCurrent = nowPlaying?.key === getPreviewKey(track.deezerTrackId)

  if (!track.hasPreview) {
    return (
      <Tooltip label="No preview available">
        <ActionIcon variant="subtle" color="gray" radius="xl" disabled>
          <IconPlayerPlayFilled size={16} />
        </ActionIcon>
      </Tooltip>
    )
  }

  return (
    <Tooltip label={isCurrent && isPlaying ? 'Pause' : 'Play 30-second preview'}>
      <ActionIcon variant={isCurrent ? 'filled' : 'light'} radius="xl" onClick={() => void playPreview(track)}>
        {isCurrent && isLoading
          ? <Loader size={14} color="white" />
          : isCurrent && isPlaying ? <IconPlayerPauseFilled size={16} /> : <IconPlayerPlayFilled size={16} />}
      </ActionIcon>
    </Tooltip>
  )
}
