using Rabbithole.Domain.DTOs.ThirdParty.AudioMuse;

namespace Rabbithole.Domain.Interfaces.ThirdParty
{
    public interface IAudioMuseApiClient
    {
        bool IsConfigured();
        Task<AudioMuseTrack[]> GetSimilarTracks(string songId, int count);
    }
}
