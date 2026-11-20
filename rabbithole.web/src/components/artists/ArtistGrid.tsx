import { SimpleGrid } from '@mantine/core'
import ArtistCard from '@/components/artists/ArtistCard'
import type { ArtistSummaryItem } from '@/interfaces/api/shared/ArtistSummaryItem'

export interface ArtistGridEntry {
  artist: ArtistSummaryItem
  becauseOf?: string[]
}

interface ArtistGridProps {
  entries: ArtistGridEntry[]
}

export default function ArtistGrid({ entries }: ArtistGridProps) {
  return (
    <SimpleGrid cols={{ base: 2, xs: 3, md: 4, lg: 6 }} spacing="md">
      {entries.map(entry => (
        <ArtistCard key={entry.artist.deezerArtistId} artist={entry.artist} becauseOf={entry.becauseOf} />
      ))}
    </SimpleGrid>
  )
}
