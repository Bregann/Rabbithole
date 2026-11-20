import { Group, Text } from '@mantine/core'
import { Link } from 'react-router'
import TrackActionsMenu from '@/components/artists/TrackActionsMenu'
import TrackRow from '@/components/artists/TrackRow'
import LibraryStateBadge from '@/components/common/LibraryStateBadge'
import type { WeeklyPickItem } from '@/interfaces/api/discover/GetWeeklyPicksDto'
import type { PlaylistItem } from '@/interfaces/api/playlists/GetPlaylistsDto'

interface WeeklyPickRowProps {
  pick: WeeklyPickItem
  playlists: PlaylistItem[]
  pendingTrackId: number | null
  onRequest: (_deezerTrackId: number, _playlistId: string | null) => void
}

export default function WeeklyPickRow({ pick, playlists, pendingTrackId, onRequest }: WeeklyPickRowProps) {
  if (pick.track === null) {
    // ListenBrainz suggested it but Deezer doesn't have it, so no preview
    return (
      <Group gap="sm" py={6} wrap="nowrap">
        <Text size="sm" fw={500}>{pick.title}</Text>
        <Text size="xs" c="dimmed">{pick.artistName}</Text>
        <LibraryStateBadge state={pick.libraryState} />
      </Group>
    )
  }

  const track = pick.track

  return (
    <TrackRow
      track={track}
      showArtist
      extra={
        <Group gap={6}>
          <LibraryStateBadge state={pick.libraryState} />
          {track.deezerArtistId !== null && (
            <Text component={Link} to={`/artist/${track.deezerArtistId}`} size="xs" c="violet">More like this</Text>
          )}
        </Group>
      }
      actions={
        <TrackActionsMenu
          track={track}
          playlists={playlists}
          isPending={pendingTrackId === track.deezerTrackId}
          onRequest={onRequest}
        />
      }
    />
  )
}
