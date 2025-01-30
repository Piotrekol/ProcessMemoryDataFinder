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
using OsuMemoryDataProvider.OsuMemoryModels;
using OsuMemoryDataProvider.OsuMemoryModels.Abstract;
using OsuMemoryDataProvider.OsuMemoryModels.Direct;

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
            _sreader = StructuredOsuMemoryReader.GetInstance(new("osu!", osuWindowTitleHint));
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

        private T ReadProperty<T>(object readObj, string propName, T defaultValue = default) where T : struct
        {
            if (_sreader.TryReadProperty(readObj, propName, out var readResult))
                return (T)readResult;

            return defaultValue;
        }

        private T ReadClassProperty<T>(object readObj, string propName, T defaultValue = default) where T : class
        {
            if (_sreader.TryReadProperty(readObj, propName, out var readResult))
                return (T)readResult;

            return defaultValue;
        }

        private int ReadInt(object readObj, string propName)
            => ReadProperty<int>(readObj, propName, -5);
        private short ReadShort(object readObj, string propName)
            => ReadProperty<short>(readObj, propName, -5);

        private float ReadFloat(object readObj, string propName)
            => ReadProperty<float>(readObj, propName, -5f);

        private string ReadString(object readObj, string propName)
            => ReadClassProperty<string>(readObj, propName, "INVALID READ");

        private async void OnShown(object sender, EventArgs eventArgs)
        {
            if (!string.IsNullOrEmpty(_osuWindowTitleHint)) Text += $": {_osuWindowTitleHint}";
            Text += $" ({(Environment.Is64BitProcess ? "x64" : "x86")})";
            _sreader.InvalidRead += SreaderOnInvalidRead;
            await Task.Run(async () =>
            {
                Stopwatch stopwatch;
                double readTimeMs, readTimeMsMin, readTimeMsMax;
                _sreader.WithTimes = true;
                var readUsingProperty = false;
                var baseAddresses = new OsuBaseAddresses();
                while (true)
                {
                    if (cts.IsCancellationRequested)
                        return;

                    if (!_sreader.CanRead)
                    {
                        Invoke((MethodInvoker)(() =>
                        {
                            textBox_Data.Text = "osu! process not found";
                        }));
                        await Task.Delay(_readDelay);
                        continue;
                    }

                    stopwatch = Stopwatch.StartNew();
                    if (readUsingProperty)
                    {
                        baseAddresses.Beatmap.Id = ReadInt(baseAddresses.Beatmap, nameof(CurrentBeatmap.Id));
                        baseAddresses.Beatmap.SetId = ReadInt(baseAddresses.Beatmap, nameof(CurrentBeatmap.SetId));
                        baseAddresses.Beatmap.MapString = ReadString(baseAddresses.Beatmap, nameof(CurrentBeatmap.MapString));
                        baseAddresses.Beatmap.FolderName = ReadString(baseAddresses.Beatmap, nameof(CurrentBeatmap.FolderName));
                        baseAddresses.Beatmap.OsuFileName = ReadString(baseAddresses.Beatmap, nameof(CurrentBeatmap.OsuFileName));
                        baseAddresses.Beatmap.Md5 = ReadString(baseAddresses.Beatmap, nameof(CurrentBeatmap.Md5));
                        baseAddresses.Beatmap.Ar = ReadFloat(baseAddresses.Beatmap, nameof(CurrentBeatmap.Ar));
                        baseAddresses.Beatmap.Cs = ReadFloat(baseAddresses.Beatmap, nameof(CurrentBeatmap.Cs));
                        baseAddresses.Beatmap.Hp = ReadFloat(baseAddresses.Beatmap, nameof(CurrentBeatmap.Hp));
                        baseAddresses.Beatmap.Od = ReadFloat(baseAddresses.Beatmap, nameof(CurrentBeatmap.Od));
                        baseAddresses.Beatmap.Status = ReadShort(baseAddresses.Beatmap, nameof(CurrentBeatmap.Status));
                        baseAddresses.Skin.Folder = ReadString(baseAddresses.Skin, nameof(Skin.Folder));
                        baseAddresses.GeneralData.RawStatus = ReadInt(baseAddresses.GeneralData, nameof(GeneralData.RawStatus));
                        baseAddresses.GeneralData.GameMode = ReadInt(baseAddresses.GeneralData, nameof(GeneralData.GameMode));
                        baseAddresses.GeneralData.Retries = ReadInt(baseAddresses.GeneralData, nameof(GeneralData.Retries));
                        baseAddresses.GeneralData.AudioTime = ReadInt(baseAddresses.GeneralData, nameof(GeneralData.AudioTime));
                        baseAddresses.GeneralData.Mods = ReadInt(baseAddresses.GeneralData, nameof(GeneralData.Mods));
                    }
                    else
                    {
                        _sreader.TryRead(baseAddresses.Beatmap);
                        _sreader.TryRead(baseAddresses.Skin);
                        _sreader.TryRead(baseAddresses.GeneralData);
                        _sreader.TryRead(baseAddresses.BanchoUser);
                    }

                    if (baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.SongSelect)
                        _sreader.TryRead(baseAddresses.SongSelectionScores);
                    else
                        baseAddresses.SongSelectionScores.Scores.Clear();

                    if (baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.ResultsScreen)
                        _sreader.TryRead(baseAddresses.ResultsScreen);

                    if (baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.Playing)
                    {
                        _sreader.TryRead(baseAddresses.Player);
                        //TODO: flag needed for single/multi player detection (should be read once per play in singleplayer)
                        _sreader.TryRead(baseAddresses.LeaderBoard);
                        _sreader.TryRead(baseAddresses.KeyOverlay);
                        if (readUsingProperty)
                        {
                            //Testing reading of reference types(other than string)
                            _sreader.TryReadProperty(baseAddresses.Player, nameof(Player.Mods), out var dummyResult);
                        }
                    }
                    else
                    {
                        baseAddresses.LeaderBoard.Players.Clear();
                    }

                    var hitErrors = baseAddresses.Player?.HitErrors;
                    if (hitErrors != null)
                    {
                        var hitErrorsCount = hitErrors.Count;
                        hitErrors.Clear();
                        hitErrors.Add(hitErrorsCount);
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
                        Invoke((MethodInvoker)(() =>
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

        private void SreaderOnInvalidRead(object sender, (object readObject, string propPath) e)
        {
            try
            {
                if (InvokeRequired)
                {
                    //Async call to not impact memory read times(too much)
                    BeginInvoke((MethodInvoker)(() => SreaderOnInvalidRead(sender, e)));
                    return;
                }

                listBox_logs.Items.Add($"{DateTime.Now:T} Error reading {e.propPath}{Environment.NewLine}");
                if (listBox_logs.Items.Count > 500)
                    listBox_logs.Items.RemoveAt(0);

                listBox_logs.SelectedIndex = listBox_logs.Items.Count - 1;
                listBox_logs.ClearSelected();
            }
            catch (ObjectDisposedException)
            {

            }
        }

        private void button_ResetReadTimeMinMax_Click(object sender, EventArgs e)
        {
            lock (_minMaxLock)
            {
                _memoryReadTimeMin = double.PositiveInfinity;
                _memoryReadTimeMax = double.NegativeInfinity;
            }

            listBox_logs.Items.Clear();
        }
    }
}
