import { Card, Stack } from '@mantine/core'
import { useQuery } from '@tanstack/react-query'
import { useParams } from 'react-router'
import AlbumGrid from '@/components/artists/AlbumGrid'
import ArtistGrid from '@/components/artists/ArtistGrid'
import ArtistHeader from '@/components/artists/ArtistHeader'
import TopTracksList from '@/components/artists/TopTracksList'
import EmptyState from '@/components/common/EmptyState'
import PageLoader from '@/components/common/PageLoader'
import QueryErrorAlert from '@/components/common/QueryErrorAlert'
import SectionTitle from '@/components/common/SectionTitle'
import { doQueryGet } from '@/helpers/apiClient'
import { QueryKeys } from '@/helpers/QueryKeys'
import { useBlockArtist } from '@/hooks/useBlockArtist'
import { useOwnPlaylists } from '@/hooks/useOwnPlaylists'
import { useRequestActions } from '@/hooks/useRequestActions'
import type { GetArtistDto } from '@/interfaces/api/artists/GetArtistDto'

export default function ArtistComponent() {
  const { artistId } = useParams()
  const playlists = useOwnPlaylists()
  const { requestAlbum, requestTrack, pendingAlbumId, pendingTrackId } = useRequestActions()
  const { blockArtist, isBlocking } = useBlockArtist()

  const { data, isLoading, error } = useQuery({
    queryKey: [QueryKeys.Artist, artistId],
    queryFn: async () => await doQueryGet<GetArtistDto>(`/api/artists/GetArtist?deezerArtistId=${artistId}`),
    enabled: artistId !== undefined
  })

  if (isLoading) {
    return <PageLoader />
  }

  if (error !== null) {
    return <QueryErrorAlert title="Couldn't load this artist" error={error} />
  }

  if (data === undefined) {
    return null
  }

  return (
    <Stack gap="xl">
      <ArtistHeader
        artist={data.artist}
        isHiding={isBlocking}
        onHide={() => blockArtist(data.artist.name, data.artist.deezerArtistId)}
      />

      <section>
        <SectionTitle title="Top tracks" subtitle="Press play for a 30-second preview" />
        <Card withBorder padding="sm">
          <TopTracksList tracks={data.topTracks} playlists={playlists} pendingTrackId={pendingTrackId} onRequest={requestTrack} />
        </Card>
      </section>

      <section>
        <SectionTitle title="Similar artists" subtitle="Coloured by what you already have" />
        {data.similarArtists.length === 0
          ? <EmptyState title="No similar artists found" />
          : <ArtistGrid entries={data.similarArtists.map(artist => ({ artist }))} />}
      </section>

      <section>
        <SectionTitle title="Albums" subtitle="Request one and Lidarr will fetch it" />
        <AlbumGrid albums={data.albums} pendingAlbumId={pendingAlbumId} onRequest={requestAlbum} />
      </section>
    </Stack>
  )
}
