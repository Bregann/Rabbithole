import { Group, Stack, Text } from '@mantine/core'
import dayjs from 'dayjs'
import CoverImage from '@/components/common/CoverImage'
import RequestStatusBadge from '@/components/requests/RequestStatusBadge'
import type { RequestItem } from '@/interfaces/api/requests/GetRequestsDto'

interface RequestRowProps {
  request: RequestItem
}

export default function RequestRow({ request }: RequestRowProps) {
  return (
    <Group gap="md" wrap="nowrap" py="xs">
      <CoverImage src={request.coverUrl} size={56} />
      <Stack gap={2} style={{ flex: 1, minWidth: 0 }}>
        <Text fw={600} truncate>{request.albumTitle}</Text>
        <Text size="sm" c="dimmed" truncate>{request.artistName}</Text>
        {request.statusMessage && <Text size="xs" c="dimmed">{request.statusMessage}</Text>}
        {request.pendingPlaylistTracks > 0 && (
          <Text size="xs" c="violet">{request.pendingPlaylistTracks} song{request.pendingPlaylistTracks === 1 ? '' : 's'} waiting to go into playlists</Text>
        )}
      </Stack>
      <Stack gap={4} align="flex-end">
        <RequestStatusBadge status={request.status} />
        <Text size="xs" c="dimmed">{dayjs(request.requestedAt).format('D MMM')}</Text>
      </Stack>
    </Group>
  )
}
