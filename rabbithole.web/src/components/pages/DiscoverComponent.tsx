import { ActionIcon, Alert, Button, Card, Stack, Title, Tooltip } from '@mantine/core'
import { IconCheck, IconInfoCircle, IconRefresh, IconX } from '@tabler/icons-react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router'
import ArtistGrid from '@/components/artists/ArtistGrid'
import EmptyState from '@/components/common/EmptyState'
import PageLoader from '@/components/common/PageLoader'
import QueryErrorAlert from '@/components/common/QueryErrorAlert'
import SectionTitle from '@/components/common/SectionTitle'
import SeedSummary from '@/components/discover/SeedSummary'
import WeeklyPicksList from '@/components/discover/WeeklyPicksList'
import { doQueryGet } from '@/helpers/apiClient'
import { useMutationPost } from '@/helpers/mutations/useMutationPost'
import notificationHelper from '@/helpers/notificationHelper'
import { QueryKeys } from '@/helpers/QueryKeys'
import { useOwnPlaylists } from '@/hooks/useOwnPlaylists'
import { useRequestActions } from '@/hooks/useRequestActions'
import type { GetDiscoverFeedDto } from '@/interfaces/api/discover/GetDiscoverFeedDto'
import type { GetWeeklyPicksDto } from '@/interfaces/api/discover/GetWeeklyPicksDto'
import type { SyncWeeklyPlaylistDto } from '@/interfaces/api/discover/SyncWeeklyPlaylistDto'

export default function DiscoverComponent() {
  const queryClient = useQueryClient()
  const playlists = useOwnPlaylists()
  const { requestTrack, pendingTrackId } = useRequestActions()

  const feedQuery = useQuery({
    queryKey: [QueryKeys.DiscoverFeed],
    queryFn: async () => await doQueryGet<GetDiscoverFeedDto>('/api/discover/GetFeed')
  })

  const weeklyQuery = useQuery({
    queryKey: [QueryKeys.WeeklyPicks],
    queryFn: async () => await doQueryGet<GetWeeklyPicksDto>('/api/discover/GetWeeklyPicks')
  })

  const refreshFeed = async () => {
    const fresh = await doQueryGet<GetDiscoverFeedDto>('/api/discover/GetFeed?refresh=true')
    queryClient.setQueryData([QueryKeys.DiscoverFeed], fresh)
  }

  const { mutate: syncWeekly, isPending: isSyncing } = useMutationPost<void, SyncWeeklyPlaylistDto>({
    url: '/api/discover/SyncMyWeeklyPlaylist',
    queryKey: [QueryKeys.Playlists],
    invalidateQuery: true,
    onSuccess: data => notificationHelper.showSuccessNotification(
      'Weekly playlist updated',
      `${data?.matchedSongs ?? 0} of ${data?.totalTracks ?? 0} picks were already in your library`,
      4000,
      <IconCheck />),
    onError: error => notificationHelper.showErrorNotification('Couldn\'t update', error.message, 4000, <IconX />)
  })

  const feed = feedQuery.data
  const weekly = weeklyQuery.data

  return (
    <Stack gap="xl">
      <Stack gap={4}>
        <Title order={1}>Discover</Title>
        {feed !== undefined && <SeedSummary source={feed.seedSource} seeds={feed.seeds} />}
      </Stack>

      <section>
        <SectionTitle
          title="Artists you might like"
          subtitle="Click one to see who they're like. Keep clicking."
          action={
            <Tooltip label="Rebuild suggestions">
              <ActionIcon variant="subtle" onClick={() => void refreshFeed()} loading={feedQuery.isFetching} aria-label="Refresh">
                <IconRefresh size={18} />
              </ActionIcon>
            </Tooltip>
          }
        />
        {feedQuery.isLoading && <PageLoader />}
        {feedQuery.error !== null && <QueryErrorAlert title="Couldn't build your suggestions" error={feedQuery.error} />}
        {feed !== undefined && feed.artists.length === 0 && (
          <EmptyState
            title="Not enough listening history yet"
            description="Play some music in Navidrome, or link ListenBrainz in Settings, and suggestions will appear here."
            action={<Button component={Link} to="/settings" variant="light">Settings</Button>}
          />
        )}
        {feed !== undefined && feed.artists.length > 0 && (
          <ArtistGrid entries={feed.artists.map(a => ({ artist: a.artist, becauseOf: a.becauseOf }))} />
        )}
      </section>

      <section>
        <SectionTitle
          title={weekly?.playlistTitle ?? 'Weekly picks'}
          subtitle="This week's songs from ListenBrainz, based on what you've been playing"
          action={weekly?.isListenBrainzLinked && (
            <Button size="xs" variant="light" loading={isSyncing} onClick={() => syncWeekly()}>
              Update my Rabbithole Weekly playlist
            </Button>
          )}
        />
        {weeklyQuery.isLoading && <PageLoader />}
        {weeklyQuery.error !== null && <QueryErrorAlert title="Couldn't load weekly picks" error={weeklyQuery.error} />}
        {weekly !== undefined && !weekly.isListenBrainzLinked && (
          <Alert variant="light" icon={<IconInfoCircle />} title="Link ListenBrainz for weekly picks">
            Add your ListenBrainz username in <Link to="/settings">Settings</Link> to get a fresh set of songs every week.
          </Alert>
        )}
        {weekly !== undefined && weekly.isListenBrainzLinked && weekly.picks.length === 0 && (
          <EmptyState title="No weekly picks yet" description="ListenBrainz builds these on Mondays once it has enough of your listens." />
        )}
        {weekly !== undefined && weekly.picks.length > 0 && (
          <Card withBorder padding="sm">
            <WeeklyPicksList picks={weekly.picks} playlists={playlists} pendingTrackId={pendingTrackId} onRequest={requestTrack} />
          </Card>
        )}
      </section>
    </Stack>
  )
}
