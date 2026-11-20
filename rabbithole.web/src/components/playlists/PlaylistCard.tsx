import { Badge, Card, Group, Stack, Text } from '@mantine/core'
import { Link } from 'react-router'
import CoverImage from '@/components/common/CoverImage'
import { getCoverArtUrl } from '@/helpers/coverArtHelper'
import { formatDuration } from '@/helpers/formatHelper'
import type { PlaylistItem } from '@/interfaces/api/playlists/GetPlaylistsDto'

interface PlaylistCardProps {
  playlist: PlaylistItem
}

export default function PlaylistCard({ playlist }: PlaylistCardProps) {
  return (
    <Card component={Link} to={`/playlists/${playlist.id}`} withBorder padding="sm" style={{ textDecoration: 'none' }}>
      <Stack gap={6}>
        <CoverImage src={getCoverArtUrl(playlist.coverArtId, 400)} size={180} radius="md" />
        <Text fw={600} lineClamp={1}>{playlist.name}</Text>
        <Text size="xs" c="dimmed">
          {playlist.songCount} songs{playlist.durationSeconds > 0 ? ` · ${formatDuration(playlist.durationSeconds)}` : ''}
        </Text>
        <Group gap={4}>
          {playlist.isRabbitholeWeekly && <Badge size="xs" color="violet">Weekly</Badge>}
          {!playlist.isOwnedByMe && <Badge size="xs" variant="outline" color="gray">By {playlist.owner}</Badge>}
        </Group>
      </Stack>
    </Card>
  )
}
