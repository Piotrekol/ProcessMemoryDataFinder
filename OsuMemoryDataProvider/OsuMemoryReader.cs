//#define MemoryTimes

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ProcessMemoryDataFinder;
using ProcessMemoryDataFinder.API;

namespace OsuMemoryDataProvider
{
    [Obsolete("This version of reader is not updated anymore with new values. Use StructuredOsuMemoryReader in new implementations.", false)]
    public class OsuMemoryReader : SigMemoryReader, IOsuMemoryReader
    {
        protected readonly object _lockingObject = new object();

        /// <summary>
        ///     It is strongly encouraged to use single <see cref="OsuMemoryReader" /> instance in order to not have to duplicate
        ///     find-signature-location work
        /// </summary>
        public static IOsuMemoryReader Instance => instance ??= new OsuMemoryReader(new("osu!"));

        private static IOsuMemoryReader instance;
        private static readonly ConcurrentDictionary<ProcessTargetOptions, IOsuMemoryReader> Instances =
            new ConcurrentDictionary<ProcessTargetOptions, IOsuMemoryReader>();

        public static IOsuMemoryReader GetInstance(ProcessTargetOptions processTargetOptions)
        {
            if (processTargetOptions is null)
            {
                return Instance;
            }

            return Instances.GetOrAdd(processTargetOptions, s => new OsuMemoryReader(processTargetOptions));
        }

        public OsuMemoryReader(ProcessTargetOptions processTargetOptions) : base(processTargetOptions)
        {
            ObjectReader.IntPtrSize = 4;
            CreateSignatures();
        }

        internal void CreateSignatures()
        {
            Signatures.Add((int)SignatureNames.OsuBase, new SigEx
            {
                Name = "OsuBase",
                Pattern = PatternHelpers.UnpackStr("F80174048365"),
                UseMask = false
            });

            Signatures.Add((int)SignatureNames.GameMode, new SigEx
            {
                ParentSig = Signatures[(int)SignatureNames.OsuBase],
                Offset = -51,
                PointerOffsets = { 0 }
            });

            Signatures.Add((int)SignatureNames.Retrys, new SigEx
            {
                ParentSig = Signatures[(int)SignatureNames.OsuBase],
                Offset = -51,
                PointerOffsets = { 8 }
            });

            CreateBeatmapDataSignatures();

            Signatures.Add((int)SignatureNames.OsuStatus, new SigEx
            {
                Name = "OsuStatus",
                ParentSig = Signatures[(int)SignatureNames.OsuBase],
                Offset = -60,
                PointerOffsets = { 0 }
            });
            Signatures.Add((int)SignatureNames.PlayTime, new SigEx
            {
                Name = "PlayTime",
                ParentSig = Signatures[(int)SignatureNames.OsuBase],
                Offset = 100,
                PointerOffsets = { -16 }
            });
            Signatures[(int)SignatureNames.Mods] = new SigEx
            {
                Name = "mods",
                Pattern = PatternHelpers.UnpackStr("C8FF0000000000810D0000000000080000"),
                Mask = "xx?????xx????xxxx",
                Offset = 9,
                PointerOffsets = { 0 },
                UseMask = true,
            };

            Signatures[(int)SignatureNames.IsReplay] = new SigEx
            {
                Name = "IsReplay",
                Pattern = PatternHelpers.UnpackStr("8BFAB801000000"),
                Mask = "xxxxxxx",
                Offset = 0x2A,
                PointerOffsets = { 0 },
                UseMask = true,
            };

            CreateSkinSignatures();
            CreatePlaySignatures();
            CreateTourneySignatures();
        }

        private void CreateSkinSignatures()
        {
            Signatures[(int)SignatureNames.CurrentSkinData] = new SigEx
            {
                Name = "currentSkinData",
                Pattern = PatternHelpers.UnpackStr("75218b1d"),
                UseMask = false,
                Offset = 4,
                PointerOffsets = { 0, 0 }
            };
            Signatures[(int)SignatureNames.CurrentSkinFolder] = new SigEx
            {
                Name = "currentSkinFolder",
                ParentSig = Signatures[(int)SignatureNames.CurrentSkinData],
                Offset = 68
            };
        }

