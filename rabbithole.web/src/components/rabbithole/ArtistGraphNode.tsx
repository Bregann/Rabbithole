import { getLibraryStateColour } from '@/helpers/libraryStateHelper'
import type { ArtistSummaryItem } from '@/interfaces/api/shared/ArtistSummaryItem'
import classes from '@/css/rabbithole.module.css'

interface ArtistGraphNodeProps {
  artist: ArtistSummaryItem
  x: number
  y: number
  radius: number
  isCentre?: boolean
  onSelect?: (_artist: ArtistSummaryItem) => void
}

const truncate = (text: string, length: number) => text.length > length ? `${text.slice(0, length - 1)}…` : text

export default function ArtistGraphNode({ artist, x, y, radius, isCentre = false, onSelect }: ArtistGraphNodeProps) {
  const clipId = `clip-${artist.deezerArtistId}`
  const ringColour = `var(--mantine-color-${getLibraryStateColour(artist.libraryState)}-6)`

  return (
    <g
      className={isCentre ? undefined : classes.node}
      onClick={() => onSelect?.(artist)}
      role={onSelect ? 'button' : undefined}
      aria-label={artist.name}
    >
      <defs>
        <clipPath id={clipId}>
          <circle cx={x} cy={y} r={radius} />
        </clipPath>
      </defs>
      <circle cx={x} cy={y} r={radius + 3} fill={ringColour} />
      <circle cx={x} cy={y} r={radius} fill="var(--mantine-color-dark-5)" />
      {artist.imageUrl && (
        <image
          href={artist.imageUrl}
          x={x - radius}
          y={y - radius}
          width={radius * 2}
          height={radius * 2}
          clipPath={`url(#${clipId})`}
          preserveAspectRatio="xMidYMid slice"
        />
      )}
      <text
        x={x}
        y={y + radius + (isCentre ? 24 : 18)}
        textAnchor="middle"
        className={isCentre ? classes.centreLabel : classes.label}
      >
        {truncate(artist.name, isCentre ? 28 : 18)}
      </text>
    </g>
  )
}
