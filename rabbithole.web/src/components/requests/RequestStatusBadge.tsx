import { Badge } from '@mantine/core'
import { getRequestStatusColour, getRequestStatusLabel } from '@/helpers/requestStatusHelper'
import type { RequestStatus } from '@/interfaces/api/requests/RequestStatus'

interface RequestStatusBadgeProps {
  status: RequestStatus
}

export default function RequestStatusBadge({ status }: RequestStatusBadgeProps) {
  return <Badge variant="light" color={getRequestStatusColour(status)}>{getRequestStatusLabel(status)}</Badge>
}