        private void CreateBeatmapDataSignatures()
        {
            Signatures.Add((int)SignatureNames.CurrentBeatmapData, new SigEx
            {
                Name = "CurrentBeatmapData",
                ParentSig = Signatures[(int)SignatureNames.OsuBase],
                Offset = -12,
                PointerOffsets = { 0 },
                UseMask = false
            });
            Signatures.Add((int)SignatureNames.MapId, new SigEx
            {
                //int
                ParentSig = Signatures[(int)SignatureNames.CurrentBeatmapData],
                PointerOffsets = { 0xC8 }
            });
            Signatures.Add((int)SignatureNames.MapSetId, new SigEx
            {
                //int
                ParentSig = Signatures[(int)SignatureNames.CurrentBeatmapData],
                PointerOffsets = { 0xCC }
            });
            Signatures.Add((int)SignatureNames.MapString, new SigEx
            {
                //string
                ParentSig = Signatures[(int)SignatureNames.CurrentBeatmapData],
                PointerOffsets = { 0x80 }
            });
            Signatures.Add((int)SignatureNames.MapFolderName, new SigEx
            {
                //string
                ParentSig = Signatures[(int)SignatureNames.CurrentBeatmapData],
                PointerOffsets = { 0x78 }
            });
            Signatures.Add((int)SignatureNames.MapOsuFileName, new SigEx
            {
                //string
                ParentSig = Signatures[(int)SignatureNames.CurrentBeatmapData],
                PointerOffsets = { 0x90 }
            });
            Signatures.Add((int)SignatureNames.MapMd5, new SigEx
            {
                //string
                ParentSig = Signatures[(int)SignatureNames.CurrentBeatmapData],
                PointerOffsets = { 0x6C }
            });
            Signatures.Add((int)SignatureNames.MapAr, new SigEx
            {
                //float
                ParentSig = Signatures[(int)SignatureNames.CurrentBeatmapData],
                PointerOffsets = { 44 }
            });
            Signatures.Add((int)SignatureNames.MapCs, new SigEx
            {
                ParentSig = Signatures[(int)SignatureNames.CurrentBeatmapData],
                PointerOffsets = { 48 }
            });
            Signatures.Add((int)SignatureNames.MapHp, new SigEx
            {
                ParentSig = Signatures[(int)SignatureNames.CurrentBeatmapData],
                PointerOffsets = { 52 }
            });
            Signatures.Add((int)SignatureNames.MapOd, new SigEx
            {
                ParentSig = Signatures[(int)SignatureNames.CurrentBeatmapData],
                PointerOffsets = { 56 }
            });
        }

        private void CreateTourneySignatures()
        {
            Signatures.Add((int)SignatureNames.TourneyBase, new SigEx
            {
                Name = "TourneyBase",
                Pattern = PatternHelpers.UnpackStr("7D15A10000000085C0"),
                Mask = "xxx????xx",
                Offset = -0xB
            });
            Signatures.Add((int)SignatureNames.TourneyIpcState, new SigEx
            {
                ParentSig = Signatures[(int)SignatureNames.TourneyBase],
                PointerOffsets = { 4, 0x54 }
            });
            Signatures.Add((int)SignatureNames.TourneyLeftStars, new SigEx
            {
                ParentSig = Signatures[(int)SignatureNames.TourneyBase],
                PointerOffsets = { 4, 0x1C, 0x2C }
            });
            Signatures.Add((int)SignatureNames.TourneyRightStars, new SigEx
            {
                ParentSig = Signatures[(int)SignatureNames.TourneyBase],
                PointerOffsets = { 4, 0x20, 0x2C }
            });
            Signatures.Add((int)SignatureNames.TourneyBO, new SigEx
            {
                ParentSig = Signatures[(int)SignatureNames.TourneyBase],
                PointerOffsets = { 4, 0x20, 0x30 }
            });
            Signatures.Add((int)SignatureNames.TourneyStarsVisible, new SigEx
            {
                ParentSig = Signatures[(int)SignatureNames.TourneyBase],
                PointerOffsets = { 4, 0x20, 0x38 }
            });
            Signatures.Add((int)SignatureNames.TourneyScoreVisible, new SigEx
            {
                ParentSig = Signatures[(int)SignatureNames.TourneyBase],
                PointerOffsets = { 4, 0x20, 0x39 }
            });
        }

