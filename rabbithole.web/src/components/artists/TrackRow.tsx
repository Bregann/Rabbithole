import { Group, Stack, Text } from '@mantine/core'
import { type ReactNode } from 'react'
import CoverImage from '@/components/common/CoverImage'
import PreviewButton from '@/components/player/PreviewButton'
import { formatDuration } from '@/helpers/formatHelper'
import type { TrackItem } from '@/interfaces/api/shared/TrackItem'

interface TrackRowProps {
  track: TrackItem
  showArtist?: boolean
  extra?: ReactNode
  actions?: ReactNode
}

export default function TrackRow({ track, showArtist = false, extra, actions }: TrackRowProps) {
  return (
    <Group gap="sm" wrap="nowrap" py={6}>
      <PreviewButton track={track} />
      <CoverImage src={track.coverUrl} size={44} />
      <Stack gap={0} style={{ flex: 1, minWidth: 0 }}>
        <Text size="sm" fw={500} truncate>{track.title}</Text>
        <Text size="xs" c="dimmed" truncate>
          {[showArtist ? track.artistName : null, track.albumTitle].filter(Boolean).join(' · ')}
        </Text>
        {extra}
      </Stack>
      <Text size="xs" c="dimmed" visibleFrom="xs">{formatDuration(track.durationSeconds)}</Text>
      {actions}
    </Group>
  )
}
