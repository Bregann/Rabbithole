export interface GetIntegrationSettingsDto {
  settings: IntegrationSettingItem[]
}

export interface IntegrationSettingItem {
  key: string
  value: string
  isSecret: boolean
  hasValue: boolean
}
