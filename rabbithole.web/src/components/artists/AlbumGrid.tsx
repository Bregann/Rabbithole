import { SimpleGrid } from '@mantine/core'
import AlbumCard from '@/components/artists/AlbumCard'
import type { AlbumItem } from '@/interfaces/api/artists/GetArtistDto'

interface AlbumGridProps {
  albums: AlbumItem[]
  pendingAlbumId: number | null
  onRequest: (_deezerAlbumId: number) => void
}

export default function AlbumGrid({ albums, pendingAlbumId, onRequest }: AlbumGridProps) {
  return (
    <SimpleGrid cols={{ base: 2, xs: 3, md: 4, lg: 6 }} spacing="md">
      {albums.map(album => (
        <AlbumCard
          key={album.deezerAlbumId}
          album={album}
          isRequesting={pendingAlbumId === album.deezerAlbumId}
          onRequest={onRequest}
        />
      ))}
    </SimpleGrid>
  )
}
