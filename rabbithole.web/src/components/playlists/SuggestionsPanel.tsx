import { ActionIcon, Badge, Button, Divider, Stack, Tabs, Text, Tooltip } from '@mantine/core'
import { IconPlus, IconPlaylistAdd } from '@tabler/icons-react'
import { Fragment } from 'react'
import TrackRow from '@/components/artists/TrackRow'
import EmptyState from '@/components/common/EmptyState'
import LibraryStateBadge from '@/components/common/LibraryStateBadge'
import SongRow from '@/components/playlists/SongRow'
import { formatBecauseOf } from '@/helpers/formatHelper'
import type { GetPlaylistSuggestionsDto } from '@/interfaces/api/playlists/GetPlaylistSuggestionsDto'

interface SuggestionsPanelProps {
  suggestions: GetPlaylistSuggestionsDto
  canEdit: boolean
  addingSongId: string | null
  pendingTrackId: number | null
  onAddSong: (_songId: string) => void
  onRequestTrack: (_deezerTrackId: number) => void
}

export default function SuggestionsPanel({ suggestions, canEdit, addingSongId, pendingTrackId, onAddSong, onRequestTrack }: SuggestionsPanelProps) {
  return (
    <Tabs defaultValue="library">
      <Tabs.List>
        <Tabs.Tab value="library">
          From your library {suggestions.usedAudioMuse && <Badge size="xs" ml={4} variant="light">AudioMuse</Badge>}
        </Tabs.Tab>
        <Tabs.Tab value="new">New to you</Tabs.Tab>
      </Tabs.List>

      <Tabs.Panel value="library" pt="sm">
        {suggestions.fromYourLibrary.length === 0 && <EmptyState title="No library suggestions yet" description="Add a few more songs and try again." />}
        <Stack gap={0}>
          {suggestions.fromYourLibrary.map((song, index) => (
            <Fragment key={song.songId}>
              {index > 0 && <Divider />}
              <SongRow
                song={song}
                actions={canEdit && (
                  <Tooltip label="Add to this playlist">
                    <ActionIcon variant="light" loading={addingSongId === song.songId} onClick={() => onAddSong(song.songId)} aria-label="Add">
                      <IconPlus size={16} />
                    </ActionIcon>
                  </Tooltip>
                )}
              />
            </Fragment>
          ))}
        </Stack>
      </Tabs.Panel>

      <Tabs.Panel value="new" pt="sm">
        {suggestions.newToYou.length === 0 && <EmptyState title="No new suggestions" description="Everything similar is already in your library." />}
        <Stack gap={0}>
          {suggestions.newToYou.map((suggestion, index) => (
            <Fragment key={suggestion.track.deezerTrackId}>
              {index > 0 && <Divider />}
              <TrackRow
                track={suggestion.track}
                showArtist
                extra={
                  <Stack gap={2} mt={2}>
                    <LibraryStateBadge state={suggestion.libraryState} />
                    <Text size="xs" c="dimmed">{formatBecauseOf(suggestion.becauseOf).replace('you listen to', 'this playlist has')}</Text>
                  </Stack>
                }
                actions={canEdit && (
                  <Tooltip label="Request the album and add this song when it arrives">
                    <Button
                      size="xs"
                      variant="light"
                      leftSection={<IconPlaylistAdd size={14} />}
                      loading={pendingTrackId === suggestion.track.deezerTrackId}
                      onClick={() => onRequestTrack(suggestion.track.deezerTrackId)}
                    >
                      Get it
                    </Button>
                  </Tooltip>
                )}
              />
            </Fragment>
          ))}
        </Stack>
      </Tabs.Panel>
    </Tabs>
  )
}
