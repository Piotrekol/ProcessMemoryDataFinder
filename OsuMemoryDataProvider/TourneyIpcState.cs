namespace OsuMemoryDataProvider
{
    public enum TourneyIpcState
    {
        Initialising,
        Idle,
        WaitingForClients,
        Playing,
        Ranking,

        /// <summary>
        /// Indicates that state read in osu memory is not defined in <see cref="TourneyIpcState"/>
        /// </summary>
        Unknown = -1,
    }
}