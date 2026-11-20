import { ActionIcon, Divider, Stack, Tooltip } from '@mantine/core'
import { IconTrash } from '@tabler/icons-react'
import { Fragment } from 'react'
import EmptyState from '@/components/common/EmptyState'
import SongRow from '@/components/playlists/SongRow'
import type { LibrarySongItem } from '@/interfaces/api/shared/LibrarySongItem'

interface PlaylistSongListProps {
  songs: LibrarySongItem[]
  canEdit: boolean
  removingIndex: number | null
  onRemove: (_index: number) => void
}

export default function PlaylistSongList({ songs, canEdit, removingIndex, onRemove }: PlaylistSongListProps) {
  if (songs.length === 0) {
    return <EmptyState title="This playlist is empty" description="Add songs from your library, or pick some suggestions." />
  }

  return (
    <Stack gap={0}>
      {songs.map((song, index) => (
        <Fragment key={`${song.songId}-${index}`}>
          {index > 0 && <Divider />}
          <SongRow
            song={song}
            actions={canEdit && (
              <Tooltip label="Remove from playlist">
                <ActionIcon variant="subtle" color="gray" loading={removingIndex === index} onClick={() => onRemove(index)} aria-label="Remove">
                  <IconTrash size={16} />
                </ActionIcon>
              </Tooltip>
            )}
          />
        </Fragment>
      ))}
    </Stack>
  )
}
