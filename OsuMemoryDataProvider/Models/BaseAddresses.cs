namespace OsuMemoryDataProvider.Models
{
    public class BaseAddresses
    {
        public CurrentBeatmap Beatmap { get; set; } = new CurrentBeatmap();
        public Player Player { get; set; } = new Player();
        public LeaderBoard LeaderBoard { get; set; } = new LeaderBoard();

        public Skin Skin { get; set; } = new Skin();
        public ResultsScreen ResultsScreen { get; set; } = new ResultsScreen();
        public Misc MiscData { get; set; } = new Misc();
    }
}