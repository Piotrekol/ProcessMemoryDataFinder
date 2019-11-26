using System.Reflection;

namespace OsuMemoryDataProvider
{
    public class PlayContainer
    {
        public double Acc { get; set; }
        public ushort C300 { get; set; }
        public ushort C100 { get; set; }
        public ushort C50 { get; set; }
        public ushort CGeki { get; set; }
        public ushort CKatsu { get; set; }
        public ushort CMiss { get; set; }
        public ushort MaxCombo { get; set; }
        public ushort Combo { get; set; }
        public double Hp { get; set; }

        public int Score { get; set; }

        [Obfuscation(Exclude = true)]
        public void Reset()
        {
            Acc = 0;
            C300 = 0;
            C100 = 0;
            C50 = 0;
            CGeki = 0;
            CKatsu = 0;
            CMiss = 0;
            Combo = 0;
            MaxCombo = 0;
            Hp = 0;
            Score = 0;
        }
    }
}