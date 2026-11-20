using Microsoft.Extensions.Caching.Memory;
using Rabbithole.Domain.DTOs.ThirdParty.Deezer;
using Rabbithole.Domain.Exceptions;
using Rabbithole.Domain.Interfaces.ThirdParty;
using Serilog;
using System.Net.Http.Json;
using System.Text.Json;

namespace Rabbithole.Domain.ThirdParty.Deezer
{
    // Public Deezer API: artist search, related artists, top tracks, albums and 30-second previews. No key needed.
    // Catalogue lookups are cached because Deezer rate limits (~50 requests per 5 seconds).
    // Single tracks are never cached: their preview URLs are signed and expire.
    public class DeezerApiClient(HttpClient httpClient, IMemoryCache memoryCache) : IDeezerApiClient
    {
        private const string BaseUrl = "https://api.deezer.com";
        private static readonly TimeSpan _cacheDuration = TimeSpan.FromHours(6);

        public Task<DeezerArtist[]> SearchArtists(string query, int limit)
        {
            return GetCachedList<DeezerArtist>($"/search/artist?q={Uri.EscapeDataString(query)}&limit={limit}");
        }

        public async Task<DeezerArtist?> GetArtist(long artistId)
        {
            var artist = await GetCached<DeezerArtist>($"/artist/{artistId}");
            return artist.Error != null || artist.Id == 0 ? null : artist;
        }

        public Task<DeezerArtist[]> GetRelatedArtists(long artistId, int limit)
        {
            return GetCachedList<DeezerArtist>($"/artist/{artistId}/related?limit={limit}");
        }

        public Task<DeezerTrack[]> GetTopTracks(long artistId, int limit)
        {
            return GetCachedList<DeezerTrack>($"/artist/{artistId}/top?limit={limit}");
        }

        public Task<DeezerAlbum[]> GetArtistAlbums(long artistId, int limit)
        {
            return GetCachedList<DeezerAlbum>($"/artist/{artistId}/albums?limit={limit}");
        }

        public async Task<DeezerAlbum?> GetAlbum(long albumId)
        {
            var album = await GetCached<DeezerAlbum>($"/album/{albumId}");
            return album.Error != null || album.Id == 0 ? null : album;
        }

        public async Task<DeezerTrack?> GetTrack(long trackId)
        {
            var track = await Get<DeezerTrack>($"/track/{trackId}");
            return track.Error != null || track.Id == 0 ? null : track;
        }

        public Task<DeezerTrack[]> SearchTracks(string artistName, string trackTitle, int limit)
        {
            var query = $"artist:\"{artistName}\" track:\"{trackTitle}\"";
            return GetCachedList<DeezerTrack>($"/search/track?q={Uri.EscapeDataString(query)}&limit={limit}");
        }

        private async Task<T[]> GetCachedList<T>(string path)
        {
            var list = await GetCached<DeezerList<T>>(path);

            if (list.Error != null)
            {
                Log.Warning("[Deezer] {Path} returned {Error}", path, list.Error.Message);
                return [];
            }

            return list.Data ?? [];
        }

        private async Task<T> GetCached<T>(string path)
        {
            var cacheKey = $"deezer:{path}";

            if (memoryCache.TryGetValue(cacheKey, out T? cached) && cached != null)
            {
                return cached;
            }

            var result = await Get<T>(path);
            memoryCache.Set(cacheKey, result, _cacheDuration);

            return result;
        }

        private async Task<T> Get<T>(string path)
        {
            try
            {
                return await httpClient.GetFromJsonAsync<T>($"{BaseUrl}{path}", JsonDefaults.Options)
                    ?? throw new ExternalServiceException("Deezer sent an empty response.");
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                Log.Warning(ex, "[Deezer] {Path} failed", path);
                throw new ExternalServiceException("Couldn't reach Deezer right now. Try again in a minute.");
            }
        }
    }
}
