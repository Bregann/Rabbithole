using Rabbithole.Domain.DTOs.ThirdParty.Navidrome;
using Rabbithole.Domain.Enums;
using Rabbithole.Domain.Exceptions;
using Rabbithole.Domain.Interfaces.Helpers;
using Rabbithole.Domain.Interfaces.ThirdParty;
using Serilog;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Rabbithole.Domain.ThirdParty.Navidrome
{
    // Talks to Navidrome's Subsonic API as a specific user, using token auth (md5(password + salt)).
    public class NavidromeApiClient(HttpClient httpClient, IEnvironmentalSettingHelper environmentalSettingHelper) : INavidromeApiClient
    {
        private const string ApiVersion = "1.16.1";
        private const string ClientName = "rabbithole";
        private const int WrongCredentialsErrorCode = 40;
        private const int NotFoundErrorCode = 70;

        public async Task<bool> Ping(NavidromeCredentials credentials)
        {
            var response = await Send("ping", credentials, []);

            if (response.Status == "ok")
            {
                return true;
            }

            if (response.Error?.Code == WrongCredentialsErrorCode)
            {
                return false;
            }

            throw new ExternalServiceException($"Navidrome refused the request: {response.Error?.Message}");
        }

        public async Task<SubsonicPlaylist[]> GetPlaylists(NavidromeCredentials credentials)
        {
            var response = await SendOk("getPlaylists", credentials, []);
            return response.Playlists?.Playlist ?? [];
        }

        public async Task<SubsonicPlaylist?> GetPlaylist(NavidromeCredentials credentials, string playlistId)
        {
            var response = await Send("getPlaylist", credentials, [Param("id", playlistId)]);

            if (response.Error?.Code == NotFoundErrorCode)
            {
                return null;
            }

            return EnsureOk(response).Playlist;
        }

        public async Task<SubsonicPlaylist> CreatePlaylist(NavidromeCredentials credentials, string name, string[] songIds)
        {
            var parameters = new List<KeyValuePair<string, string>> { Param("name", name) };
            parameters.AddRange(songIds.Select(id => Param("songId", id)));

            var response = await SendOk("createPlaylist", credentials, parameters);
            return response.Playlist ?? throw new ExternalServiceException("Navidrome didn't return the new playlist.");
        }

        // Returns false when the playlist no longer exists
        public async Task<bool> ReplacePlaylistSongs(NavidromeCredentials credentials, string playlistId, string[] songIds)
        {
            // createPlaylist with an existing playlistId replaces its songs
            var parameters = new List<KeyValuePair<string, string>> { Param("playlistId", playlistId) };
            parameters.AddRange(songIds.Select(id => Param("songId", id)));

            var response = await Send("createPlaylist", credentials, parameters);

            if (response.Error?.Code == NotFoundErrorCode)
            {
                return false;
            }

            EnsureOk(response);
            return true;
        }

        public async Task UpdatePlaylist(NavidromeCredentials credentials, string playlistId, string[] songIdsToAdd, int[] songIndexesToRemove, bool? isPublic = null)
        {
            var parameters = new List<KeyValuePair<string, string>> { Param("playlistId", playlistId) };
            parameters.AddRange(songIdsToAdd.Select(id => Param("songIdToAdd", id)));
            parameters.AddRange(songIndexesToRemove.Select(index => Param("songIndexToRemove", index.ToString())));

            if (isPublic != null)
            {
                parameters.Add(Param("public", isPublic.Value ? "true" : "false"));
            }

            await SendOk("updatePlaylist", credentials, parameters);
        }

        public async Task<SubsonicSearchResult> Search(NavidromeCredentials credentials, string query, int artistCount, int albumCount, int songCount)
        {
            var response = await SendOk("search3", credentials,
            [
                Param("query", query),
                Param("artistCount", artistCount.ToString()),
                Param("albumCount", albumCount.ToString()),
                Param("songCount", songCount.ToString())
            ]);

            return response.SearchResult3 ?? new SubsonicSearchResult();
        }

        public async Task<SubsonicArtist[]> GetArtists(NavidromeCredentials credentials)
        {
            var response = await SendOk("getArtists", credentials, []);

            return response.Artists?.Index?
                .SelectMany(i => i.Artist ?? [])
                .ToArray() ?? [];
        }

        public async Task<SubsonicAlbum[]> GetFrequentAlbums(NavidromeCredentials credentials, int size)
        {
            var response = await SendOk("getAlbumList2", credentials, [Param("type", "frequent"), Param("size", size.ToString())]);
            return response.AlbumList2?.Album ?? [];
        }

        public async Task<NavidromeStream?> Stream(NavidromeCredentials credentials, string songId, string? rangeHeader)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl("stream", credentials, [Param("id", songId)], asJson: false));

            if (!string.IsNullOrWhiteSpace(rangeHeader))
            {
                request.Headers.TryAddWithoutValidation("Range", rangeHeader);
            }

            return await SendBinary(request);
        }

        public async Task<NavidromeStream?> GetCoverArt(NavidromeCredentials credentials, string coverArtId, int size)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl("getCoverArt", credentials, [Param("id", coverArtId), Param("size", size.ToString())], asJson: false));
            return await SendBinary(request);
        }

        private async Task<SubsonicResponse> SendOk(string method, NavidromeCredentials credentials, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            return EnsureOk(await Send(method, credentials, parameters));
        }

        // Anything other than "ok" here is unexpected (missing items are handled by the callers that expect them)
        private static SubsonicResponse EnsureOk(SubsonicResponse response)
        {
            if (response.Status == "ok")
            {
                return response;
            }

            if (response.Error?.Code == WrongCredentialsErrorCode)
            {
                throw new ExternalServiceException("Navidrome rejected the saved password. Sign out and in again.");
            }

            throw new ExternalServiceException($"Navidrome returned an error: {response.Error?.Message}");
        }

        private async Task<SubsonicResponse> Send(string method, NavidromeCredentials credentials, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            var url = BuildUrl(method, credentials, parameters, asJson: true);

            try
            {
                var envelope = await httpClient.GetFromJsonAsync<SubsonicEnvelope>(url, JsonDefaults.Options);
                return envelope?.Response ?? throw new ExternalServiceException("Navidrome sent an empty response.");
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                Log.Warning(ex, "[Navidrome] {Method} failed", method);
                throw new ExternalServiceException("Couldn't reach Navidrome. Check the Navidrome URL in settings.");
            }
        }

        // Returns null when Navidrome doesn't have the song or image
        private async Task<NavidromeStream?> SendBinary(HttpRequestMessage request)
        {
            HttpResponseMessage response;

            try
            {
                response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                Log.Warning(ex, "[Navidrome] Binary request failed");
                throw new ExternalServiceException("Couldn't reach Navidrome. Check the Navidrome URL in settings.");
            }

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

            // Subsonic reports errors on binary endpoints as a JSON/XML body
            if (!response.IsSuccessStatusCode || contentType.Contains("json") || contentType.Contains("xml"))
            {
                response.Dispose();
                return null;
            }

            return new NavidromeStream
            {
                Content = await response.Content.ReadAsStreamAsync(),
                ContentType = contentType,
                StatusCode = (int)response.StatusCode,
                ContentLength = response.Content.Headers.ContentLength,
                ContentRange = response.Content.Headers.ContentRange?.ToString(),
                AcceptRanges = response.Headers.AcceptRanges.Count > 0 ? string.Join(",", response.Headers.AcceptRanges) : null
            };
        }

        private string BuildUrl(string method, NavidromeCredentials credentials, IEnumerable<KeyValuePair<string, string>> parameters, bool asJson)
        {
            var baseUrl = environmentalSettingHelper.TryGetEnvironmentalSettingValue(EnvironmentalSettingEnum.NavidromeUrl)
                ?? throw new ExternalServiceException("Navidrome isn't set up yet. Ask an admin to add the Navidrome URL.");

            var salt = Convert.ToHexString(RandomNumberGenerator.GetBytes(8)).ToLowerInvariant();

#pragma warning disable CA5351 // The Subsonic API's token auth is defined as md5(password + salt)
            var token = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(credentials.Password + salt))).ToLowerInvariant();
#pragma warning restore CA5351

            var query = new List<string>
            {
                $"u={Uri.EscapeDataString(credentials.Username)}",
                $"t={token}",
                $"s={salt}",
                $"v={ApiVersion}",
                $"c={ClientName}"
            };

            if (asJson)
            {
                query.Add("f=json");
            }

            query.AddRange(parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

            return $"{baseUrl.TrimEnd('/')}/rest/{method}?{string.Join('&', query)}";
        }

        private static KeyValuePair<string, string> Param(string key, string value)
        {
            return new KeyValuePair<string, string>(key, value);
        }
    }
}
