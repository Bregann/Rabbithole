import { Button, Card, Group, Stack, Text, Title } from '@mantine/core'
import type { BlockedArtistItem } from '@/interfaces/api/feedback/GetBlockedArtistsDto'

interface BlockedArtistsCardProps {
  artists: BlockedArtistItem[]
  unblockingId: number | null
  onUnblock: (_id: number) => void
}

export default function BlockedArtistsCard({ artists, unblockingId, onUnblock }: BlockedArtistsCardProps) {
  return (
    <Card withBorder padding="lg">
      <Title order={3} mb="xs">Hidden artists</Title>
      {artists.length === 0 && <Text size="sm" c="dimmed">Nobody hidden. Use "Not for me" on an artist to stop seeing them.</Text>}
      <Stack gap="xs">
        {artists.map(artist => (
          <Group key={artist.id} justify="space-between">
            <Text size="sm">{artist.artistName}</Text>
            <Button size="xs" variant="subtle" loading={unblockingId === artist.id} onClick={() => onUnblock(artist.id)}>Show again</Button>
          </Group>
        ))}
      </Stack>
    </Card>
  )
}
