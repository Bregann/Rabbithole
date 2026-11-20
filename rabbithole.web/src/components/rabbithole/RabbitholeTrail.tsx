import { Anchor, Breadcrumbs, Text } from '@mantine/core'

export interface TrailEntry {
  deezerArtistId: number
  name: string
}

interface RabbitholeTrailProps {
  trail: TrailEntry[]
  onJump: (_index: number) => void
}

export default function RabbitholeTrail({ trail, onJump }: RabbitholeTrailProps) {
  return (
    <Breadcrumbs separator="›" style={{ flexWrap: 'wrap' }}>
      {trail.map((entry, index) => index === trail.length - 1
        ? <Text key={entry.deezerArtistId} fw={600}>{entry.name}</Text>
        : <Anchor key={entry.deezerArtistId} onClick={() => onJump(index)}>{entry.name}</Anchor>)}
    </Breadcrumbs>
  )
}
