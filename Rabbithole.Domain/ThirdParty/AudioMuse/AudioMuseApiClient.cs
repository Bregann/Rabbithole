using Rabbithole.Domain.DTOs.ThirdParty.AudioMuse;
using Rabbithole.Domain.Enums;
using Rabbithole.Domain.Interfaces.Helpers;
using Rabbithole.Domain.Interfaces.ThirdParty;
using Serilog;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Rabbithole.Domain.ThirdParty.AudioMuse
{
    // AudioMuse-AI: sonic similarity between songs it has analysed from Navidrome.
    // Optional, so failures return nothing instead of throwing.
    public class AudioMuseApiClient(HttpClient httpClient, IEnvironmentalSettingHelper environmentalSettingHelper) : IAudioMuseApiClient
    {
        public bool IsConfigured()
        {
            return environmentalSettingHelper.TryGetEnvironmentalSettingValue(EnvironmentalSettingEnum.AudioMuseUrl) != null;
        }

        public async Task<AudioMuseTrack[]> GetSimilarTracks(string songId, int count)
        {
            var baseUrl = environmentalSettingHelper.TryGetEnvironmentalSettingValue(EnvironmentalSettingEnum.AudioMuseUrl);

            if (baseUrl == null)
            {
                return [];
            }

            using (var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl.TrimEnd('/')}/api/similar_tracks?item_id={Uri.EscapeDataString(songId)}&n={count}&eliminate_duplicates=true"))
            {
                var token = environmentalSettingHelper.TryGetEnvironmentalSettingValue(EnvironmentalSettingEnum.AudioMuseApiToken);
                if (token != null)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                try
                {
                    using (var response = await httpClient.SendAsync(request))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            // 404: AudioMuse hasn't analysed that song yet
                            return [];
                        }

                        return await response.Content.ReadFromJsonAsync<AudioMuseTrack[]>(JsonDefaults.Options) ?? [];
                    }
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
                {
                    Log.Warning(ex, "[AudioMuse] Similar tracks for {SongId} failed", songId);
                    return [];
                }
            }
        }
    }
}
