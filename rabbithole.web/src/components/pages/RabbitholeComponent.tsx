import { Button, Card, Grid, Group, Stack, Text, Title } from '@mantine/core'
import { IconArrowRight } from '@tabler/icons-react'
import { useQuery } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import TopTracksList from '@/components/artists/TopTracksList'
import PageLoader from '@/components/common/PageLoader'
import QueryErrorAlert from '@/components/common/QueryErrorAlert'
import ArtistGraph from '@/components/rabbithole/ArtistGraph'
import GraphLegend from '@/components/rabbithole/GraphLegend'
import RabbitholeTrail, { type TrailEntry } from '@/components/rabbithole/RabbitholeTrail'
import { doQueryGet } from '@/helpers/apiClient'
import { QueryKeys } from '@/helpers/QueryKeys'
import { useOwnPlaylists } from '@/hooks/useOwnPlaylists'
import { useRequestActions } from '@/hooks/useRequestActions'
import type { GetArtistDto } from '@/interfaces/api/artists/GetArtistDto'
import type { GetSimilarArtistsDto } from '@/interfaces/api/artists/GetSimilarArtistsDto'
import type { ArtistSummaryItem } from '@/interfaces/api/shared/ArtistSummaryItem'

// Visual exploration: click through similar artists, keeping a trail of where you've been
export default function RabbitholeComponent() {
  const { artistId } = useParams()
  const navigate = useNavigate()
  const playlists = useOwnPlaylists()
  const { requestTrack, pendingTrackId } = useRequestActions()
  const [trail, setTrail] = useState<TrailEntry[]>([])

  const graphQuery = useQuery({
    queryKey: [QueryKeys.SimilarArtists, artistId],
    queryFn: async () => await doQueryGet<GetSimilarArtistsDto>(`/api/artists/GetSimilarArtists?deezerArtistId=${artistId}`),
    enabled: artistId !== undefined
  })

  const artistQuery = useQuery({
    queryKey: [QueryKeys.Artist, artistId],
    queryFn: async () => await doQueryGet<GetArtistDto>(`/api/artists/GetArtist?deezerArtistId=${artistId}`),
    enabled: artistId !== undefined
  })

  // Starting from a link (not a click in the graph) begins a fresh trail
  const centre = graphQuery.data?.artist
  useEffect(() => {
    if (centre !== undefined && !trail.some(t => t.deezerArtistId === centre.deezerArtistId)) {
      setTrail([{ deezerArtistId: centre.deezerArtistId, name: centre.name }])
    }
  }, [centre, trail])

  const goTo = (artist: ArtistSummaryItem) => {
    setTrail(current => [...current, { deezerArtistId: artist.deezerArtistId, name: artist.name }])
    void navigate(`/rabbithole/${artist.deezerArtistId}`)
  }

  const jumpBack = (index: number) => {
    const target = trail[index]
    setTrail(current => current.slice(0, index + 1))
    void navigate(`/rabbithole/${target.deezerArtistId}`)
  }

  return (
    <Stack gap="md">
      <Group justify="space-between" align="flex-end">
        <Stack gap={4}>
          <Title order={1}>Rabbithole</Title>
          <RabbitholeTrail trail={trail} onJump={jumpBack} />
        </Stack>
        <GraphLegend />
      </Group>

      {graphQuery.error !== null && <QueryErrorAlert title="Couldn't load similar artists" error={graphQuery.error} />}

      <Grid gutter="lg">
        <Grid.Col span={{ base: 12, lg: 8 }}>
          <Card withBorder padding="xs">
            {graphQuery.isLoading && <PageLoader />}
            {graphQuery.data !== undefined && (
              <ArtistGraph centre={graphQuery.data.artist} similar={graphQuery.data.similarArtists} onSelect={goTo} />
            )}
          </Card>
        </Grid.Col>

        <Grid.Col span={{ base: 12, lg: 4 }}>
          <Card withBorder padding="md">
            {artistQuery.isLoading && <PageLoader />}
            {artistQuery.data !== undefined && (
              <Stack gap="sm">
                <Group justify="space-between">
                  <Text fw={700} size="lg">{artistQuery.data.artist.name}</Text>
                  <Button
                    component={Link}
                    to={`/artist/${artistQuery.data.artist.deezerArtistId}`}
                    size="xs"
                    variant="subtle"
                    rightSection={<IconArrowRight size={14} />}
                  >
                    Artist page
                  </Button>
                </Group>
                <Text size="xs" c="dimmed">Have a listen before you go deeper</Text>
                <TopTracksList
                  tracks={artistQuery.data.topTracks.slice(0, 5)}
                  playlists={playlists}
                  pendingTrackId={pendingTrackId}
                  onRequest={requestTrack}
                />
              </Stack>
            )}
          </Card>
        </Grid.Col>
      </Grid>
    </Stack>
  )
}