        private void CreatePlaySignatures()
        {
            Signatures.Add((int)SignatureNames.CurrentRuleset, new SigEx
            {
                Name = "CurrentRuleset",
                Pattern = PatternHelpers.UnpackStr("C7864801000001000000A1"),
                Offset = 0xB,
                PointerOffsets = { 0x4 },
                UseMask = false
            });

            Signatures.Add((int)SignatureNames.PlayContainer, new SigEx
            {
                //avaliable only when playing;
                //need to reset on each play
                Name = "PlayContainer",
                ParentSig = Signatures[(int)SignatureNames.CurrentRuleset],
                Offset = 0,
                PointerOffsets = { 0x68 },
                UseMask = false
            });

            Signatures.Add((int)SignatureNames.PlayingMods, new SigEx
            {
                //Complex - 2 xored ints
                ParentSig = Signatures[(int)SignatureNames.PlayContainer],
                PointerOffsets = { 56, 28 }
            });
            Signatures.Add((int)SignatureNames.PlayerName, new SigEx
            {
                //char[]
                ParentSig = Signatures[(int)SignatureNames.PlayContainer],
                PointerOffsets = { 56, 40 }
            });
            Signatures.Add((int)SignatureNames.HitErrors, new SigEx
            {
                //int[]
                ParentSig = Signatures[(int)SignatureNames.PlayContainer],
                PointerOffsets = { 56, 56 }
            });
            Signatures.Add((int)SignatureNames.Combo, new SigEx
            {
                //ushort
                ParentSig = Signatures[(int)SignatureNames.PlayContainer],
                PointerOffsets = { 56, 148 }
            });
            Signatures.Add((int)SignatureNames.ComboMax, new SigEx
            {
                //ushort
                ParentSig = Signatures[(int)SignatureNames.PlayContainer],
                PointerOffsets = { 56, 104 }
            });
            Signatures.Add((int)SignatureNames.Hit100c, new SigEx
            {
                //ushort
                ParentSig = Signatures[(int)SignatureNames.PlayContainer],
                PointerOffsets = { 56, 136 }
            });
            Signatures.Add((int)SignatureNames.Hit300c, new SigEx
            {
                //ushort
                ParentSig = Signatures[(int)SignatureNames.PlayContainer],
                PointerOffsets = { 56, 138 }
            });
            Signatures.Add((int)SignatureNames.Hit50c, new SigEx
            {
                //ushort
                ParentSig = Signatures[(int)SignatureNames.PlayContainer],
                PointerOffsets = { 56, 140 }
            });
            Signatures.Add((int)SignatureNames.HitGeki, new SigEx
            {
                //ushort
                ParentSig = Signatures[(int)SignatureNames.PlayContainer],
                PointerOffsets = { 56, 142 }
            });
            Signatures.Add((int)SignatureNames.HitKatsu, new SigEx
            {
                //ushort
                ParentSig = Signatures[(int)SignatureNames.PlayContainer],
                PointerOffsets = { 56, 144 }
            });
            Signatures.Add((int)SignatureNames.HitMissc, new SigEx
            {
                //ushort
                ParentSig = Signatures[(int)SignatureNames.PlayContainer],
                PointerOffsets = { 56, 146 }
            });
            Signatures.Add((int)SignatureNames.Score, new SigEx
            {
                //int
                ParentSig = Signatures[(int)SignatureNames.CurrentRuleset],
                PointerOffsets = { 0x100 }
            });
            Signatures.Add((int)SignatureNames.PlayingGameMode, new SigEx
            {
                //int
                ParentSig = Signatures[(int)SignatureNames.PlayContainer],
                PointerOffsets = { 56, 104 }
            });
            Signatures.Add((int)SignatureNames.Acc, new SigEx
            {
                //double
                ParentSig = Signatures[(int)SignatureNames.PlayContainer],
                PointerOffsets = { 72, 20 }
            });
            Signatures.Add((int)SignatureNames.PlayerHp, new SigEx
            {
                //ushort
                ParentSig = Signatures[(int)SignatureNames.PlayContainer],
                PointerOffsets = { 64, 28 }
            });
            Signatures.Add((int)SignatureNames.PlayerHpSmoothed, new SigEx
            {
                //ushort
                ParentSig = Signatures[(int)SignatureNames.PlayContainer],
                PointerOffsets = { 64, 20 }
            });

            Signatures.Add((int)SignatureNames.ScoreV2, new SigEx
            {
                //int
                ParentSig = Signatures[(int)SignatureNames.CurrentRuleset],
                PointerOffsets = { 0x100 }
            });
        }

        #region IOsuMemoryReader members

        /// <summary>
        ///     Fills all fields of PlayContainer with data read from osu! memory.
        /// </summary>
        /// <param name="playContainer">Initalized object to fill with data</param>
        public void GetPlayData(PlayContainer playContainer)
        {
            lock (_lockingObject)
            {
                playContainer.Acc = ReadAcc();
                playContainer.Hp = ReadPlayerHp();
                playContainer.C300 = ReadHit300();
                playContainer.C100 = ReadHit100();
                playContainer.C50 = ReadHit50();
                playContainer.CGeki = ReadHitGeki();
                playContainer.CKatsu = ReadHitKatsu();
                playContainer.CMiss = ReadHitMiss();
                playContainer.Combo = ReadCombo();
                playContainer.MaxCombo = ReadComboMax();
                playContainer.Score = ReadScore();
            }
        }

