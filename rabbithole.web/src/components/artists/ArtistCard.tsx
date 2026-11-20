import { Card, Stack, Text } from '@mantine/core'
import { Link } from 'react-router'
import CoverImage from '@/components/common/CoverImage'
import LibraryStateBadge from '@/components/common/LibraryStateBadge'
import { formatBecauseOf } from '@/helpers/formatHelper'
import type { ArtistSummaryItem } from '@/interfaces/api/shared/ArtistSummaryItem'

interface ArtistCardProps {
  artist: ArtistSummaryItem
  becauseOf?: string[]
}

export default function ArtistCard({ artist, becauseOf }: ArtistCardProps) {
  return (
    <Card
      component={Link}
      to={`/artist/${artist.deezerArtistId}`}
      withBorder
      padding="sm"
      style={{ textDecoration: 'none' }}
    >
      <Stack gap={6} align="center">
        <CoverImage src={artist.imageUrl} size={140} round />
        <Text fw={600} ta="center" lineClamp={1}>{artist.name}</Text>
        <LibraryStateBadge state={artist.libraryState} />
        {becauseOf !== undefined && becauseOf.length > 0 && (
          <Text size="xs" c="dimmed" ta="center" lineClamp={2}>{formatBecauseOf(becauseOf)}</Text>
        )}
      </Stack>
    </Card>
  )
}
