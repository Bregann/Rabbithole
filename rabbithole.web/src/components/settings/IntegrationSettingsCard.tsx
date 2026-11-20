import { Button, Card, Group, PasswordInput, Stack, Text, TextInput, Title } from '@mantine/core'
import { useEffect, useState } from 'react'
import type { IntegrationSettingItem } from '@/interfaces/api/settings/GetIntegrationSettingsDto'
import type { UpdateIntegrationSettingItem } from '@/interfaces/api/settings/UpdateIntegrationSettingsRequest'

interface IntegrationSettingsCardProps {
  settings: IntegrationSettingItem[]
  isSaving: boolean
  onSave: (_settings: UpdateIntegrationSettingItem[]) => void
}

const labels: Record<string, string> = {
  NavidromeUrl: 'Navidrome URL',
  ListenBrainzApiUrl: 'ListenBrainz API URL',
  LidarrUrl: 'Lidarr URL',
  LidarrApiKey: 'Lidarr API key',
  AudioMuseUrl: 'AudioMuse URL',
  AudioMuseApiToken: 'AudioMuse API token',
  WeeklyRequestCap: 'Requests per user per week',
}

export default function IntegrationSettingsCard({ settings, isSaving, onSave }: IntegrationSettingsCardProps) {
  const [values, setValues] = useState<Record<string, string>>({})

  useEffect(() => {
    setValues(Object.fromEntries(settings.map(s => [s.key, s.value])))
  }, [settings])

  const setValue = (key: string, value: string) => setValues(current => ({ ...current, [key]: value }))

  return (
    <Card withBorder padding="lg">
      <Title order={3} mb="xs">Connections</Title>
      <Text size="sm" c="dimmed" mb="md">
        Admins only. A matching environment variable on the container overrides these on restart.
      </Text>
      <Stack gap="sm">
        {settings.map(setting => setting.isSecret
          ? (
            <PasswordInput
              key={setting.key}
              label={labels[setting.key] ?? setting.key}
              placeholder={setting.hasValue ? 'Saved. Leave blank to keep it' : 'Not set'}
              value={values[setting.key] ?? ''}
              onChange={event => setValue(setting.key, event.currentTarget.value)}
            />
          )
          : (
            <TextInput
              key={setting.key}
              label={labels[setting.key] ?? setting.key}
              value={values[setting.key] ?? ''}
              onChange={event => setValue(setting.key, event.currentTarget.value)}
            />
          ))}
        <Group justify="flex-end">
          <Button loading={isSaving} onClick={() => onSave(Object.entries(values).map(([key, value]) => ({ key, value })))}>
            Save connections
          </Button>
        </Group>
      </Stack>
    </Card>
  )
}
