export interface UpdateIntegrationSettingsRequest {
  settings: UpdateIntegrationSettingItem[]
}

export interface UpdateIntegrationSettingItem {
  key: string
  value: string
}
