import { Stack, Text, ThemeIcon } from '@mantine/core'
import { IconMoodEmpty } from '@tabler/icons-react'
import { type ReactNode } from 'react'

interface EmptyStateProps {
  title: string
  description?: string
  action?: ReactNode
}

export default function EmptyState({ title, description, action }: EmptyStateProps) {
  return (
    <Stack align="center" gap="xs" py="xl">
      <ThemeIcon size="xl" radius="xl" variant="light" color="gray">
        <IconMoodEmpty />
      </ThemeIcon>
      <Text fw={500}>{title}</Text>
      {description && <Text size="sm" c="dimmed" ta="center" maw={420}>{description}</Text>}
      {action}
    </Stack>
  )
}
