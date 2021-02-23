using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using OsuMemoryDataProvider;
using OsuMemoryDataProvider.Models;
using OsuMemoryDataProvider.Models.Memory;

namespace StructuredOsuMemoryProviderTester
{

    public partial class Form1 : Form
    {
        private readonly string _osuWindowTitleHint;
        private int _readDelay = 33;
        private readonly object _minMaxLock = new object();
        private double _memoryReadTimeMin = double.PositiveInfinity;
        private double _memoryReadTimeMax = double.NegativeInfinity;

        private readonly StructuredOsuMemoryReader _sreader;
        private CancellationTokenSource cts = new CancellationTokenSource();

        public Form1(string osuWindowTitleHint)
        {
            InitializeComponent();
            _sreader = StructuredOsuMemoryReader.Instance.GetInstanceForWindowTitleHint(osuWindowTitleHint);
            Shown += OnShown;
            Closing += OnClosing;
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            cts.Cancel();
        }

        private void numericUpDown_readDelay_ValueChanged(object sender, EventArgs e)
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


        private async void OnShown(object sender, EventArgs eventArgs)
        {
            if (!string.IsNullOrEmpty(_osuWindowTitleHint)) Text += $": {_osuWindowTitleHint}";

            await Task.Run(async () =>
            {
                Stopwatch stopwatch;
                double readTimeMs, readTimeMsMin, readTimeMsMax;
                _sreader.WithTimes = true;
                var readUsingProperty= true;
                var baseAddresses = new BaseAddresses();
                while (true)
                {
                    if (cts.IsCancellationRequested)
                        return;

                    stopwatch = Stopwatch.StartNew();
                    if (readUsingProperty)
                    {
                        baseAddresses.Beatmap.Id = (int)_sreader.ReadProperty(baseAddresses.Beatmap, nameof(CurrentBeatmap.Id));
                        baseAddresses.Beatmap.SetId = (int)_sreader.ReadProperty(baseAddresses.Beatmap, nameof(CurrentBeatmap.SetId));
                        baseAddresses.Beatmap.MapString = (string)_sreader.ReadProperty(baseAddresses.Beatmap, nameof(CurrentBeatmap.MapString));
                        baseAddresses.Beatmap.FolderName = (string)_sreader.ReadProperty(baseAddresses.Beatmap, nameof(CurrentBeatmap.FolderName));
                        baseAddresses.Beatmap.OsuFileName = (string)_sreader.ReadProperty(baseAddresses.Beatmap, nameof(CurrentBeatmap.OsuFileName));
                        baseAddresses.Beatmap.Md5 = (string)_sreader.ReadProperty(baseAddresses.Beatmap, nameof(CurrentBeatmap.Md5));
                        baseAddresses.Beatmap.Ar = (float)_sreader.ReadProperty(baseAddresses.Beatmap, nameof(CurrentBeatmap.Ar));
                        baseAddresses.Beatmap.Cs = (float)_sreader.ReadProperty(baseAddresses.Beatmap, nameof(CurrentBeatmap.Cs));
                        baseAddresses.Beatmap.Hp = (float)_sreader.ReadProperty(baseAddresses.Beatmap, nameof(CurrentBeatmap.Hp));
                        baseAddresses.Beatmap.Od = (float)_sreader.ReadProperty(baseAddresses.Beatmap, nameof(CurrentBeatmap.Od));
                        baseAddresses.Skin.Folder = (string)_sreader.ReadProperty(baseAddresses.Skin, nameof(Skin.Folder));
                        baseAddresses.MiscData.RawStatus = (int)_sreader.ReadProperty(baseAddresses.MiscData, nameof(Misc.RawStatus));
                        baseAddresses.MiscData.GameMode = (int)_sreader.ReadProperty(baseAddresses.MiscData, nameof(Misc.GameMode));
                        baseAddresses.MiscData.Retries = (int)_sreader.ReadProperty(baseAddresses.MiscData, nameof(Misc.Retries));
                        baseAddresses.MiscData.AudioTime = (int)_sreader.ReadProperty(baseAddresses.MiscData, nameof(Misc.AudioTime));
                        baseAddresses.MiscData.Mods = (int)_sreader.ReadProperty(baseAddresses.MiscData, nameof(Misc.Mods));
                        baseAddresses.MiscData.IsReplay = (bool)_sreader.ReadProperty(baseAddresses.MiscData, nameof(Misc.IsReplay));
                    }
                    else
                    {
                        _sreader.Read(baseAddresses.Beatmap);
                        _sreader.Read(baseAddresses.Skin);
                        _sreader.Read(baseAddresses.MiscData);
                    }
                    if (baseAddresses.MiscData.OsuStatus == OsuMemoryStatus.ResultsScreen)
                        _sreader.Read(baseAddresses.ResultsScreen);
                    if (baseAddresses.MiscData.OsuStatus == OsuMemoryStatus.Playing)
                    {
                        _sreader.Read(baseAddresses.Player);
                        //TODO: flag needed for single/multi player detection (should be read once per play in singleplayer)
                        _sreader.Read(baseAddresses.LeaderBoard);
                        if (readUsingProperty)
                        {
                            var mods = (Mods)_sreader.ReadProperty(baseAddresses.Player, nameof(Player.Mods));
                            var HitErrors = (List<int>)_sreader.ReadProperty(baseAddresses.Player, nameof(Player.HitErrors));
                        }
                    }

                    if (baseAddresses.Player?.HitErrors != null)
                    {
                        var hitErrorsCount = baseAddresses.Player.HitErrors.Count;
                        baseAddresses.Player.HitErrors.Clear();
                        baseAddresses.Player.HitErrors.Add(hitErrorsCount);
                    }

                    stopwatch.Stop();

                    readTimeMs = stopwatch.ElapsedTicks / (double)TimeSpan.TicksPerMillisecond;
                    lock (_minMaxLock)
                    {
                        if (readTimeMs < _memoryReadTimeMin) _memoryReadTimeMin = readTimeMs;
                        if (readTimeMs > _memoryReadTimeMax) _memoryReadTimeMax = readTimeMs;
                        // copy value since we're inside lock
                        readTimeMsMin = _memoryReadTimeMin;
                        readTimeMsMax = _memoryReadTimeMax;
                    }

                    try
                    {
                        Invoke((MethodInvoker) (() =>
                        {
                            textBox_readTime.Text = $"         ReadTimeMS: {readTimeMs}{Environment.NewLine}" +
                                                    $" Min ReadTimeMS: {readTimeMsMin}{Environment.NewLine}" +
                                                    $"Max ReadTimeMS: {readTimeMsMax}{Environment.NewLine}";
                            textBox_Data.Text = JsonConvert.SerializeObject(baseAddresses, Formatting.Indented);
                            textBox_ReadTimes.Text =
                                JsonConvert.SerializeObject(_sreader.ReadTimes, Formatting.Indented);
                        }));
                    }
                    catch (ObjectDisposedException)
                    {
                        return;
                    }

                    _sreader.ReadTimes.Clear();
                    await Task.Delay(_readDelay);
                }
            }, cts.Token);
        }

        private void button_ResetReadTimeMinMax_Click(object sender, EventArgs e)
        {
            lock (_minMaxLock)
            {
                _memoryReadTimeMin = double.PositiveInfinity;
                _memoryReadTimeMax = double.NegativeInfinity;
            }
        }
    }
}
