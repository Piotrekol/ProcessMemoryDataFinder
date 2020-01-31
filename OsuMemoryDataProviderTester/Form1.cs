using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using OsuMemoryDataProvider;

namespace OsuMemoryDataProviderTester
{
    public partial class Form1 : Form
    {
        private readonly string _osuWindowTitleHint;
        private int _readDelay = 33;
        private Thread _thread;
        private readonly IOsuMemoryReader _reader;

        public Form1(string osuWindowTitleHint)
        {
            _osuWindowTitleHint = osuWindowTitleHint;
            InitializeComponent();
            _reader = OsuMemoryReader.Instance.GetInstanceForWindowTitleHint(osuWindowTitleHint);
            Shown += OnShown;
            Closing += OnClosing;
            numericUpDown_readDelay.ValueChanged += NumericUpDownReadDelayOnValueChanged;
        }

        private void NumericUpDownReadDelayOnValueChanged(object sender, EventArgs eventArgs)
        {
            if (int.TryParse(numericUpDown_readDelay.Value.ToString(CultureInfo.InvariantCulture), out var value))
            {
                _readDelay = value;
            }
            else
            {
                numericUpDown_readDelay.Value = 33;
            }
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            _thread?.Abort();
        }

        private void OnShown(object sender, EventArgs eventArgs)
        {
            if (!string.IsNullOrEmpty(_osuWindowTitleHint)) Text += $": {_osuWindowTitleHint}";
            _thread = new Thread(() =>
            {
                try
                {
                    var playContainer = new PlayContainerEx();
                    var playReseted = false;
                    while (true)
                    {
                        var mapId = _reader.GetMapId();

                        var songString = _reader.GetSongString();
                        var mapMd5 = _reader.GetMapMd5();
                        var mapStrings = $"songString: \"{songString}\" {Environment.NewLine}" +
                                         $"md5: \"{mapMd5}\" {Environment.NewLine}" +
                                         $"mapFolder: \"{_reader.GetMapFolderName()}\" {Environment.NewLine}" +
                                         $"fileName: \"{_reader.GetOsuFileName()}\" {Environment.NewLine}" +
                                         $"Retrys:{_reader.GetRetrys()}";

                        var mapData =
                            $"HP:{_reader.GetMapHp()} OD:{_reader.GetMapOd()}, CS:{_reader.GetMapCs()}, AR:{_reader.GetMapAr()}, setId:{_reader.GetMapSetId()}";

                        var status = _reader.GetCurrentStatus(out var num);
                        double hp = 0;
                        var playerName=string.Empty;
                        var hitErrorCount = -1;
                        if (status == OsuMemoryStatus.Playing)
                        {
                            playReseted = false;
                            _reader.GetPlayData(playContainer);
                            hp = _reader.ReadPlayerHp();
                            playerName = _reader.PlayerName();
                            hitErrorCount = _reader.HitErrors()?.Count ?? -2;
                        }
                        else if (!playReseted)
                        {
                            playReseted = true;
                            playContainer.Reset();
                        }

                        var playTime = _reader.ReadPlayTime();
                        var gameMode = _reader.ReadSongSelectGameMode();
                        var displayedPlayerHp = _reader.ReadDisplayedPlayerHp();
                        var mods = _reader.GetMods();

                        var tourneyIpcState = _reader.GetTourneyIpcState(out var tourneyIpcStateNumber);
                        var tourneyLeftStars = _reader.ReadTourneyLeftStars();
                        var tourneyRightStars = _reader.ReadTourneyRightStars();
                        var tourneyBO = _reader.ReadTourneyBO();
                        var tourneyWarmupState = _reader.ReadTourneyWarmupState();
                        var tourneyChatIsHidden = _reader.ReadTourneyChatIsHidden();

                        Invoke((MethodInvoker) (() =>
                        {
                            textBox_mapId.Text = mapId.ToString();
                            textBox_strings.Text = mapStrings;
                            textBox_time.Text = playTime.ToString();
                            textBox_mapData.Text = mapData;
                            textBox_Status.Text = status + " " + num + " " + gameMode;

                            textBox_CurrentPlayData.Text =
                                playContainer + $" time:{playTime}" + Environment.NewLine +
                                $"hp________: {hp:00.##} {Environment.NewLine}" +
                                $"displayedHp: {displayedPlayerHp:00.##} {Environment.NewLine}" +
                                $"mods:{mods} " +
                                $"PlayerName: {playerName}{Environment.NewLine}" +
                                $"HitErrorCount: {hitErrorCount}";

                            textBox_TourneyStuff.Text =
                                $"IPC-State: {tourneyIpcState} ({tourneyIpcStateNumber}) | BO {tourneyBO}{Environment.NewLine}" +
                                $"Stars: {tourneyLeftStars} | {tourneyRightStars}{Environment.NewLine}" +
                                $"Warmup-State: {(tourneyWarmupState ? "scores visible" : "warmup is enabled")}{Environment.NewLine}" +
                                $"Chat is hidden: {tourneyChatIsHidden}";
                        }));
                        Thread.Sleep(_readDelay);
                    }
                }
                catch (ThreadAbortException)
                {
                }
            });

            _thread.Start();
        }

        public class PlayContainerEx : PlayContainer
        {
            public override string ToString()
            {
                var nl = Environment.NewLine;
                return $"{C300}/{C100}/{C50}/{CMiss} : {CGeki},{CKatsu}" + nl +
                       $"acc:{Acc}, combo: {Combo}, maxCombo {MaxCombo}" + nl +
                       $"score: {Score}";
            }
        }
    }
}