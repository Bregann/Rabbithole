import { Card, Divider, Group, Stack, Text, Title } from '@mantine/core'
import { useQuery } from '@tanstack/react-query'
import { Fragment } from 'react'
import EmptyState from '@/components/common/EmptyState'
import PageLoader from '@/components/common/PageLoader'
import QueryErrorAlert from '@/components/common/QueryErrorAlert'
import RequestRow from '@/components/requests/RequestRow'
import { doQueryGet } from '@/helpers/apiClient'
import { QueryKeys } from '@/helpers/QueryKeys'
import type { GetRequestsDto } from '@/interfaces/api/requests/GetRequestsDto'

export default function RequestsComponent() {
  const { data, isLoading, error } = useQuery({
    queryKey: [QueryKeys.Requests],
    queryFn: async () => await doQueryGet<GetRequestsDto>('/api/requests/GetMyRequests'),
    refetchInterval: 60_000
  })

  return (
    <Stack gap="lg">
      <Group justify="space-between" align="flex-end">
        <Title order={1}>Requests</Title>
        {data?.remainingThisWeek !== null && data?.remainingThisWeek !== undefined && (
          <Text size="sm" c="dimmed">{data.remainingThisWeek} of {data.weeklyLimit} left this week</Text>
        )}
      </Group>

      {isLoading && <PageLoader />}
      {error !== null && <QueryErrorAlert title="Couldn't load your requests" error={error} />}
      {data !== undefined && data.requests.length === 0 && (
        <EmptyState title="Nothing requested yet" description="Request albums from any artist page, or use “Get it” on a playlist suggestion." />
      )}
      {data !== undefined && data.requests.length > 0 && (
        <Card withBorder padding="sm">
          <Stack gap={0}>
            {data.requests.map((request, index) => (
              <Fragment key={request.id}>
                {index > 0 && <Divider />}
                <RequestRow request={request} />
              </Fragment>
            ))}
          </Stack>
        </Card>
      )}
    </Stack>
  )
}
