import { NavLink, Stack } from '@mantine/core'
import { IconCompass, IconListDetails, IconPlaylist, IconSearch, IconSettings } from '@tabler/icons-react'
import { Link, useLocation } from 'react-router'

const links = [
  { to: '/', label: 'Discover', icon: IconCompass },
  { to: '/search', label: 'Search', icon: IconSearch },
  { to: '/playlists', label: 'Playlists', icon: IconPlaylist },
  { to: '/requests', label: 'Requests', icon: IconListDetails },
  { to: '/settings', label: 'Settings', icon: IconSettings },
]

interface NavLinksProps {
  onNavigate: () => void
}

export default function NavLinks({ onNavigate }: NavLinksProps) {
  const location = useLocation()

  const isActive = (to: string) => to === '/' ? location.pathname === '/' : location.pathname.startsWith(to)

  return (
    <Stack gap={4}>
      {links.map(link => (
        <NavLink
          key={link.to}
          component={Link}
          to={link.to}
          label={link.label}
          leftSection={<link.icon size={18} />}
          active={isActive(link.to)}
          onClick={onNavigate}
        />
      ))}
    </Stack>
  )
}
