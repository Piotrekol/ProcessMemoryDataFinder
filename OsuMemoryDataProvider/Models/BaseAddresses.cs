namespace OsuMemoryDataProvider.Models
{
    public class BaseAddresses
    {
        public CurrentBeatmap Beatmap { get; set; } = new CurrentBeatmap();
        public Play Play { get; set; } = new Play();
        public Skin Skin { get; set; } = new Skin();
        public ResultsScreen ResultsScreen { get; set; } = new ResultsScreen();
        public Misc MiscData { get; set; } = new Misc();
    }
}