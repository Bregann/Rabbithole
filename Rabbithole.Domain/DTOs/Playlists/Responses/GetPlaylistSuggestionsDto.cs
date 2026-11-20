using Rabbithole.Domain.DTOs.Shared;

namespace Rabbithole.Domain.DTOs.Playlists.Responses
{
    public class GetPlaylistSuggestionsDto
    {
        public required bool UsedAudioMuse { get; set; }
        public required LibrarySongItem[] FromYourLibrary { get; set; } = [];
        public required SuggestedTrackItem[] NewToYou { get; set; } = [];
    }
}
