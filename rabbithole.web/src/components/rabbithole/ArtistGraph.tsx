import ArtistGraphNode from '@/components/rabbithole/ArtistGraphNode'
import type { ArtistSummaryItem } from '@/interfaces/api/shared/ArtistSummaryItem'
import classes from '@/css/rabbithole.module.css'

interface ArtistGraphProps {
  centre: ArtistSummaryItem
  similar: ArtistSummaryItem[]
  onSelect: (_artist: ArtistSummaryItem) => void
}

const WIDTH = 900
const HEIGHT = 620
const CENTRE_RADIUS = 70
const NODE_RADIUS = 42
const ORBIT_RADIUS = 240

// Radial layout: the current artist in the middle, similar artists evenly spaced around it
export default function ArtistGraph({ centre, similar, onSelect }: ArtistGraphProps) {
  const cx = WIDTH / 2
  const cy = HEIGHT / 2 - 10

  const positions = similar.map((artist, index) => {
    const angle = (index / similar.length) * Math.PI * 2 - Math.PI / 2
    return { artist, x: cx + Math.cos(angle) * ORBIT_RADIUS, y: cy + Math.sin(angle) * ORBIT_RADIUS * 0.85 }
  })

  return (
    <svg viewBox={`0 0 ${WIDTH} ${HEIGHT}`} width="100%" style={{ maxHeight: '70vh' }} role="img" aria-label={`Artists similar to ${centre.name}`}>
      {positions.map(p => (
        <line key={`edge-${p.artist.deezerArtistId}`} x1={cx} y1={cy} x2={p.x} y2={p.y} className={classes.edge} />
      ))}
      {positions.map(p => (
        <ArtistGraphNode key={p.artist.deezerArtistId} artist={p.artist} x={p.x} y={p.y} radius={NODE_RADIUS} onSelect={onSelect} />
      ))}
      <ArtistGraphNode artist={centre} x={cx} y={cy} radius={CENTRE_RADIUS} isCentre />
    </svg>
  )
}
