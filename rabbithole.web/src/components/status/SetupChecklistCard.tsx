import { Badge, Card, Divider, Group, Stack, Text, Title } from '@mantine/core'
import { Fragment } from 'react'
import IntegrationStatusRow from '@/components/status/IntegrationStatusRow'
import type { GetStatusDto } from '@/interfaces/api/status/GetStatusDto'

interface SetupChecklistCardProps {
  status: GetStatusDto
}

export default function SetupChecklistCard({ status }: SetupChecklistCardProps) {
  return (
    <Card withBorder padding="lg">
      <Group justify="space-between" mb="md">
        <Title order={3}>Setup</Title>
        <Badge color={status.isFullyConfigured ? 'green' : 'yellow'} variant="light">
          {status.isFullyConfigured ? 'Ready' : 'Needs setup'}
        </Badge>
      </Group>

      <Stack gap="sm">
        {status.integrations.map((item, index) => (
          <Fragment key={item.integration}>
            {index > 0 && <Divider />}
            <IntegrationStatusRow item={item} />
          </Fragment>
        ))}
      </Stack>

      <Text size="xs" c="dimmed" mt="md">Rabbithole v{status.version}</Text>
    </Card>
  )
}
