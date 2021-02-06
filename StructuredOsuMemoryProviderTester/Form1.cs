using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using OsuMemoryDataProvider;
using OsuMemoryDataProvider.Models;

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
            var tbaseAddresses = new BaseAddresses();
            await Task.Run(async () =>
            {
                Stopwatch stopwatch;
                double readTimeMs, readTimeMsMin, readTimeMsMax;
                var playReseted = false;
                var baseAddresses = new BaseAddresses();
                while (true)
                {
                    if (cts.IsCancellationRequested)
                        return;

                    stopwatch = Stopwatch.StartNew();
                    _sreader.Read(baseAddresses.Beatmap);
                    _sreader.Read(baseAddresses.Skin);
                    _sreader.Read(baseAddresses.OsuStatus);
                    if (baseAddresses.OsuStatus.Status == OsuMemoryStatus.ResultsScreen)
                        _sreader.Read(baseAddresses.ResultsScreen);
                    if (baseAddresses.OsuStatus.Status == OsuMemoryStatus.Playing)
                        _sreader.Read(baseAddresses.Play);
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
                    Invoke((MethodInvoker)(() =>
                    {
                        textBox_readTime.Text = $"         ReadTimeMS: {readTimeMs}{Environment.NewLine}" +
                                                $" Min ReadTimeMS: {readTimeMsMin}{Environment.NewLine}" +
                                                $"Max ReadTimeMS: {readTimeMsMax}{Environment.NewLine}";
                        textBox_Data.Text = JsonConvert.SerializeObject(baseAddresses, Formatting.Indented);
                    }));

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
