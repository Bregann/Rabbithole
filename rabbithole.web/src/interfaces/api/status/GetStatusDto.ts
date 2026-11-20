import type { IntegrationType } from '@/interfaces/api/status/IntegrationType'

export interface GetStatusDto {
  version: string
  isFullyConfigured: boolean
  integrations: IntegrationStatusItem[]
}

export interface IntegrationStatusItem {
  integration: IntegrationType
  isConfigured: boolean
}
