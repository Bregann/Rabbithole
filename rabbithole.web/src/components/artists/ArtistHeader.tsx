import { Button, Group, Stack, Text, Title } from '@mantine/core'
import { IconEyeOff, IconSpiral } from '@tabler/icons-react'
import { Link } from 'react-router'
import CoverImage from '@/components/common/CoverImage'
import LibraryStateBadge from '@/components/common/LibraryStateBadge'
import { formatFanCount } from '@/helpers/formatHelper'
import type { ArtistSummaryItem } from '@/interfaces/api/shared/ArtistSummaryItem'

interface ArtistHeaderProps {
  artist: ArtistSummaryItem
  onHide: () => void
  isHiding: boolean
}

export default function ArtistHeader({ artist, onHide, isHiding }: ArtistHeaderProps) {
  return (
    <Group gap="xl" align="center" wrap="wrap">
      <CoverImage src={artist.imageUrl} size={180} round />
      <Stack gap="xs">
        <Title order={1}>{artist.name}</Title>
        <Group gap="xs">
          <LibraryStateBadge state={artist.libraryState} size="sm" />
          {artist.fanCount !== null && <Text size="sm" c="dimmed">{formatFanCount(artist.fanCount)}</Text>}
        </Group>
        <Group gap="xs" mt="xs">
          <Button component={Link} to={`/rabbithole/${artist.deezerArtistId}`} leftSection={<IconSpiral size={16} />}>
            Go down the rabbithole
          </Button>
          {!artist.isBlocked && (
            <Button variant="subtle" color="gray" leftSection={<IconEyeOff size={16} />} onClick={onHide} loading={isHiding}>
              Not for me
            </Button>
          )}
        </Group>
      </Stack>
    </Group>
  )
}
