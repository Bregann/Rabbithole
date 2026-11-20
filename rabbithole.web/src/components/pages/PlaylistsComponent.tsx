import { Button, Group, SimpleGrid, Stack, Title } from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import { IconCheck, IconPlus, IconX } from '@tabler/icons-react'
import { useQuery } from '@tanstack/react-query'
import { useNavigate } from 'react-router'
import EmptyState from '@/components/common/EmptyState'
import PageLoader from '@/components/common/PageLoader'
import QueryErrorAlert from '@/components/common/QueryErrorAlert'
import CreatePlaylistModal from '@/components/playlists/CreatePlaylistModal'
import PlaylistCard from '@/components/playlists/PlaylistCard'
import { doQueryGet } from '@/helpers/apiClient'
import { useMutationPost } from '@/helpers/mutations/useMutationPost'
import notificationHelper from '@/helpers/notificationHelper'
import { QueryKeys } from '@/helpers/QueryKeys'
import type { CreatePlaylistRequest } from '@/interfaces/api/playlists/CreatePlaylistRequest'
import type { GetPlaylistsDto } from '@/interfaces/api/playlists/GetPlaylistsDto'

export default function PlaylistsComponent() {
  const navigate = useNavigate()
  const [createOpened, { open: openCreate, close: closeCreate }] = useDisclosure(false)

  const { data, isLoading, error } = useQuery({
    queryKey: [QueryKeys.Playlists],
    queryFn: async () => await doQueryGet<GetPlaylistsDto>('/api/playlists/GetPlaylists')
  })

  const { mutate: createPlaylist, isPending: isCreating } = useMutationPost<CreatePlaylistRequest, string>({
    url: '/api/playlists/CreatePlaylist',
    queryKey: [QueryKeys.Playlists],
    invalidateQuery: true,
    onSuccess: playlistId => {
      closeCreate()
      notificationHelper.showSuccessNotification('Created', 'New playlist is in Navidrome', 3000, <IconCheck />)
      if (playlistId !== undefined) {
        void navigate(`/playlists/${playlistId}`)
      }
    },
    onError: err => notificationHelper.showErrorNotification('Couldn\'t create playlist', err.message, 4000, <IconX />)
  })

  return (
    <Stack gap="lg">
      <Group justify="space-between">
        <Title order={1}>Playlists</Title>
        <Button leftSection={<IconPlus size={16} />} onClick={openCreate}>New playlist</Button>
      </Group>

      {isLoading && <PageLoader />}
      {error !== null && <QueryErrorAlert title="Couldn't load your playlists" error={error} />}
      {data !== undefined && data.playlists.length === 0 && (
        <EmptyState title="No playlists yet" description="Make one here and it'll show up in Navidrome too." />
      )}
      {data !== undefined && data.playlists.length > 0 && (
        <SimpleGrid cols={{ base: 2, xs: 3, md: 4, lg: 5 }} spacing="md">
          {data.playlists.map(playlist => <PlaylistCard key={playlist.id} playlist={playlist} />)}
        </SimpleGrid>
      )}

      <CreatePlaylistModal opened={createOpened} isPending={isCreating} onClose={closeCreate} onCreate={name => createPlaylist({ name })} />
    </Stack>
  )
}
