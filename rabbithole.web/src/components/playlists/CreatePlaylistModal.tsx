import { Button, Group, Modal, TextInput } from '@mantine/core'
import { useState } from 'react'

interface CreatePlaylistModalProps {
  opened: boolean
  isPending: boolean
  onClose: () => void
  onCreate: (_name: string) => void
}

export default function CreatePlaylistModal({ opened, isPending, onClose, onCreate }: CreatePlaylistModalProps) {
  const [name, setName] = useState('')

  const submit = () => {
    if (name.trim().length > 0) {
      onCreate(name.trim())
      setName('')
    }
  }

  return (
    <Modal opened={opened} onClose={onClose} title="New playlist" centered>
      <TextInput
        label="Name"
        placeholder="Late night finds"
        value={name}
        onChange={event => setName(event.currentTarget.value)}
        onKeyDown={event => {
          if (event.key === 'Enter') {
            submit()
          }
        }}
        data-autofocus
      />
      <Group justify="flex-end" mt="md">
        <Button variant="default" onClick={onClose}>Cancel</Button>
        <Button onClick={submit} loading={isPending} disabled={name.trim().length === 0}>Create in Navidrome</Button>
      </Group>
    </Modal>
  )
}
