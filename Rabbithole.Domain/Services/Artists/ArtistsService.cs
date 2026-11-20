using Microsoft.EntityFrameworkCore;
using Rabbithole.Domain.DTOs.Artists.Responses;
using Rabbithole.Domain.Database.Context;
using Rabbithole.Domain.Extensions;
using Rabbithole.Domain.Interfaces.Api.Artists;
using Rabbithole.Domain.Interfaces.Helpers;
using Rabbithole.Domain.Interfaces.ThirdParty;
using Rabbithole.Domain.Results;

namespace Rabbithole.Domain.Services.Artists
{
    public class ArtistsService(
        AppDbContext context,
        IDeezerApiClient deezerApiClient,
        ICurrentUserHelper currentUserHelper) : IArtistsService
    {
        private const int SearchLimit = 20;
        private const int TopTrackLimit = 10;
        private const int AlbumLimit = 50;
        private const int SimilarArtistLimit = 20;
        private const int GraphArtistLimit = 12;

        public async Task<Result<SearchArtistsDto>> SearchArtists(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Result.Fail<SearchArtistsDto>("Type an artist to search for.");
            }

            var snapshot = await currentUserHelper.GetLibrarySnapshot();
            var artists = await deezerApiClient.SearchArtists(query.Trim(), SearchLimit);

            return Result.Ok(new SearchArtistsDto
            {
                Artists = artists.Select(a => a.ToArtistSummaryItem(snapshot)).ToArray()
            });
        }

        public async Task<Result<GetArtistDto>> GetArtist(long deezerArtistId)
        {
            var snapshot = await currentUserHelper.GetLibrarySnapshot();
            var userId = currentUserHelper.GetUserId();

            var artist = await deezerApiClient.GetArtist(deezerArtistId);

            if (artist == null)
            {
                return Result.NotFoundFail<GetArtistDto>("Couldn't find that artist.");
            }

            var topTracks = await deezerApiClient.GetTopTracks(deezerArtistId, TopTrackLimit);
            var albums = await deezerApiClient.GetArtistAlbums(deezerArtistId, AlbumLimit);
            var similarArtists = await deezerApiClient.GetRelatedArtists(deezerArtistId, SimilarArtistLimit);

            // Show this user's latest request status against each album
            var albumIds = albums.Select(a => a.Id).ToArray();
            var requestStatuses = (await context.AlbumRequests
                .Where(r => r.UserId == userId && albumIds.Contains(r.DeezerAlbumId))
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => new { r.DeezerAlbumId, r.Status })
                .ToArrayAsync())
                .GroupBy(r => r.DeezerAlbumId)
                .ToDictionary(g => g.Key, g => g.First().Status);

            return Result.Ok(new GetArtistDto
            {
                Artist = artist.ToArtistSummaryItem(snapshot),
                TopTracks = topTracks.Select(t => t.ToTrackItem()).ToArray(),
                Albums = albums
                    .OrderByDescending(a => a.ReleaseDate)
                    .Select(a => a.ToAlbumItem(requestStatuses.TryGetValue(a.Id, out var status) ? status : null))
                    .ToArray(),
                SimilarArtists = similarArtists
                    .Where(a => !snapshot.IsBlocked(a.Name))
                    .Select(a => a.ToArtistSummaryItem(snapshot))
                    .ToArray()
            });
        }

        public async Task<Result<GetSimilarArtistsDto>> GetSimilarArtists(long deezerArtistId)
        {
            var snapshot = await currentUserHelper.GetLibrarySnapshot();

            var artist = await deezerApiClient.GetArtist(deezerArtistId);

            if (artist == null)
            {
                return Result.NotFoundFail<GetSimilarArtistsDto>("Couldn't find that artist.");
            }

            var similarArtists = await deezerApiClient.GetRelatedArtists(deezerArtistId, SimilarArtistLimit);

            return Result.Ok(new GetSimilarArtistsDto
            {
                Artist = artist.ToArtistSummaryItem(snapshot),
                SimilarArtists = similarArtists
                    .Where(a => !snapshot.IsBlocked(a.Name))
                    .Take(GraphArtistLimit)
                    .Select(a => a.ToArtistSummaryItem(snapshot))
                    .ToArray()
            });
        }
    }
}
