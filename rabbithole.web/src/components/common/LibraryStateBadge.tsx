import { Badge } from '@mantine/core'
import { getLibraryStateColour, getLibraryStateLabel } from '@/helpers/libraryStateHelper'
import type { LibraryState } from '@/interfaces/api/status/LibraryState'

interface LibraryStateBadgeProps {
  state: LibraryState
  size?: 'xs' | 'sm' | 'md'
}

export default function LibraryStateBadge({ state, size = 'xs' }: LibraryStateBadgeProps) {
  return (
    <Badge size={size} variant="light" color={getLibraryStateColour(state)}>
      {getLibraryStateLabel(state)}
    </Badge>
  )
}
