import { Button, Stack, Text, Title } from '@mantine/core'
import { Link } from 'react-router'

export default function NotFoundComponent() {
  return (
    <Stack align="flex-start">
      <Title order={2}>Too far down</Title>
      <Text c="dimmed">This page doesn&apos;t exist.</Text>
      <Button component={Link} to="/" variant="light">Back to the top</Button>
    </Stack>
  )
}
