import { Text } from '@mantine/core'

interface SeedSummaryProps {
  source: string
  seeds: string[]
}

export default function SeedSummary({ source, seeds }: SeedSummaryProps) {
  if (seeds.length === 0) {
    return null
  }

  const from = source === 'ListenBrainz' ? 'your ListenBrainz listening this month' : 'your most played albums in Navidrome'

  return (
    <Text size="sm" c="dimmed">
      Built from {from}: {seeds.slice(0, 6).join(', ')}{seeds.length > 6 ? '…' : ''}
    </Text>
  )
}