        public int GetMods()
        {
            return GetInt((int)SignatureNames.Mods);
        }

        public int GetPlayingMods()
        {
            lock (_lockingObject)
            {
#if DEBUG && MemoryTimes
                LogCaller("Start");
#endif
                ResetPointer((int)SignatureNames.PlayingMods);
                var pointer = GetPointer((int)SignatureNames.PlayingMods);
                var data1 = ReadData(pointer + 8, 4);
                var data2 = ReadData(pointer + 12, 4);
#if DEBUG && MemoryTimes
                LogCaller("End");
#endif
                if (data1 != null && data2 != null)
                {
                    var num1 = BitConverter.ToInt32(data1, 0);
                    var num2 = BitConverter.ToInt32(data2, 0);
                    return num1 ^ num2;
                }

                return -1;
            }
        }

        public string GetSkinFolderName()
        {
            return GetString((int)SignatureNames.CurrentSkinFolder);
        }
        public List<int> HitErrors()
        {
            lock (_lockingObject)
            {
                ResetPointer((int)SignatureNames.HitErrors);
                return GetIntList((int)SignatureNames.HitErrors);
            }
        }

        public string PlayerName()
        {
            return GetString((int)SignatureNames.PlayerName);
        }

        public int GetMapId()
        {
            return GetInt((int)SignatureNames.MapId);
        }

        public float GetMapAr()
        {
            return GetFloat((int)SignatureNames.MapAr);
        }

        public float GetMapCs()
        {
            return GetFloat((int)SignatureNames.MapCs);
        }

        public float GetMapHp()
        {
            return GetFloat((int)SignatureNames.MapHp);
        }

        public float GetMapOd()
        {
            return GetFloat((int)SignatureNames.MapOd);
        }

        public int GetMapSetId()
        {
            return GetInt((int)SignatureNames.MapSetId);
        }

        public string GetSongString()
        {
            return GetString((int)SignatureNames.MapString);
        }

        public string GetMapMd5()
        {
            return GetString((int)SignatureNames.MapMd5);
        }

        public string GetOsuFileName()
        {
            return GetString((int)SignatureNames.MapOsuFileName);
        }

        public string GetMapFolderName()
        {
            return GetString((int)SignatureNames.MapFolderName);
        }

        public int ReadPlayTime()
        {
            return GetInt((int)SignatureNames.PlayTime);
        }

        public ushort ReadHit300()
        {
            return GetUShort((int)SignatureNames.Hit300c);
        }

        public ushort ReadHit100()
        {
            return GetUShort((int)SignatureNames.Hit100c);
        }

        public ushort ReadHit50()
        {
            return GetUShort((int)SignatureNames.Hit50c);
        }

        public ushort ReadHitGeki()
        {
            return GetUShort((int)SignatureNames.HitGeki);
        }

        public ushort ReadHitKatsu()
        {
            return GetUShort((int)SignatureNames.HitKatsu);
        }

        public ushort ReadHitMiss()
        {
            return GetUShort((int)SignatureNames.HitMissc);
        }

        public double ReadAcc()
        {
            return GetDouble((int)SignatureNames.Acc);
        }

        public ushort ReadCombo()
        {
            return GetUShort((int)SignatureNames.Combo);
        }

        public int ReadPlayedGameMode()
        {
            return GetInt((int)SignatureNames.PlayingGameMode);
        }

        public int ReadSongSelectGameMode()
        {
            return GetInt((int)SignatureNames.GameMode);
        }

        public int GetRetrys()
        {
            return GetInt((int)SignatureNames.Retrys);
        }

        public int ReadScore()
        {
            return GetInt((int)SignatureNames.Score);
        }

        public bool IsReplay()
        {
            return GetBoolean((int)SignatureNames.IsReplay);
        }

        public int ReadScoreV2()
        {
            return GetInt((int)SignatureNames.ScoreV2);
        }

        public ushort ReadComboMax()
        {
            return GetUShort((int)SignatureNames.ComboMax);
        }

        public double ReadPlayerHp()
        {
            return GetDouble((int)SignatureNames.PlayerHp);
        }

        public double ReadDisplayedPlayerHp()
        {
            return GetDouble((int)SignatureNames.PlayerHpSmoothed);
        }

