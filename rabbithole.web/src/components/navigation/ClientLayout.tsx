import { AppShell, Burger, Container, Group } from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import { Outlet } from 'react-router'
import AppLogo from '@/components/navigation/AppLogo'
import HeaderSearch from '@/components/navigation/HeaderSearch'
import NavLinks from '@/components/navigation/NavLinks'
import UserMenu from '@/components/navigation/UserMenu'
import PlayerBar from '@/components/player/PlayerBar'
import { usePlayer } from '@/context/playerContext'

export default function ClientLayout() {
  const [navOpened, { toggle: toggleNav, close: closeNav }] = useDisclosure(false)
  const { nowPlaying } = usePlayer()

  return (
    <AppShell
      header={{ height: 60 }}
      navbar={{ width: 220, breakpoint: 'sm', collapsed: { mobile: !navOpened } }}
      footer={{ height: 72, collapsed: nowPlaying === null }}
      padding="md"
    >
      <AppShell.Header>
        <Group h="100%" px="md" justify="space-between" wrap="nowrap">
          <Group gap="sm" wrap="nowrap">
            <Burger opened={navOpened} onClick={toggleNav} hiddenFrom="sm" size="sm" />
            <AppLogo />
          </Group>
          <Group gap="sm" wrap="nowrap">
            <HeaderSearch />
            <UserMenu />
          </Group>
        </Group>
      </AppShell.Header>

      <AppShell.Navbar p="sm">
        <NavLinks onNavigate={closeNav} />
      </AppShell.Navbar>

      <AppShell.Main>
        <Container size="xl" px={0}>
          <Outlet />
        </Container>
      </AppShell.Main>

      <AppShell.Footer>
        <PlayerBar />
      </AppShell.Footer>
    </AppShell>
  )
}
