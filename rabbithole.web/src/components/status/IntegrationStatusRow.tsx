import { Group, Stack, Text, ThemeIcon } from '@mantine/core'
import { IconCheck, IconX } from '@tabler/icons-react'
import { getIntegrationDescription, getIntegrationLabel, getIntegrationSettings } from '@/helpers/statusHelper'
import type { IntegrationStatusItem } from '@/interfaces/api/status/GetStatusDto'

interface IntegrationStatusRowProps {
  item: IntegrationStatusItem
}

export default function IntegrationStatusRow({ item }: IntegrationStatusRowProps) {
  return (
    <Group justify="space-between" wrap="nowrap">
      <Stack gap={2}>
        <Text fw={500}>{getIntegrationLabel(item.integration)}</Text>
        <Text size="sm" c="dimmed">{getIntegrationDescription(item.integration)}</Text>
        {!item.isConfigured && (
          <Text size="xs" c="dimmed">Set: {getIntegrationSettings(item.integration)}</Text>
        )}
      </Stack>
      <ThemeIcon variant="light" color={item.isConfigured ? 'green' : 'red'} radius="xl">
        {item.isConfigured ? <IconCheck size={16} /> : <IconX size={16} />}
      </ThemeIcon>
    </Group>
  )
}
