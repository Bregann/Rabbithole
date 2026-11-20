using Rabbithole.Domain.DTOs.ThirdParty.ListenBrainz;
using Rabbithole.Domain.Enums;
using Rabbithole.Domain.Exceptions;
using Rabbithole.Domain.Interfaces.Helpers;
using Rabbithole.Domain.Interfaces.ThirdParty;
using Serilog;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Rabbithole.Domain.ThirdParty.ListenBrainz
{
    public class ListenBrainzApiClient(HttpClient httpClient, IEnvironmentalSettingHelper environmentalSettingHelper) : IListenBrainzApiClient
    {
        private const string DefaultBaseUrl = "https://api.listenbrainz.org";

        public async Task<bool> UserExists(string username)
        {
            var response = await Send($"/1/user/{Uri.EscapeDataString(username)}/listen-count");
            return response.IsSuccessStatusCode;
        }

        public async Task<ListenBrainzArtistStat[]> GetTopArtists(string username, int count)
        {
            var response = await Send($"/1/stats/user/{Uri.EscapeDataString(username)}/artists?range=month&count={count}");

            // 204: stats not calculated yet. 404: unknown user.
            if (response.StatusCode is HttpStatusCode.NoContent or HttpStatusCode.NotFound)
            {
                return [];
            }

            var stats = await Read<ListenBrainzArtistStatsResponse>(response);
            return stats?.Payload?.Artists ?? [];
        }

        public async Task<ListenBrainzPlaylist[]> GetCreatedForPlaylists(string username)
        {
            var response = await Send($"/1/user/{Uri.EscapeDataString(username)}/playlists/createdfor?count=25");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return [];
            }

            var result = await Read<ListenBrainzCreatedForResponse>(response);

            return result?.Playlists?
                .Where(p => p.Playlist != null)
                .Select(p => p.Playlist!)
                .ToArray() ?? [];
        }

        public async Task<ListenBrainzPlaylist?> GetPlaylist(string playlistMbid)
        {
            var response = await Send($"/1/playlist/{Uri.EscapeDataString(playlistMbid)}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            var result = await Read<ListenBrainzPlaylistWrapper>(response);
            return result?.Playlist;
        }

        private async Task<HttpResponseMessage> Send(string path)
        {
            var baseUrl = environmentalSettingHelper.TryGetEnvironmentalSettingValue(EnvironmentalSettingEnum.ListenBrainzApiUrl) ?? DefaultBaseUrl;

            try
            {
                return await httpClient.GetAsync($"{baseUrl.TrimEnd('/')}{path}");
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                Log.Warning(ex, "[ListenBrainz] {Path} failed", path);
                throw new ExternalServiceException("Couldn't reach ListenBrainz right now.");
            }
        }

        private static async Task<T?> Read<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new ExternalServiceException($"ListenBrainz returned {(int)response.StatusCode}.");
            }

            try
            {
                return await response.Content.ReadFromJsonAsync<T>(JsonDefaults.Options);
            }
            catch (JsonException ex)
            {
                Log.Warning(ex, "[ListenBrainz] Unexpected response shape");
                throw new ExternalServiceException("ListenBrainz sent something unexpected.");
            }
        }
    }
}
