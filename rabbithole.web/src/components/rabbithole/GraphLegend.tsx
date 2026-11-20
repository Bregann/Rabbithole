import { Group } from '@mantine/core'
import LibraryStateBadge from '@/components/common/LibraryStateBadge'
import { LibraryState } from '@/interfaces/api/status/LibraryState'

export default function GraphLegend() {
  return (
    <Group gap="xs">
      <LibraryStateBadge state={LibraryState.InLibrary} />
      <LibraryStateBadge state={LibraryState.InLidarr} />
      <LibraryStateBadge state={LibraryState.New} />
    </Group>
  )
}
