using Rabbithole.Domain.DTOs.ThirdParty.Lidarr;
using Rabbithole.Domain.Enums;
using Rabbithole.Domain.Exceptions;
using Rabbithole.Domain.Interfaces.Helpers;
using Rabbithole.Domain.Interfaces.ThirdParty;
using Serilog;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Rabbithole.Domain.ThirdParty.Lidarr
{
    public class LidarrApiClient(HttpClient httpClient, IEnvironmentalSettingHelper environmentalSettingHelper) : ILidarrApiClient
    {
        public bool IsConfigured()
        {
            return environmentalSettingHelper.TryGetEnvironmentalSettingValue(EnvironmentalSettingEnum.LidarrUrl) != null
                && environmentalSettingHelper.TryGetEnvironmentalSettingValue(EnvironmentalSettingEnum.LidarrApiKey) != null;
        }

        public async Task<LidarrArtist[]> GetArtists()
        {
            return await Send<LidarrArtist[]>(HttpMethod.Get, "/artist") ?? [];
        }

        public async Task<LidarrArtistLookup[]> LookupArtists(string term)
        {
            var results = await Send<JsonArray>(HttpMethod.Get, $"/artist/lookup?term={Uri.EscapeDataString(term)}") ?? [];

            return results
                .OfType<JsonObject>()
                .Select(raw => new LidarrArtistLookup
                {
                    Artist = raw.Deserialize<LidarrArtist>(JsonDefaults.Options) ?? new LidarrArtist(),
                    Raw = raw
                })
                .ToArray();
        }

        public async Task<LidarrArtist> AddArtist(LidarrArtistLookup lookup, int qualityProfileId, int metadataProfileId, string rootFolderPath)
        {
            var body = lookup.Raw.DeepClone().AsObject();
            body["qualityProfileId"] = qualityProfileId;
            body["metadataProfileId"] = metadataProfileId;
            body["rootFolderPath"] = rootFolderPath;
            body["monitored"] = true;
            body["monitorNewItems"] = "none";

            // Only the requested album gets monitored, never the whole discography
            body["addOptions"] = new JsonObject
            {
                ["monitor"] = "none",
                ["searchForMissingAlbums"] = false
            };

            return await Send<LidarrArtist>(HttpMethod.Post, "/artist", body)
                ?? throw new ExternalServiceException("Lidarr didn't return the new artist.");
        }

        public async Task<LidarrProfile[]> GetQualityProfiles()
        {
            return await Send<LidarrProfile[]>(HttpMethod.Get, "/qualityprofile") ?? [];
        }

        public async Task<LidarrProfile[]> GetMetadataProfiles()
        {
            return await Send<LidarrProfile[]>(HttpMethod.Get, "/metadataprofile") ?? [];
        }

        public async Task<LidarrRootFolder[]> GetRootFolders()
        {
            return await Send<LidarrRootFolder[]>(HttpMethod.Get, "/rootfolder") ?? [];
        }

        public async Task<LidarrAlbum[]> GetAlbums(int artistId)
        {
            return await Send<LidarrAlbum[]>(HttpMethod.Get, $"/album?artistId={artistId}") ?? [];
        }

        public async Task<LidarrAlbum?> GetAlbum(int albumId)
        {
            return await Send<LidarrAlbum>(HttpMethod.Get, $"/album/{albumId}");
        }

        public async Task MonitorAlbums(int[] albumIds)
        {
            await Send<JsonNode>(HttpMethod.Put, "/album/monitor", new { albumIds, monitored = true });
        }

        public async Task SearchAlbums(int[] albumIds)
        {
            await Send<JsonNode>(HttpMethod.Post, "/command", new { name = "AlbumSearch", albumIds });
        }

        private async Task<T?> Send<T>(HttpMethod method, string path, object? body = null)
        {
            var baseUrl = environmentalSettingHelper.TryGetEnvironmentalSettingValue(EnvironmentalSettingEnum.LidarrUrl);
            var apiKey = environmentalSettingHelper.TryGetEnvironmentalSettingValue(EnvironmentalSettingEnum.LidarrApiKey);

            if (baseUrl == null || apiKey == null)
            {
                // Callers check IsConfigured() first, so reaching here is a bug
                throw new ExternalServiceException("Lidarr isn't set up yet. Ask an admin to add the Lidarr URL and API key.");
            }

            using (var request = new HttpRequestMessage(method, $"{baseUrl.TrimEnd('/')}/api/v1{path}"))
            {
                request.Headers.Add("X-Api-Key", apiKey);

                if (body != null)
                {
                    request.Content = JsonContent.Create(body, options: JsonDefaults.Options);
                }

                try
                {
                    using (var response = await httpClient.SendAsync(request))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            var error = await response.Content.ReadAsStringAsync();
                            Log.Warning("[Lidarr] {Method} {Path} returned {Status}: {Error}", method, path, (int)response.StatusCode, error);
                            throw new ExternalServiceException($"Lidarr returned {(int)response.StatusCode}.");
                        }

                        var text = await response.Content.ReadAsStringAsync();
                        return string.IsNullOrWhiteSpace(text) ? default : JsonSerializer.Deserialize<T>(text, JsonDefaults.Options);
                    }
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
                {
                    Log.Warning(ex, "[Lidarr] {Method} {Path} failed", method, path);
                    throw new ExternalServiceException("Couldn't reach Lidarr. Check the Lidarr URL in settings.");
                }
            }
        }
    }
}
