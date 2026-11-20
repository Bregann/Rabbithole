import { ActionIcon, Menu, Tooltip } from '@mantine/core'
import { IconDownload, IconDots, IconPlaylistAdd } from '@tabler/icons-react'
import type { PlaylistItem } from '@/interfaces/api/playlists/GetPlaylistsDto'
import type { TrackItem } from '@/interfaces/api/shared/TrackItem'

interface TrackActionsMenuProps {
  track: TrackItem
  playlists: PlaylistItem[]
  isPending: boolean
  onRequest: (_deezerTrackId: number, _playlistId: string | null) => void
}

// Songs from Deezer can't go into a Navidrome playlist until they're in the library,
// so "add to playlist" requests the album and adds the song once it's downloaded.
export default function TrackActionsMenu({ track, playlists, isPending, onRequest }: TrackActionsMenuProps) {
  return (
    <Menu position="bottom-end" width={260} withinPortal>
      <Menu.Target>
        <Tooltip label="Request or add to a playlist">
          <ActionIcon variant="subtle" color="gray" loading={isPending} aria-label="Track actions">
            <IconDots size={18} />
          </ActionIcon>
        </Tooltip>
      </Menu.Target>
      <Menu.Dropdown>
        <Menu.Item leftSection={<IconDownload size={14} />} onClick={() => onRequest(track.deezerTrackId, null)}>
          Request the album
        </Menu.Item>
        {playlists.length > 0 && <Menu.Label>Request and add to playlist</Menu.Label>}
        {playlists.map(playlist => (
          <Menu.Item
            key={playlist.id}
            leftSection={<IconPlaylistAdd size={14} />}
            onClick={() => onRequest(track.deezerTrackId, playlist.id)}
          >
            {playlist.name}
          </Menu.Item>
        ))}
      </Menu.Dropdown>
    </Menu>
  )
}
