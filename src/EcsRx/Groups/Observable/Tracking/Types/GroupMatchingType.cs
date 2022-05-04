namespace EcsRx.Groups.Observable.Tracking.Types
{
    public enum GroupMatchingType : byte
    {
        MatchesNoExcludes = 1,
        MatchesWithExcludes = 2,
        NoMatchesWithExcludes = 3,
        NoMatchesNoExcludes = 4
    }
}