import { TextInput } from '@mantine/core'
import { IconSearch } from '@tabler/icons-react'
import { useState } from 'react'
import { useNavigate } from 'react-router'

export default function HeaderSearch() {
  const [query, setQuery] = useState('')
  const navigate = useNavigate()

  const submit = () => {
    if (query.trim().length > 0) {
      void navigate(`/search?q=${encodeURIComponent(query.trim())}`)
    }
  }

  return (
    <TextInput
      placeholder="Search for an artist"
      leftSection={<IconSearch size={16} />}
      value={query}
      onChange={event => setQuery(event.currentTarget.value)}
      onKeyDown={event => {
        if (event.key === 'Enter') {
          submit()
        }
      }}
      w={{ base: 160, sm: 280 }}
    />
  )
}
