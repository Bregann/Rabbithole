import { Group, Text } from '@mantine/core'
import { Link } from 'react-router'

export default function AppLogo() {
  return (
    <Link to="/" style={{ textDecoration: 'none', color: 'inherit' }}>
      <Group gap="xs">
        <img src="/favicon.svg" alt="" width={28} height={28} />
        <Text fw={700} size="lg">Rabbithole</Text>
      </Group>
    </Link>
  )
}
