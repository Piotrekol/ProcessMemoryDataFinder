using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OsuMemoryDataProvider;
using OsuMemoryDataProvider.OsuMemoryModels;

namespace OsuMemoryDataProviderTester
{
    public partial class Form1 : Form
    {
        private readonly string _osuWindowTitleHint;
        private int _readDelay = 33;
        private readonly object _minMaxLock = new object();
        private double _memoryReadTimeMin = double.PositiveInfinity;
        private double _memoryReadTimeMax = double.NegativeInfinity;
        private readonly ISet<string> _patternsToSkip = new HashSet<string>();
#pragma warning disable CS0618 // Type or member is obsolete
        private readonly IOsuMemoryReader _reader;
#pragma warning restore CS0618 // Type or member is obsolete
        private readonly StructuredOsuMemoryReader _sreader;
        private CancellationTokenSource cts = new CancellationTokenSource();
        public Form1(string osuWindowTitleHint)
        {
            _osuWindowTitleHint = osuWindowTitleHint;
            InitializeComponent();
#pragma warning disable CS0618 // Type or member is obsolete
            _reader = OsuMemoryReader.GetInstance(new("osu!", osuWindowTitleHint));
#pragma warning restore CS0618 // Type or member is obsolete
            _sreader = StructuredOsuMemoryReader.GetInstance(new("osu!", osuWindowTitleHint));
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
            cts.Cancel();
        }

