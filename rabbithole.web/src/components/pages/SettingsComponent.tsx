import { SimpleGrid, Stack, Title } from '@mantine/core'
import { IconCheck, IconX } from '@tabler/icons-react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import PageLoader from '@/components/common/PageLoader'
import BlockedArtistsCard from '@/components/settings/BlockedArtistsCard'
import IntegrationSettingsCard from '@/components/settings/IntegrationSettingsCard'
import ProfileSettingsCard from '@/components/settings/ProfileSettingsCard'
import SetupChecklistCard from '@/components/status/SetupChecklistCard'
import { useAuth } from '@/context/authContext'
import { doQueryGet } from '@/helpers/apiClient'
import { useMutationDelete } from '@/helpers/mutations/useMutationDelete'
import { useMutationPut } from '@/helpers/mutations/useMutationPut'
import notificationHelper from '@/helpers/notificationHelper'
import { QueryKeys } from '@/helpers/QueryKeys'
import type { GetBlockedArtistsDto } from '@/interfaces/api/feedback/GetBlockedArtistsDto'
import type { GetIntegrationSettingsDto } from '@/interfaces/api/settings/GetIntegrationSettingsDto'
import type { GetMySettingsDto } from '@/interfaces/api/settings/GetMySettingsDto'
import type { UpdateIntegrationSettingsRequest } from '@/interfaces/api/settings/UpdateIntegrationSettingsRequest'
import type { UpdateMySettingsRequest } from '@/interfaces/api/settings/UpdateMySettingsRequest'
import type { GetStatusDto } from '@/interfaces/api/status/GetStatusDto'

export default function SettingsComponent() {
  const { user } = useAuth()
  const queryClient = useQueryClient()
  const isAdmin = user?.isAdmin ?? false

  const mySettings = useQuery({
    queryKey: [QueryKeys.MySettings],
    queryFn: async () => await doQueryGet<GetMySettingsDto>('/api/settings/GetMySettings')
  })

  const blocked = useQuery({
    queryKey: [QueryKeys.BlockedArtists],
    queryFn: async () => await doQueryGet<GetBlockedArtistsDto>('/api/settings/GetBlockedArtists')
  })

  const status = useQuery({
    queryKey: [QueryKeys.Status],
    queryFn: async () => await doQueryGet<GetStatusDto>('/api/status/GetStatus')
  })

  const integrations = useQuery({
    queryKey: [QueryKeys.IntegrationSettings],
    queryFn: async () => await doQueryGet<GetIntegrationSettingsDto>('/api/settings/GetIntegrationSettings'),
    enabled: isAdmin
  })

  const saveProfile = useMutationPut<UpdateMySettingsRequest, void>({
    url: '/api/settings/UpdateMySettings',
    queryKey: [QueryKeys.MySettings],
    invalidateQuery: true,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: [QueryKeys.WeeklyPicks] })
      void queryClient.invalidateQueries({ queryKey: [QueryKeys.DiscoverFeed] })
      void queryClient.invalidateQueries({ queryKey: [QueryKeys.CurrentUser] })
      notificationHelper.showSuccessNotification('Saved', 'Your settings are updated', 3000, <IconCheck />)
    },
    onError: error => notificationHelper.showErrorNotification('Couldn\'t save', error.message, 4000, <IconX />)
  })

  const saveIntegrations = useMutationPut<UpdateIntegrationSettingsRequest, void>({
    url: '/api/settings/UpdateIntegrationSettings',
    queryKey: [QueryKeys.IntegrationSettings],
    invalidateQuery: true,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: [QueryKeys.Status] })
      notificationHelper.showSuccessNotification('Saved', 'Connections updated', 3000, <IconCheck />)
    },
    onError: error => notificationHelper.showErrorNotification('Couldn\'t save', error.message, 4000, <IconX />)
  })

  const unblock = useMutationDelete<number, void>({
    url: blockedArtistId => `/api/feedback/UnblockArtist?blockedArtistId=${blockedArtistId}`,
    queryKey: [QueryKeys.BlockedArtists],
    invalidateQuery: true,
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: [QueryKeys.DiscoverFeed] }),
    onError: error => notificationHelper.showErrorNotification('Couldn\'t update', error.message, 4000, <IconX />)
  })

  if (mySettings.isLoading) {
    return <PageLoader />
  }

  return (
    <Stack gap="lg">
      <Title order={1}>Settings</Title>
      <SimpleGrid cols={{ base: 1, md: 2 }} spacing="lg">
        <Stack gap="lg">
          {mySettings.data !== undefined && (
            <ProfileSettingsCard
              settings={mySettings.data}
              isSaving={saveProfile.isPending}
              onSave={listenBrainzUsername => saveProfile.mutate({ listenBrainzUsername })}
            />
          )}
          <BlockedArtistsCard
            artists={blocked.data?.artists ?? []}
            unblockingId={unblock.isPending ? unblock.variables ?? null : null}
            onUnblock={id => unblock.mutate(id)}
          />
        </Stack>
        <Stack gap="lg">
          {status.data !== undefined && <SetupChecklistCard status={status.data} />}
          {isAdmin && integrations.data !== undefined && (
            <IntegrationSettingsCard
              settings={integrations.data.settings}
              isSaving={saveIntegrations.isPending}
              onSave={settings => saveIntegrations.mutate({ settings })}
            />
          )}
        </Stack>
      </SimpleGrid>
    </Stack>
  )
}
