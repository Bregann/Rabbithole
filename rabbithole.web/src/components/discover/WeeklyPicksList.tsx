import { Divider, Stack } from '@mantine/core'
import { Fragment } from 'react'
import WeeklyPickRow from '@/components/discover/WeeklyPickRow'
import type { WeeklyPickItem } from '@/interfaces/api/discover/GetWeeklyPicksDto'
import type { PlaylistItem } from '@/interfaces/api/playlists/GetPlaylistsDto'

interface WeeklyPicksListProps {
  picks: WeeklyPickItem[]
  playlists: PlaylistItem[]
  pendingTrackId: number | null
  onRequest: (_deezerTrackId: number, _playlistId: string | null) => void
}

export default function WeeklyPicksList({ picks, playlists, pendingTrackId, onRequest }: WeeklyPicksListProps) {
  return (
    <Stack gap={0}>
      {picks.map((pick, index) => (
        <Fragment key={`${pick.artistName}-${pick.title}`}>
          {index > 0 && <Divider />}
          <WeeklyPickRow pick={pick} playlists={playlists} pendingTrackId={pendingTrackId} onRequest={onRequest} />
        </Fragment>
      ))}
    </Stack>
  )
}
