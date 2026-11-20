import { Alert } from '@mantine/core'
import { IconAlertTriangle } from '@tabler/icons-react'

interface QueryErrorAlertProps {
  title: string
  error: Error
}

export default function QueryErrorAlert({ title, error }: QueryErrorAlertProps) {
  return (
    <Alert color="red" variant="light" title={title} icon={<IconAlertTriangle />}>
      {error.message}
    </Alert>
  )
}
