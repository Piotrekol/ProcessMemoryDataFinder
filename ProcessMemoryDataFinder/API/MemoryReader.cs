using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProcessMemoryDataFinder.Misc;

namespace ProcessMemoryDataFinder.API
{
    public class MemoryReader : IDisposable
    {
        public delegate IntPtr FindPatternF(byte[] btPattern, string strMask, int nOffset, bool useMask);
        public delegate byte[] ReadDataF(IntPtr adress, uint size);

        private MemoryProcessAddressFinder _internals;

        private readonly string _processName;
        private readonly string _mainWindowTitleHint;
        private readonly ProcessMemoryReader _reader = new ProcessMemoryReader();
        private readonly SigScan _sigScan = new SigScan();
        private Process _currentProcess;
        private Task ProcessWatcher;
        public int ProcessWatcherDelayMs { get; set; } = 1000;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private int _intPtrSize = IntPtr.Size;
        public event EventHandler ProcessChanged;
        protected virtual IntPtr CurrentProcessHandle { get; set; } = IntPtr.Zero;
        public virtual Process CurrentProcess
        {
            get => _currentProcess;
            private set
            {
                _currentProcess = value;
                _sigScan.Process = value;
                _reader.ReadProcess = value;
                ProcessChanged?.Invoke(null, EventArgs.Empty);
                _reader.OpenProcess();
                try
                {
                    CurrentProcessHandle = value?.Handle ?? IntPtr.Zero;
                }
                catch (InvalidOperationException)
                {
                    CurrentProcessHandle = IntPtr.Zero;
                }
            }
        }

        public int IntPtrSize
        {
            get => _intPtrSize;
            set
            {
                _intPtrSize = value;
                if (value == 4)
                {
                    if(Environment.Is64BitProcess)
                        _internals = new X86ProcessX64RuntimeAddressFinder();
                    else
                        _internals = new X86ProcessX86RuntimeAddressFinder();
                }
                else
                    _internals = new X64MemoryProcessAddressFinder();
            }
        }

        public MemoryReader(string processName, string mainWindowTitleHint)
        {
            //Initialize process address finder
            IntPtrSize = IntPtrSize;

            _processName = processName;
            _mainWindowTitleHint = mainWindowTitleHint;
            ProcessWatcher = Task.Run(MonitorProcess, cts.Token);
        }

        protected async Task MonitorProcess()
        {
            while (true)
            {
                if (cts.IsCancellationRequested)
                    return;

                if (CurrentProcess == null || CurrentProcess.SafeHasExited())
                {
                    OpenProcess();
                }
                if (CurrentProcess != null)
                {
                    while (!CurrentProcess.WaitForExit(1000))
                    {
                        if (cts.IsCancellationRequested)
                            return;
                    }

                    CurrentProcess = null;
                }
                else
                    await Task.Delay(ProcessWatcherDelayMs);
            }
        }


        public IntPtr FindPattern(byte[] btPattern, string strMask, int nOffset, bool useMask)
        {
            if (CurrentProcess == null)
                return IntPtr.Zero;

            var pageExecuteRead = (uint)MemoryProtectionOptions.PAGE_EXECUTE_READ;
            IntPtr result;
            foreach (var memoryAdress in GetMemoryAddresses())
            {
                if ((memoryAdress.Protect & pageExecuteRead) != 0)
                {
                    continue;
                }

                _sigScan.ResetRegion();
                _sigScan.Address = memoryAdress.BaseAddress;
                _sigScan.Size = (int)memoryAdress.RegionSize;
                if (useMask)
                {
                    result = _sigScan.FindPattern(btPattern, strMask, nOffset);
                }
                else
                {
                    result = _sigScan.FindPattern(btPattern, nOffset);
                }

                if (result != IntPtr.Zero)
                {
                    return result;
                }
            }

            return IntPtr.Zero;
        }

        public byte[] ReadData(IntPtr address, uint size)
        {
            if (address == IntPtr.Zero || CurrentProcess == null)
            {
                return null;
            }

            var bytesRead =
                _reader.ReadProcessMemory(address, size,
                    out var bytesNumber);
            if (bytesNumber == size)
            {
                return bytesRead;
            }

            return null;
        }

        private void OpenProcess()
        {
            try
            {
                IEnumerable<Process> p = Process.GetProcessesByName(_processName);
                if (!string.IsNullOrEmpty(_mainWindowTitleHint))
                {
                    p = p.Where(x => x.MainWindowTitle.IndexOf(_mainWindowTitleHint, StringComparison.Ordinal) >= 0);
                }
                var resolvedProcess = p.FirstOrDefault();
                if (resolvedProcess != null || CurrentProcess != null)
                {
                    CurrentProcess = resolvedProcess;
                }
            }
            catch (Win32Exception)
            {
                CurrentProcess = null;
            }
        }

        private IEnumerable<MemoryProcessAddressFinder.MEMORY_BASIC_INFORMATION> GetMemoryAddresses()
        {
            var memInfoList = _internals.MemInfo(CurrentProcessHandle);

            foreach (var memoryInfo in memInfoList)
            {
                yield return memoryInfo;
            }
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [Flags]
        private enum MemoryProtectionOptions
        {
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_TARGETS_INVALID = 0x40000000,
            PAGE_TARGETS_NO_UPDATE = 0x40000000
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                cts?.Cancel();

                ProcessWatcher?.Dispose();
                _currentProcess?.Dispose();
                cts?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}