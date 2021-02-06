namespace OsuMemoryDataProvider.Models
{
    public class BaseAddresses
    {
        public CurrentBeatmap Beatmap { get; set; } = new CurrentBeatmap();
        public Play Play { get; set; } = new Play();
        //public CurrentBeatmap Beatmap2 { get; set; } = new CurrentBeatmap();
        //public CurrentBeatmap Beatmap3 { get; set; } = new CurrentBeatmap();
        //public CurrentBeatmap Beatmap4 { get; set; } = new CurrentBeatmap();
        //public CurrentBeatmap Beatmap5 { get; set; } = new CurrentBeatmap();
        //public CurrentBeatmap Beatmap6 { get; set; } = new CurrentBeatmap();
        //public CurrentBeatmap Beatmap7 { get; set; } = new CurrentBeatmap();
        public Skin Skin { get; set; } = new Skin();
        public ResultsScreen ResultsScreen { get; set; } = new ResultsScreen();
        public Misc MiscData { get; set; } = new Misc();
    }
}