import { Badge, Button, Card, Grid, Group, Stack, Text, Title } from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import { IconCheck, IconPlus, IconX } from '@tabler/icons-react'
import { useQuery } from '@tanstack/react-query'
import { useParams } from 'react-router'
import PageLoader from '@/components/common/PageLoader'
import QueryErrorAlert from '@/components/common/QueryErrorAlert'
import SectionTitle from '@/components/common/SectionTitle'
import AddSongsModal from '@/components/playlists/AddSongsModal'
import PlaylistSongList from '@/components/playlists/PlaylistSongList'
import SuggestionsPanel from '@/components/playlists/SuggestionsPanel'
import { doQueryGet } from '@/helpers/apiClient'
import { formatDuration } from '@/helpers/formatHelper'
import { useMutationPost } from '@/helpers/mutations/useMutationPost'
import notificationHelper from '@/helpers/notificationHelper'
import { QueryKeys } from '@/helpers/QueryKeys'
import { useRequestActions } from '@/hooks/useRequestActions'
import type { AddSongsToPlaylistRequest } from '@/interfaces/api/playlists/AddSongsToPlaylistRequest'
import type { GetPlaylistDto } from '@/interfaces/api/playlists/GetPlaylistDto'
import type { GetPlaylistSuggestionsDto } from '@/interfaces/api/playlists/GetPlaylistSuggestionsDto'
import type { RemoveSongFromPlaylistRequest } from '@/interfaces/api/playlists/RemoveSongFromPlaylistRequest'

export default function PlaylistComponent() {
  const { playlistId = '' } = useParams()
  const [addOpened, { open: openAdd, close: closeAdd }] = useDisclosure(false)
  const { requestTrack, pendingTrackId } = useRequestActions()

  const playlistQuery = useQuery({
    queryKey: [QueryKeys.Playlist, playlistId],
    queryFn: async () => await doQueryGet<GetPlaylistDto>(`/api/playlists/GetPlaylist?playlistId=${encodeURIComponent(playlistId)}`)
  })

  const suggestionsQuery = useQuery({
    queryKey: [QueryKeys.PlaylistSuggestions, playlistId],
    queryFn: async () => await doQueryGet<GetPlaylistSuggestionsDto>(`/api/playlists/GetPlaylistSuggestions?playlistId=${encodeURIComponent(playlistId)}`),
    enabled: (playlistQuery.data?.songs.length ?? 0) > 0,
    staleTime: 1000 * 60 * 30
  })

  const addSongs = useMutationPost<AddSongsToPlaylistRequest, void>({
    url: '/api/playlists/AddSongsToPlaylist',
    queryKey: [QueryKeys.Playlist, playlistId],
    invalidateQuery: true,
    onSuccess: () => notificationHelper.showSuccessNotification('Added', 'Song added to the playlist', 2000, <IconCheck />),
    onError: error => notificationHelper.showErrorNotification('Couldn\'t add song', error.message, 4000, <IconX />)
  })

  const removeSong = useMutationPost<RemoveSongFromPlaylistRequest, void>({
    url: '/api/playlists/RemoveSongFromPlaylist',
    queryKey: [QueryKeys.Playlist, playlistId],
    invalidateQuery: true,
    onError: error => notificationHelper.showErrorNotification('Couldn\'t remove song', error.message, 4000, <IconX />)
  })

  if (playlistQuery.isLoading) {
    return <PageLoader />
  }

  if (playlistQuery.error !== null) {
    return <QueryErrorAlert title="Couldn't load this playlist" error={playlistQuery.error} />
  }

  if (playlistQuery.data === undefined) {
    return null
  }

  const { playlist, songs } = playlistQuery.data
  const canEdit = playlist.isOwnedByMe
  const addSong = (songId: string) => addSongs.mutate({ playlistId, songIds: [songId] })
  const addingSongId = addSongs.isPending ? addSongs.variables?.songIds[0] ?? null : null

  return (
    <Stack gap="lg">
      <Group justify="space-between" align="flex-end">
        <Stack gap={4}>
          <Group gap="xs">
            <Title order={1}>{playlist.name}</Title>
            {playlist.isRabbitholeWeekly && <Badge color="violet">Weekly</Badge>}
          </Group>
          <Text size="sm" c="dimmed">
            {playlist.songCount} songs{playlist.durationSeconds > 0 ? ` · ${formatDuration(playlist.durationSeconds)}` : ''}
            {!canEdit ? ` · by ${playlist.owner}` : ''}
          </Text>
        </Stack>
        {canEdit && <Button leftSection={<IconPlus size={16} />} onClick={openAdd}>Add songs</Button>}
      </Group>

      <Grid gutter="lg">
        <Grid.Col span={{ base: 12, lg: 7 }}>
          <Card withBorder padding="sm">
            <PlaylistSongList
              songs={songs}
              canEdit={canEdit}
              removingIndex={removeSong.isPending ? removeSong.variables?.songIndex ?? null : null}
              onRemove={songIndex => removeSong.mutate({ playlistId, songIndex })}
            />
          </Card>
        </Grid.Col>

        <Grid.Col span={{ base: 12, lg: 5 }}>
          <Card withBorder padding="sm">
            <SectionTitle title="Suggestions" subtitle="Songs that would fit, from your library and beyond" />
            {songs.length === 0 && <Text size="sm" c="dimmed">Add a few songs first and suggestions will appear here.</Text>}
            {suggestionsQuery.isLoading && <PageLoader />}
            {suggestionsQuery.error !== null && <QueryErrorAlert title="Couldn't load suggestions" error={suggestionsQuery.error} />}
            {suggestionsQuery.data !== undefined && (
              <SuggestionsPanel
                suggestions={suggestionsQuery.data}
                canEdit={canEdit}
                addingSongId={addingSongId}
                pendingTrackId={pendingTrackId}
                onAddSong={addSong}
                onRequestTrack={deezerTrackId => requestTrack(deezerTrackId, playlistId)}
              />
            )}
          </Card>
        </Grid.Col>
      </Grid>

      <AddSongsModal opened={addOpened} addingSongId={addingSongId} onClose={closeAdd} onAdd={addSong} />
    </Stack>
  )
}
