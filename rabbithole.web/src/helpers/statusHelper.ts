import { IntegrationType } from '@/interfaces/api/status/IntegrationType'

const integrationLabels: Record<IntegrationType, string> = {
  [IntegrationType.Navidrome]: 'Navidrome',
  [IntegrationType.ListenBrainz]: 'ListenBrainz',
  [IntegrationType.Lidarr]: 'Lidarr',
  [IntegrationType.AudioMuse]: 'AudioMuse',
}

const integrationDescriptions: Record<IntegrationType, string> = {
  [IntegrationType.Navidrome]: 'Sign-in, library matching and playlists',
  [IntegrationType.ListenBrainz]: 'Listening history and recommendations',
  [IntegrationType.Lidarr]: 'Requesting albums you don\'t have',
  [IntegrationType.AudioMuse]: 'Ranking suggestions by how they sound',
}

const integrationSettings: Record<IntegrationType, string> = {
  [IntegrationType.Navidrome]: 'NavidromeUrl',
  [IntegrationType.ListenBrainz]: 'ListenBrainzApiUrl',
  [IntegrationType.Lidarr]: 'LidarrUrl, LidarrApiKey',
  [IntegrationType.AudioMuse]: 'AudioMuseUrl',
}

export const getIntegrationLabel = (integration: IntegrationType): string => integrationLabels[integration]

export const getIntegrationDescription = (integration: IntegrationType): string => integrationDescriptions[integration]

export const getIntegrationSettings = (integration: IntegrationType): string => integrationSettings[integration]
