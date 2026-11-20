using Rabbithole.Domain.DTOs.Library;
using Rabbithole.Domain.DTOs.Playlists.Responses;
using Rabbithole.Domain.DTOs.Shared;
using Rabbithole.Domain.DTOs.ThirdParty.Navidrome;
using Rabbithole.Domain.Enums;
using Rabbithole.Domain.Extensions;
using Rabbithole.Domain.Interfaces.Api.Discover;
using Rabbithole.Domain.Interfaces.Api.Playlists;
using Rabbithole.Domain.Interfaces.Helpers;
using Rabbithole.Domain.Interfaces.ThirdParty;
using Rabbithole.Domain.Results;

namespace Rabbithole.Domain.Services.Playlists
{
    // Songs that would fit a playlist, in two groups:
    //  - From your library: AudioMuse sonic neighbours if configured, otherwise songs by similar artists you own
    //  - New to you: top tracks from similar artists you don't have yet (previewable, requestable)
    public class PlaylistSuggestionsService(
        ICurrentUserHelper currentUserHelper,
        INavidromeApiClient navidromeApiClient,
        IAudioMuseApiClient audioMuseApiClient,
        IRelatedArtistsService relatedArtistsService,
        IDeezerApiClient deezerApiClient) : IPlaylistSuggestionsService
    {
        private const int SeedArtistCount = 5;
        private const int AudioMuseSeedSongs = 6;
        private const int AudioMuseNeighbours = 10;
        private const int MaxLibrarySuggestions = 20;
        private const int NewArtistCount = 10;
        private const int TracksPerNewArtist = 2;

        public async Task<Result<GetPlaylistSuggestionsDto>> GetPlaylistSuggestions(string playlistId)
        {
            var credentials = await currentUserHelper.GetCredentials();
            var snapshot = await currentUserHelper.GetLibrarySnapshot();
            var playlist = await navidromeApiClient.GetPlaylist(credentials, playlistId);

            if (playlist == null)
            {
                return Result.NotFoundFail<GetPlaylistSuggestionsDto>("That playlist doesn't exist.");
            }

            var songs = playlist.Entry ?? [];
            var songIdsInPlaylist = songs.Select(s => s.Id).ToHashSet();

            var seedArtists = songs
                .Select(s => s.Artist)
                .OfType<string>()
                .GroupBy(a => a.NormaliseArtistName())
                .OrderByDescending(g => g.Count())
                .Take(SeedArtistCount)
                .Select(g => g.First())
                .ToArray();

            var fromAudioMuse = await GetAudioMuseSuggestions(songs, songIdsInPlaylist);
            var fromLibrary = fromAudioMuse.Length > 0
                ? fromAudioMuse
                : await GetSimilarLibraryArtistSongs(credentials, seedArtists, snapshot, songIdsInPlaylist);

            return Result.Ok(new GetPlaylistSuggestionsDto
            {
                UsedAudioMuse = fromAudioMuse.Length > 0,
                FromYourLibrary = fromLibrary,
                NewToYou = await GetNewTracks(seedArtists, snapshot)
            });
        }

        private async Task<LibrarySongItem[]> GetAudioMuseSuggestions(SubsonicSong[] songs, HashSet<string> songIdsInPlaylist)
        {
            if (!audioMuseApiClient.IsConfigured() || songs.Length == 0)
            {
                return [];
            }

            // Spread the seeds across the playlist rather than taking the first few
            var step = Math.Max(1, songs.Length / AudioMuseSeedSongs);
            var seedSongs = songs.Where((_, index) => index % step == 0).Take(AudioMuseSeedSongs);

            var neighbours = new List<(string SongId, string? Title, string? Artist, string? Album, double Distance)>();

            foreach (var seed in seedSongs)
            {
                var similar = await audioMuseApiClient.GetSimilarTracks(seed.Id, AudioMuseNeighbours);
                neighbours.AddRange(similar.Select(t => (t.ItemId, t.Title, t.Author, t.Album, t.Distance ?? double.MaxValue)));
            }

            return neighbours
                .Where(n => !songIdsInPlaylist.Contains(n.SongId))
                .GroupBy(n => n.SongId)
                .Select(g => g.OrderBy(n => n.Distance).First())
                .OrderBy(n => n.Distance)
                .Take(MaxLibrarySuggestions)
                .Select(n => new LibrarySongItem
                {
                    SongId = n.SongId,
                    Title = n.Title ?? "Unknown track",
                    ArtistName = n.Artist,
                    AlbumTitle = n.Album,
                    CoverArtId = n.SongId
                })
                .ToArray();
        }

        private async Task<LibrarySongItem[]> GetSimilarLibraryArtistSongs(NavidromeCredentials credentials, string[] seedArtists, LibrarySnapshot snapshot, HashSet<string> songIdsInPlaylist)
        {
            var related = await relatedArtistsService.GetRelatedArtists(seedArtists, snapshot, 15, 30, includeLibraryArtists: true);
            var ownedArtists = related.Where(r => r.Artist.LibraryState == LibraryState.InLibrary).Take(6);
            var suggestions = new List<LibrarySongItem>();

            foreach (var artist in ownedArtists)
            {
                var normalisedName = artist.Artist.Name.NormaliseArtistName();
                var results = await navidromeApiClient.Search(credentials, artist.Artist.Name, 0, 0, 10);

                suggestions.AddRange((results.Song ?? [])
                    .Where(s => s.Artist.NormaliseArtistName() == normalisedName && !songIdsInPlaylist.Contains(s.Id))
                    .Take(3)
                    .Select(s => s.ToLibrarySongItem()));
            }

            return suggestions.Take(MaxLibrarySuggestions).ToArray();
        }

        private async Task<SuggestedTrackItem[]> GetNewTracks(string[] seedArtists, LibrarySnapshot snapshot)
        {
            var related = await relatedArtistsService.GetRelatedArtists(seedArtists, snapshot, 10, NewArtistCount, includeLibraryArtists: false);
            var suggestions = new List<SuggestedTrackItem>();

            foreach (var artist in related)
            {
                var topTracks = await deezerApiClient.GetTopTracks(artist.Artist.DeezerArtistId, TracksPerNewArtist);

                suggestions.AddRange(topTracks.Select(t => new SuggestedTrackItem
                {
                    Track = t.ToTrackItem(),
                    LibraryState = artist.Artist.LibraryState,
                    BecauseOf = artist.BecauseOf
                }));
            }

            return suggestions.ToArray();
        }
    }
}
