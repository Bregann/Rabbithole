import { ActionIcon, Divider, Modal, ScrollArea, Stack, TextInput, Tooltip } from '@mantine/core'
import { useDebouncedValue } from '@mantine/hooks'
import { IconPlus, IconSearch } from '@tabler/icons-react'
import { useQuery } from '@tanstack/react-query'
import { Fragment, useState } from 'react'
import EmptyState from '@/components/common/EmptyState'
import PageLoader from '@/components/common/PageLoader'
import SongRow from '@/components/playlists/SongRow'
import { doQueryGet } from '@/helpers/apiClient'
import { QueryKeys } from '@/helpers/QueryKeys'
import type { SearchLibraryDto } from '@/interfaces/api/library/SearchLibraryDto'

interface AddSongsModalProps {
  opened: boolean
  addingSongId: string | null
  onClose: () => void
  onAdd: (_songId: string) => void
}

// Search the Navidrome library and add songs one at a time
export default function AddSongsModal({ opened, addingSongId, onClose, onAdd }: AddSongsModalProps) {
  const [query, setQuery] = useState('')
  const [debouncedQuery] = useDebouncedValue(query.trim(), 350)

  const { data, isFetching } = useQuery({
    queryKey: [QueryKeys.LibrarySearch, debouncedQuery],
    queryFn: async () => await doQueryGet<SearchLibraryDto>(`/api/library/SearchSongs?query=${encodeURIComponent(debouncedQuery)}`),
    enabled: opened && debouncedQuery.length > 1
  })

  const songs = data?.songs ?? []

  return (
    <Modal opened={opened} onClose={onClose} title="Add songs from your library" size="lg">
      <TextInput
        placeholder="Song, artist or album"
        leftSection={<IconSearch size={16} />}
        value={query}
        onChange={event => setQuery(event.currentTarget.value)}
        data-autofocus
      />
      <ScrollArea h={420} mt="md">
        {isFetching && <PageLoader />}
        {!isFetching && debouncedQuery.length > 1 && songs.length === 0 && <EmptyState title="Nothing in your library matches that" />}
        <Stack gap={0}>
          {songs.map((song, index) => (
            <Fragment key={song.songId}>
              {index > 0 && <Divider />}
              <SongRow
                song={song}
                actions={
                  <Tooltip label="Add to playlist">
                    <ActionIcon variant="light" loading={addingSongId === song.songId} onClick={() => onAdd(song.songId)} aria-label="Add">
                      <IconPlus size={16} />
                    </ActionIcon>
                  </Tooltip>
                }
              />
            </Fragment>
          ))}
        </Stack>
      </ScrollArea>
    </Modal>
  )
}
