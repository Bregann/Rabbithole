import { Badge, Button, Card, Stack, Text } from '@mantine/core'
import { IconDownload } from '@tabler/icons-react'
import CoverImage from '@/components/common/CoverImage'
import { getRequestStatusColour, getRequestStatusLabel, isRequestActive } from '@/helpers/requestStatusHelper'
import type { AlbumItem } from '@/interfaces/api/artists/GetArtistDto'

interface AlbumCardProps {
  album: AlbumItem
  isRequesting: boolean
  onRequest: (_deezerAlbumId: number) => void
}

export default function AlbumCard({ album, isRequesting, onRequest }: AlbumCardProps) {
  return (
    <Card withBorder padding="sm">
      <Stack gap={6}>
        <CoverImage src={album.coverUrl} size={160} radius="md" />
        <Text size="sm" fw={600} lineClamp={2}>{album.title}</Text>
        <Text size="xs" c="dimmed">
          {[album.releaseYear, album.recordType].filter(Boolean).join(' · ')}
        </Text>
        {isRequestActive(album.requestStatus) && album.requestStatus !== null
          ? <Badge color={getRequestStatusColour(album.requestStatus)} variant="light">{getRequestStatusLabel(album.requestStatus)}</Badge>
          : (
            <Button size="xs" variant="light" leftSection={<IconDownload size={14} />} loading={isRequesting} onClick={() => onRequest(album.deezerAlbumId)}>
              Request
            </Button>
          )}
      </Stack>
    </Card>
  )
}
