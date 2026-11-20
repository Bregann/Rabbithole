using Rabbithole.Domain.Enums;
using Rabbithole.Domain.Extensions;

namespace Rabbithole.Domain.DTOs.Library
{
    // What one user has right now: artists in Navidrome, artists Lidarr tracks, and artists they've blocked.
    // Built once per request so labelling dozens of artists doesn't mean dozens of API calls.
    public class LibrarySnapshot(HashSet<string> libraryArtists, HashSet<string> lidarrArtists, HashSet<string> blockedArtists)
    {
        public LibraryState GetState(string? artistName)
        {
            var normalised = artistName.NormaliseArtistName();

            if (libraryArtists.Contains(normalised))
            {
                return LibraryState.InLibrary;
            }

            return lidarrArtists.Contains(normalised) ? LibraryState.InLidarr : LibraryState.New;
        }

        public bool IsBlocked(string? artistName)
        {
            return blockedArtists.Contains(artistName.NormaliseArtistName());
        }
    }
}
