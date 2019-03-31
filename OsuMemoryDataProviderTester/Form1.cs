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
        private int _readDelay = 33;
        private Thread _thread;
        private readonly IOsuMemoryReader _reader;

        public Form1()
        {
            InitializeComponent();
            _reader = OsuMemoryReader.Instance;
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
                        if (status == OsuMemoryStatus.Playing)
                        {
                            playReseted = false;
                            _reader.GetPlayData(playContainer);
                            hp = _reader.ReadPlayerHp();
                        }
                        else if (!playReseted)
                        {
                            playReseted = true;
                            playContainer.Reset();
                        }

                        BeginInvoke((MethodInvoker) (() =>
                        {
                            textBox_mapId.Text = mapId.ToString();
                            textBox_strings.Text = mapStrings;
                            textBox_time.Text = _reader.ReadPlayTime().ToString();
                            textBox_mapData.Text = mapData;
                            textBox_Status.Text = status + " " + num + " " + _reader.ReadSongSelectGameMode();
                            textBox_CurrentPlayData.Text =
                                playContainer + $" time:{_reader.ReadPlayTime()}" + Environment.NewLine +
                                $"hp________: {hp:00.##} {Environment.NewLine}" +
                                $"displayedHp: {_reader.ReadDisplayedPlayerHp():00.##} {Environment.NewLine}";
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
                return $"{C300}/{C100}/{C50}/{CMiss}" + nl +
                       $"acc:{Acc}, combo: {Combo}, maxCombo {MaxCombo}" + nl +
                       $"score: {Score}";
            }
        }
    }
}