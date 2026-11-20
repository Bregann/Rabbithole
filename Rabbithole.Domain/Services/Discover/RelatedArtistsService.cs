using Rabbithole.Domain.DTOs.Discover;
using Rabbithole.Domain.DTOs.Library;
using Rabbithole.Domain.DTOs.ThirdParty.Deezer;
using Rabbithole.Domain.Enums;
using Rabbithole.Domain.Extensions;
using Rabbithole.Domain.Interfaces.Api.Discover;
using Rabbithole.Domain.Interfaces.ThirdParty;

namespace Rabbithole.Domain.Services.Discover
{
    // The core of discovery: take some seed artists, find who's similar to each,
    // and rank the results by how many seeds point at them.
    public class RelatedArtistsService(IDeezerApiClient deezerApiClient) : IRelatedArtistsService
    {
        public async Task<RelatedArtistResult[]> GetRelatedArtists(string[] seedArtistNames, LibrarySnapshot snapshot, int perSeed, int take, bool includeLibraryArtists)
        {
            var seeds = seedArtistNames.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToArray();
            var normalisedSeeds = seeds.Select(s => s.NormaliseArtistName()).ToHashSet();
            var found = new Dictionary<long, (DeezerArtist Artist, List<string> BecauseOf)>();

            foreach (var seed in seeds)
            {
                var seedArtist = await FindArtist(seed);

                if (seedArtist == null)
                {
                    continue;
                }

                foreach (var related in await deezerApiClient.GetRelatedArtists(seedArtist.Id, perSeed))
                {
                    if (normalisedSeeds.Contains(related.Name.NormaliseArtistName()) || snapshot.IsBlocked(related.Name))
                    {
                        continue;
                    }

                    if (!includeLibraryArtists && snapshot.GetState(related.Name) == LibraryState.InLibrary)
                    {
                        continue;
                    }

                    if (!found.TryGetValue(related.Id, out var entry))
                    {
                        entry = (related, new List<string>());
                        found[related.Id] = entry;
                    }

                    entry.BecauseOf.Add(seed);
                }
            }

            return found.Values
                .OrderByDescending(e => e.BecauseOf.Count)
                .ThenByDescending(e => e.Artist.FanCount ?? 0)
                .Take(take)
                .Select(e => new RelatedArtistResult
                {
                    Artist = e.Artist.ToArtistSummaryItem(snapshot),
                    BecauseOf = e.BecauseOf.Distinct().ToArray()
                })
                .ToArray();
        }

        private async Task<DeezerArtist?> FindArtist(string name)
        {
            var results = await deezerApiClient.SearchArtists(name, 5);
            var normalisedName = name.NormaliseArtistName();

            return results.FirstOrDefault(a => a.Name.NormaliseArtistName() == normalisedName) ?? results.FirstOrDefault();
        }
    }
}
