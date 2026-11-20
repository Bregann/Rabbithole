import { Button, Card, Group, Stack, Text, TextInput, Title } from '@mantine/core'
import { useEffect, useState } from 'react'
import type { GetMySettingsDto } from '@/interfaces/api/settings/GetMySettingsDto'

interface ProfileSettingsCardProps {
  settings: GetMySettingsDto
  isSaving: boolean
  onSave: (_listenBrainzUsername: string | null) => void
}

export default function ProfileSettingsCard({ settings, isSaving, onSave }: ProfileSettingsCardProps) {
  const [listenBrainzUsername, setListenBrainzUsername] = useState(settings.listenBrainzUsername ?? '')

  useEffect(() => {
    setListenBrainzUsername(settings.listenBrainzUsername ?? '')
  }, [settings.listenBrainzUsername])

  return (
    <Card withBorder padding="lg">
      <Title order={3} mb="xs">You</Title>
      <Text size="sm" c="dimmed" mb="md">Signed in to Navidrome as <b>{settings.username}</b>{settings.isAdmin ? ' (admin)' : ''}.</Text>
      <Stack gap="xs">
        <TextInput
          label="ListenBrainz username"
          description="Rabbithole uses your listening history and Weekly Exploration playlist from ListenBrainz. Link Navidrome to ListenBrainz in Navidrome's own settings so your plays get there."
          placeholder="your-listenbrainz-name"
          value={listenBrainzUsername}
          onChange={event => setListenBrainzUsername(event.currentTarget.value)}
        />
        <Group justify="flex-end">
          <Button loading={isSaving} onClick={() => onSave(listenBrainzUsername.trim() || null)}>Save</Button>
        </Group>
      </Stack>
    </Card>
  )
}
