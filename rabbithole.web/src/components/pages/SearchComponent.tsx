import { Stack, TextInput, Title } from '@mantine/core'
import { useDebouncedValue } from '@mantine/hooks'
import { IconSearch } from '@tabler/icons-react'
import { useQuery } from '@tanstack/react-query'
import { useSearchParams } from 'react-router'
import ArtistGrid from '@/components/artists/ArtistGrid'
import EmptyState from '@/components/common/EmptyState'
import PageLoader from '@/components/common/PageLoader'
import QueryErrorAlert from '@/components/common/QueryErrorAlert'
import { doQueryGet } from '@/helpers/apiClient'
import { QueryKeys } from '@/helpers/QueryKeys'
import type { SearchArtistsDto } from '@/interfaces/api/artists/SearchArtistsDto'

export default function SearchComponent() {
  const [searchParams, setSearchParams] = useSearchParams()
  const query = searchParams.get('q') ?? ''
  const [debouncedQuery] = useDebouncedValue(query.trim(), 350)

  const { data, isLoading, error } = useQuery({
    queryKey: [QueryKeys.ArtistSearch, debouncedQuery],
    queryFn: async () => await doQueryGet<SearchArtistsDto>(`/api/artists/SearchArtists?query=${encodeURIComponent(debouncedQuery)}`),
    enabled: debouncedQuery.length > 0
  })

  return (
    <Stack gap="lg">
      <Title order={1}>Search</Title>
      <TextInput
        size="md"
        placeholder="Find an artist to start from"
        leftSection={<IconSearch size={18} />}
        value={query}
        onChange={event => setSearchParams(event.currentTarget.value ? { q: event.currentTarget.value } : {}, { replace: true })}
        data-autofocus
      />
      {isLoading && <PageLoader />}
      {error !== null && <QueryErrorAlert title="Search failed" error={error} />}
      {data !== undefined && data.artists.length === 0 && <EmptyState title="No artists found" />}
      {data !== undefined && data.artists.length > 0 && (
        <ArtistGrid entries={data.artists.map(artist => ({ artist }))} />
      )}
    </Stack>
  )
}
