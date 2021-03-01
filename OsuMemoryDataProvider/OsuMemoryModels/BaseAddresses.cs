using OsuMemoryDataProvider.OsuMemoryModels.Abstract;
using OsuMemoryDataProvider.OsuMemoryModels.Direct;

namespace OsuMemoryDataProvider.OsuMemoryModels
{
    public class OsuBaseAddresses
    {
        public CurrentBeatmap Beatmap { get; set; } = new CurrentBeatmap();
        public Player Player { get; set; } = new Player();
        public LeaderBoard LeaderBoard { get; set; } = new LeaderBoard();
        public Skin Skin { get; set; } = new Skin();
        public ResultsScreen ResultsScreen { get; set; } = new ResultsScreen();
        public GeneralData GeneralData { get; set; } = new GeneralData();
    }
}