using System.Collections.Generic;
using System.Linq;
using ProcessMemoryDataFinder.Structured;

namespace OsuMemoryDataProvider.Models
{
    [MemoryAddress("[[CurrentRuleset]+0x74]+0x24")]
    public class LeaderBoard
    {
        public LeaderBoard()
        {
            //single player: top50 + player top score + current score
            RawPlayers = Enumerable.Range(0, 52).Select(x => new MultiplayerPlayer()).ToList();
        }

        [MemoryAddress("")]
        private int RawHasLeaderboard { get; set; }
        public bool HasLeaderBoard => RawHasLeaderboard != 0;

        [MemoryAddress("[[]+0x10]")]
        public MainPlayer MainPlayer { get; set; } = new MainPlayer();

        [MemoryAddress("[[]+0x4]+0xC")]
        public int AmountOfPlayers { get; set; }
        private List<MultiplayerPlayer> _players;
        [MemoryAddress("[]+0x4")]
        private List<MultiplayerPlayer> RawPlayers
        {
            get => _players;
            set
            {
                _players = value;
                Players = _players.GetRange(0, AmountOfPlayers > _players.Count ? _players.Count : AmountOfPlayers);
            }
        }
        public List<MultiplayerPlayer> Players { get; private set; }
    }
}