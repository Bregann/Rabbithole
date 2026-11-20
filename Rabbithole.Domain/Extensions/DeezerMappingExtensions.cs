using Rabbithole.Domain.DTOs.Artists.Responses;
using Rabbithole.Domain.DTOs.Library;
using Rabbithole.Domain.DTOs.Shared;
using Rabbithole.Domain.DTOs.ThirdParty.Deezer;
using Rabbithole.Domain.Enums;
using System.Globalization;

namespace Rabbithole.Domain.Extensions
{
    public static class DeezerMappingExtensions
    {
        public static ArtistSummaryItem ToArtistSummaryItem(this DeezerArtist artist, LibrarySnapshot snapshot)
        {
            return new ArtistSummaryItem
            {
                DeezerArtistId = artist.Id,
                Name = artist.Name,
                ImageUrl = artist.PictureXl ?? artist.PictureMedium,
                FanCount = artist.FanCount,
                LibraryState = snapshot.GetState(artist.Name),
                IsBlocked = snapshot.IsBlocked(artist.Name)
            };
        }

        public static TrackItem ToTrackItem(this DeezerTrack track)
        {
            return new TrackItem
            {
                DeezerTrackId = track.Id,
                Title = track.Title,
                ArtistName = track.Artist?.Name ?? "Unknown artist",
                DeezerArtistId = track.Artist?.Id,
                AlbumTitle = track.Album?.Title,
                DeezerAlbumId = track.Album?.Id,
                CoverUrl = track.Album?.CoverMedium,
                DurationSeconds = track.Duration,
                HasPreview = !string.IsNullOrWhiteSpace(track.Preview)
            };
        }

        public static AlbumItem ToAlbumItem(this DeezerAlbum album, RequestStatus? requestStatus)
        {
            return new AlbumItem
            {
                DeezerAlbumId = album.Id,
                Title = album.Title,
                CoverUrl = album.CoverXl ?? album.CoverMedium,
                ReleaseYear = DateOnly.TryParse(album.ReleaseDate, CultureInfo.InvariantCulture, out var releaseDate) ? releaseDate.Year : null,
                RecordType = album.RecordType,
                RequestStatus = requestStatus
            };
        }
    }
}
