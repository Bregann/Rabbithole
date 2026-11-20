import { Avatar, Menu, UnstyledButton } from '@mantine/core'
import { IconLogout, IconSettings } from '@tabler/icons-react'
import { useNavigate } from 'react-router'
import { useAuth } from '@/context/authContext'

export default function UserMenu() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  if (user === null) {
    return null
  }

  const signOut = async () => {
    await logout()
    void navigate('/login')
  }

  return (
    <Menu position="bottom-end" width={180}>
      <Menu.Target>
        <UnstyledButton aria-label="Account">
          <Avatar color="violet" radius="xl">{user.username.slice(0, 2).toUpperCase()}</Avatar>
        </UnstyledButton>
      </Menu.Target>
      <Menu.Dropdown>
        <Menu.Label>{user.username}{user.isAdmin ? ' (admin)' : ''}</Menu.Label>
        <Menu.Item leftSection={<IconSettings size={14} />} onClick={() => void navigate('/settings')}>Settings</Menu.Item>
        <Menu.Item leftSection={<IconLogout size={14} />} onClick={() => void signOut()}>Sign out</Menu.Item>
      </Menu.Dropdown>
    </Menu>
  )
}
