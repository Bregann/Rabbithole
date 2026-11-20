using Rabbithole.Domain.DTOs.Playlists.Responses;
using Rabbithole.Domain.DTOs.Shared;
using Rabbithole.Domain.DTOs.ThirdParty.Navidrome;

namespace Rabbithole.Domain.Extensions
{
    public static class SubsonicMappingExtensions
    {
        public static LibrarySongItem ToLibrarySongItem(this SubsonicSong song)
        {
            return new LibrarySongItem
            {
                SongId = song.Id,
                Title = song.Title,
                ArtistName = song.Artist,
                AlbumTitle = song.Album,
                DurationSeconds = song.Duration,
                CoverArtId = song.CoverArt ?? song.AlbumId
            };
        }

        public static PlaylistItem ToPlaylistItem(this SubsonicPlaylist playlist, string currentUsername, string? weeklyPlaylistId)
        {
            return new PlaylistItem
            {
                Id = playlist.Id,
                Name = playlist.Name,
                SongCount = playlist.SongCount,
                DurationSeconds = playlist.Duration,
                CoverArtId = playlist.CoverArt,
                IsPublic = playlist.Public,
                Owner = playlist.Owner,
                IsOwnedByMe = string.Equals(playlist.Owner, currentUsername, StringComparison.OrdinalIgnoreCase),
                IsRabbitholeWeekly = playlist.Id == weeklyPlaylistId
            };
        }
    }
}
