import { Group, Stack, Text } from '@mantine/core'
import { type ReactNode } from 'react'
import CoverImage from '@/components/common/CoverImage'
import SongPlayButton from '@/components/player/SongPlayButton'
import { getCoverArtUrl } from '@/helpers/coverArtHelper'
import { formatDuration } from '@/helpers/formatHelper'
import type { LibrarySongItem } from '@/interfaces/api/shared/LibrarySongItem'

interface SongRowProps {
  song: LibrarySongItem
  actions?: ReactNode
}

export default function SongRow({ song, actions }: SongRowProps) {
  return (
    <Group gap="sm" wrap="nowrap" py={6}>
      <SongPlayButton song={song} />
      <CoverImage src={getCoverArtUrl(song.coverArtId, 96)} size={44} />
      <Stack gap={0} style={{ flex: 1, minWidth: 0 }}>
        <Text size="sm" fw={500} truncate>{song.title}</Text>
        <Text size="xs" c="dimmed" truncate>{[song.artistName, song.albumTitle].filter(Boolean).join(' · ')}</Text>
      </Stack>
      <Text size="xs" c="dimmed" visibleFrom="xs">{formatDuration(song.durationSeconds)}</Text>
      {actions}
    </Group>
  )
}
