namespace Rabbithole.Domain.Exceptions
{
    // An upstream service (Navidrome, Deezer, ListenBrainz, Lidarr) is unreachable, misconfigured or answered
    // with something unexpected. That's exceptional, not an expected outcome, so it isn't a Result: it becomes
    // a generic 500. Callers that can carry on without the service (e.g. Lidarr labels) catch it and degrade.
    public class ExternalServiceException(string message) : Exception(message);
}
