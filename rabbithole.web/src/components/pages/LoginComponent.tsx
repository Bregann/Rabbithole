import { Alert, Button, Card, Center, PasswordInput, Stack, Text, TextInput, Title } from '@mantine/core'
import { IconAlertTriangle } from '@tabler/icons-react'
import { useState } from 'react'
import { Navigate, useLocation } from 'react-router'
import { useAuth } from '@/context/authContext'

export default function LoginComponent() {
  const { user, login } = useAuth()
  const location = useLocation()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const redirectTo = (location.state as { from?: string } | null)?.from ?? '/'

  // Signing in sets the user, which sends them on to where they were going
  if (user !== null) {
    return <Navigate to={redirectTo} replace />
  }

  const submit = async () => {
    setIsSubmitting(true)
    setError(await login({ username, password }))
    setIsSubmitting(false)
  }

  return (
    <Center mih="100vh" p="md">
      <Card withBorder padding="xl" w={380}>
        <Stack>
          <Stack gap={4} align="center">
            <img src="/favicon.svg" alt="" width={56} height={56} />
            <Title order={2}>Rabbithole</Title>
            <Text size="sm" c="dimmed" ta="center">Sign in with your Navidrome account</Text>
          </Stack>

          {error && <Alert color="red" variant="light" icon={<IconAlertTriangle />}>{error}</Alert>}

          <form onSubmit={event => {
            event.preventDefault()
            void submit()
          }}>
            <Stack>
              <TextInput label="Username" value={username} onChange={event => setUsername(event.currentTarget.value)} autoComplete="username" data-autofocus />
              <PasswordInput label="Password" value={password} onChange={event => setPassword(event.currentTarget.value)} autoComplete="current-password" />
              <Button type="submit" loading={isSubmitting} fullWidth>Sign in</Button>
            </Stack>
          </form>
        </Stack>
      </Card>
    </Center>
  )
}