        private void OnShown(object sender, EventArgs eventArgs)
        {
            if (!string.IsNullOrEmpty(_osuWindowTitleHint))
                Text += $": {_osuWindowTitleHint}";
            _ = Task.Run(async () =>
            {
                try
                {
                    Stopwatch stopwatch;
                    double readTimeMs, readTimeMsMin, readTimeMsMax;
                    var playContainer = new PlayContainerEx();
                    var playReseted = false;
                    var baseAddresses = new OsuBaseAddresses();
                    while (true)
                    {
                        if (cts.IsCancellationRequested)
                            return;

                        var patternsToRead = GetPatternsToRead();

                        stopwatch = Stopwatch.StartNew();

                        #region OsuBase

                        var mapId = -1;
                        var mapSetId = -1;
                        var songString = string.Empty;
                        var mapMd5 = string.Empty;
                        var mapFolderName = string.Empty;
                        var osuFileName = string.Empty;
                        var retrys = -1;
                        var gameMode = -1;
                        var mapData = string.Empty;
                        var status = OsuMemoryStatus.Unknown;
                        var statusNum = -1;
                        var playTime = -1;
                        if (patternsToRead.OsuBase)
                        {
                            mapId = _reader.GetMapId();
                            mapSetId = _reader.GetMapSetId();
                            songString = _reader.GetSongString();
                            mapMd5 = _reader.GetMapMd5();
                            mapFolderName = _reader.GetMapFolderName();
                            osuFileName = _reader.GetOsuFileName();
                            retrys = _reader.GetRetrys();
                            gameMode = _reader.ReadSongSelectGameMode();
                            mapData =
                                $"HP:{_reader.GetMapHp()} OD:{_reader.GetMapOd()}, CS:{_reader.GetMapCs()}, AR:{_reader.GetMapAr()}, setId:{_reader.GetMapSetId()}";
                            status = _reader.GetCurrentStatus(out statusNum);
                            playTime = _reader.ReadPlayTime();
                        }

                        #endregion

                        #region Mods

                        var mods = -1;
                        if (patternsToRead.Mods)
                        {
                            mods = _reader.GetMods();
                        }

                        #endregion

                        #region CurrentSkinData

                        var skinFolderName = string.Empty;
                        if (patternsToRead.CurrentSkinData)
                        {
                            skinFolderName = _reader.GetSkinFolderName();
                        }

                        #endregion

                        #region IsReplay

                        bool isReplay = false;
                        if (status == OsuMemoryStatus.Playing && patternsToRead.IsReplay)
                        {
                            isReplay = _reader.IsReplay();
                        }

                        #endregion

                        #region PlayContainer

                        double hp = 0;
                        var playerName = string.Empty;
                        var hitErrorCount = -1;
                        var playingMods = -1;
                        double displayedPlayerHp = 0;
                        int scoreV2 = -1;
                        if (status == OsuMemoryStatus.Playing && patternsToRead.PlayContainer)
                        {
                            playReseted = false;
                            _reader.GetPlayData(playContainer);
                            hp = _reader.ReadPlayerHp();
                            playerName = _reader.PlayerName();
                            hitErrorCount = _reader.HitErrors()?.Count ?? -2;
                            playingMods = _reader.GetPlayingMods();
                            displayedPlayerHp = _reader.ReadDisplayedPlayerHp();
                            scoreV2 = _reader.ReadScoreV2();
                        }
                        else if (!playReseted)
                        {
                            playReseted = true;
                            playContainer.Reset();
                        }

                        #endregion

                        #region TourneyBase

                        // TourneyBase
                        var tourneyIpcState = TourneyIpcState.Unknown;
                        var tourneyIpcStateNumber = -1;
                        var tourneyLeftStars = -1;
                        var tourneyRightStars = -1;
                        var tourneyBO = -1;
                        var tourneyStarsVisible = false;
                        var tourneyScoreVisible = false;
                        if (status == OsuMemoryStatus.Tourney && patternsToRead.TourneyBase)
                        {
                            tourneyIpcState = _reader.GetTourneyIpcState(out tourneyIpcStateNumber);
                            tourneyLeftStars = _reader.ReadTourneyLeftStars();
                            tourneyRightStars = _reader.ReadTourneyRightStars();
                            tourneyBO = _reader.ReadTourneyBO();
                            tourneyStarsVisible = _reader.ReadTourneyStarsVisible();
                            tourneyScoreVisible = _reader.ReadTourneyScoreVisible();
                        }

                        #endregion

                        stopwatch.Stop();

                        readTimeMs = stopwatch.ElapsedTicks / (double)TimeSpan.TicksPerMillisecond;
                        lock (_minMaxLock)
                        {
                            if (readTimeMs < _memoryReadTimeMin)
                                _memoryReadTimeMin = readTimeMs;
                            if (readTimeMs > _memoryReadTimeMax)
                                _memoryReadTimeMax = readTimeMs;
                            // copy value since we're inside lock
                            readTimeMsMin = _memoryReadTimeMin;
                            readTimeMsMax = _memoryReadTimeMax;
                        }

                        Invoke((MethodInvoker)(() =>
                       {
                           textBox_readTime.Text = $"         ReadTimeMS: {readTimeMs}{Environment.NewLine}" +
                                                   $" Min ReadTimeMS: {readTimeMsMin}{Environment.NewLine}" +
                                                   $"Max ReadTimeMS: {readTimeMsMax}{Environment.NewLine}";

                           textBox_mapId.Text = $"Id:{mapId} setId:{mapSetId}";
                           textBox_strings.Text = $"songString: \"{songString}\" {Environment.NewLine}" +
                                                  $"md5: \"{mapMd5}\" {Environment.NewLine}" +
                                                  $"mapFolder: \"{mapFolderName}\" {Environment.NewLine}" +
                                                  $"fileName: \"{osuFileName}\" {Environment.NewLine}" +
                                                  $"Retrys:{retrys} {Environment.NewLine}" +
                                                  $"mods:{(Mods)mods}({mods}) {Environment.NewLine}" +
                                                  $"SkinName: \"{skinFolderName}\"";
                           textBox_time.Text = playTime.ToString();
                           textBox_mapData.Text = mapData;
                           textBox_Status.Text = status + " " + statusNum + " " + gameMode;

                           textBox_CurrentPlayData.Text =
                               playContainer + Environment.NewLine +
                               $"scoreV2: {scoreV2} {Environment.NewLine}" +
                               $"IsReplay: {isReplay} {Environment.NewLine}" +
                               $"hp________: {hp:00.##} {Environment.NewLine}" +
                               $"displayedHp: {displayedPlayerHp:00.##} {Environment.NewLine}" +
                               $"playingMods:{(Mods)playingMods} ({playingMods}) " +
                               $"PlayerName: {playerName}{Environment.NewLine}" +
                               $"HitErrorCount: {hitErrorCount} ";

                           if (status == OsuMemoryStatus.Tourney)
                           {
                               textBox_TourneyStuff.Text =
                                   $"IPC-State: {tourneyIpcState} ({tourneyIpcStateNumber}) | BO {tourneyBO}{Environment.NewLine}" +
                                   $"Stars: {tourneyLeftStars} | {tourneyRightStars}{Environment.NewLine}" +
                                   $"Warmup/Stars State: {(tourneyStarsVisible ? "stars visible, warmup disabled" : "stars hidden, warmup enabled")}{Environment.NewLine}" +
                                   $"Score/Chat state: {(tourneyScoreVisible ? "chat hidden, score visible or no lobby joined" : "chat visible, score hidden")}{Environment.NewLine}";
                           }
                           else
                           {
                               textBox_TourneyStuff.Text = "no data since not in tourney mode";
                           }
                       }));
                        await Task.Delay(_readDelay);
                    }
                }
                catch (ThreadAbortException)
                {
                }
            });
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

        private void button_ResetReadTimeMinMax_Click(object sender, EventArgs e)
        {
            lock (_minMaxLock)
            {
                _memoryReadTimeMin = double.PositiveInfinity;
                _memoryReadTimeMax = double.NegativeInfinity;
            }
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            var cb = (CheckBox)sender;
            var name = (string)cb.Tag;
            var shouldInclude = cb.Checked;
            lock (_patternsToSkip)
            {
                // we store inverted state, easier since default is all on, so we can have a default of empty set to check :D
                if (shouldInclude)
                {
                    _patternsToSkip.Remove(name);
                }
                else
                {
                    _patternsToSkip.Add(name);
                }
            }
        }

        private PatternsToRead GetPatternsToRead()
        {
            lock (_patternsToSkip)
            {
                return new PatternsToRead(_patternsToSkip);
            }
        }
    }

    internal struct PatternsToRead
    {
        public readonly bool OsuBase;
        public readonly bool Mods;
        public readonly bool IsReplay;
        public readonly bool CurrentSkinData;
        public readonly bool TourneyBase;
        public readonly bool PlayContainer;

        public PatternsToRead(ISet<string> patternsToSkip)
        {
            OsuBase = !patternsToSkip.Contains(nameof(OsuBase));
            Mods = !patternsToSkip.Contains(nameof(Mods));
            IsReplay = !patternsToSkip.Contains(nameof(IsReplay));
            CurrentSkinData = !patternsToSkip.Contains(nameof(CurrentSkinData));
            TourneyBase = !patternsToSkip.Contains(nameof(TourneyBase));
            PlayContainer = !patternsToSkip.Contains(nameof(PlayContainer));
        }
    }
}