        public TourneyIpcState GetTourneyIpcState(out int ipcStateNumber)
        {
#if DEBUG && MemoryTimes
            LogCaller("Start");
#endif
            int num;
            lock (_lockingObject)
            {
                num = GetInt((int)SignatureNames.TourneyIpcState);
            }
#if DEBUG && MemoryTimes
            LogCaller("End");
#endif

            ipcStateNumber = num;
            if (Enum.IsDefined(typeof(TourneyIpcState), num))
            {
                return (TourneyIpcState)num;
            }

            return TourneyIpcState.Unknown;
        }

        public int ReadTourneyLeftStars()
        {
            return GetInt((int)SignatureNames.TourneyLeftStars);
        }

        public int ReadTourneyRightStars()
        {
            return GetInt((int)SignatureNames.TourneyRightStars);
        }

        public int ReadTourneyBO()
        {
            return GetInt((int)SignatureNames.TourneyBO);
        }

        public bool ReadTourneyStarsVisible()
        {
            return GetByte((int)SignatureNames.TourneyStarsVisible) != 0;
        }

        public bool ReadTourneyScoreVisible()
        {
            return GetByte((int)SignatureNames.TourneyScoreVisible) != 0;
        }

        /// <summary>
        /// Gets the current osu! status.
        /// </summary>
        /// <param name="statusNumber">Use this number whenever <see cref="OsuMemoryStatus.Unknown"/> is returned</param>
        /// <returns></returns>
        public OsuMemoryStatus GetCurrentStatus(out int statusNumber)
        {
#if DEBUG && MemoryTimes
            LogCaller("Start");
#endif
            int num;
            lock (_lockingObject)
            {
                num = GetInt((int)SignatureNames.OsuStatus);
            }
#if DEBUG && MemoryTimes
            LogCaller("End");
#endif

            statusNumber = num;
            if (Enum.IsDefined(typeof(OsuMemoryStatus), num))
            {
                return (OsuMemoryStatus)num;
            }

            return OsuMemoryStatus.Unknown;
        }

        #endregion

        protected override bool GetBoolean(int signatureId)
        {
            lock (_lockingObject)
            {
#if DEBUG && MemoryTimes
                LogCaller("Start");
#endif
                ResetPointer(signatureId);
                return base.GetBoolean(signatureId);
#if DEBUG && MemoryTimes
                LogCaller("End");
#endif
            }
        }

        protected override int GetInt(int signatureId)
        {
            lock (_lockingObject)
            {
#if DEBUG && MemoryTimes
                LogCaller("Start");
#endif
                ResetPointer(signatureId);
                return base.GetInt(signatureId);
#if DEBUG && MemoryTimes
                LogCaller("End");
#endif
            }
        }

        protected override float GetFloat(int signatureId)
        {
            lock (_lockingObject)
            {
#if DEBUG && MemoryTimes
                LogCaller("Start");
#endif
                ResetPointer(signatureId);
                return base.GetFloat(signatureId);
#if DEBUG && MemoryTimes
                LogCaller("End");
#endif
            }
        }

        protected override string GetString(int signatureId)
        {
            lock (_lockingObject)
            {
#if DEBUG && MemoryTimes
                LogCaller("Start");
#endif
                ResetPointer(signatureId);
                return base.GetString(signatureId);
#if DEBUG && MemoryTimes
                LogCaller("End");
#endif
            }
        }

        protected override ushort GetUShort(int signatureId)
        {
            lock (_lockingObject)
            {
#if DEBUG && MemoryTimes
                LogCaller("Start");
#endif
                ResetPointer(signatureId);
                return base.GetUShort(signatureId);
#if DEBUG && MemoryTimes
                LogCaller("End");
#endif
            }
        }

        protected override byte GetByte(int signatureId)
        {
            lock (_lockingObject)
            {
#if DEBUG && MemoryTimes
                LogCaller("Start");
#endif
                ResetPointer(signatureId);
                return base.GetByte(signatureId);
#if DEBUG && MemoryTimes
                LogCaller("End");
#endif
            }
        }

        protected override double GetDouble(int signatureId)
        {
            lock (_lockingObject)
            {
#if DEBUG && MemoryTimes
                LogCaller("Start");
#endif
                ResetPointer(signatureId);
                return base.GetDouble(signatureId);
#if DEBUG && MemoryTimes
                LogCaller("End");
#endif
            }
        }

#if DEBUG && MemoryTimes
        private void LogCaller(string prependText)
        {
            StackTrace stackTrace = new StackTrace();

            // Get calling method name
            Console.WriteLine(DateTime.Now.ToString("mm:ss.fff") +" "+prependText +":"+stackTrace.GetFrame(2).GetMethod().Name);
        }
#endif
    }
}
