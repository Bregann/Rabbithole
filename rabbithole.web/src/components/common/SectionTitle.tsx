import { Group, Text, Title } from '@mantine/core'
import { type ReactNode } from 'react'

interface SectionTitleProps {
  title: string
  subtitle?: string
  action?: ReactNode
}

export default function SectionTitle({ title, subtitle, action }: SectionTitleProps) {
  return (
    <Group justify="space-between" align="flex-end" mb="sm">
      <div>
        <Title order={3}>{title}</Title>
        {subtitle && <Text size="sm" c="dimmed">{subtitle}</Text>}
      </div>
      {action}
    </Group>
  )
}
