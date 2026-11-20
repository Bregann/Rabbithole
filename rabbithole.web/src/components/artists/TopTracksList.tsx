import { Divider, Stack } from '@mantine/core'
import { Fragment } from 'react'
import TrackActionsMenu from '@/components/artists/TrackActionsMenu'
import TrackRow from '@/components/artists/TrackRow'
import type { PlaylistItem } from '@/interfaces/api/playlists/GetPlaylistsDto'
import type { TrackItem } from '@/interfaces/api/shared/TrackItem'

interface TopTracksListProps {
  tracks: TrackItem[]
  playlists: PlaylistItem[]
  pendingTrackId: number | null
  onRequest: (_deezerTrackId: number, _playlistId: string | null) => void
}

export default function TopTracksList({ tracks, playlists, pendingTrackId, onRequest }: TopTracksListProps) {
  return (
    <Stack gap={0}>
      {tracks.map((track, index) => (
        <Fragment key={track.deezerTrackId}>
          {index > 0 && <Divider />}
          <TrackRow
            track={track}
            actions={
              <TrackActionsMenu
                track={track}
                playlists={playlists}
                isPending={pendingTrackId === track.deezerTrackId}
                onRequest={onRequest}
              />
            }
          />
        </Fragment>
      ))}
    </Stack>
  )
}
