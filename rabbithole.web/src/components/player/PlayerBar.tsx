import { ActionIcon, Badge, Group, Slider, Stack, Text } from '@mantine/core'
import { IconPlayerPauseFilled, IconPlayerPlayFilled, IconX } from '@tabler/icons-react'
import CoverImage from '@/components/common/CoverImage'
import { usePlayer } from '@/context/playerContext'
import { formatDuration } from '@/helpers/formatHelper'

export default function PlayerBar() {
  const { nowPlaying, isPlaying, progress, duration, togglePlay, seek, stop } = usePlayer()

  if (nowPlaying === null) {
    return null
  }

  return (
    <Group h="100%" px="md" gap="md" wrap="nowrap">
      <CoverImage src={nowPlaying.coverUrl} size={44} />

      <Stack gap={0} style={{ minWidth: 0, width: 220 }}>
        <Text size="sm" fw={600} truncate>{nowPlaying.title}</Text>
        <Group gap={6} wrap="nowrap">
          <Text size="xs" c="dimmed" truncate>{nowPlaying.artistName}</Text>
          {nowPlaying.isPreview && <Badge size="xs" variant="outline" color="violet">Preview</Badge>}
        </Group>
      </Stack>

      <ActionIcon size="lg" radius="xl" onClick={togglePlay} aria-label={isPlaying ? 'Pause' : 'Play'}>
        {isPlaying ? <IconPlayerPauseFilled size={18} /> : <IconPlayerPlayFilled size={18} />}
      </ActionIcon>

      <Text size="xs" c="dimmed" w={40} ta="right">{formatDuration(progress) || '0:00'}</Text>
      <Slider
        style={{ flex: 1 }}
        size="sm"
        min={0}
        max={duration || 1}
        value={progress}
        onChange={seek}
        label={null}
        disabled={duration === 0}
      />
      <Text size="xs" c="dimmed" w={40}>{formatDuration(duration)}</Text>

      <ActionIcon variant="subtle" color="gray" onClick={stop} aria-label="Close player">
        <IconX size={16} />
      </ActionIcon>
    </Group>
  )
}
