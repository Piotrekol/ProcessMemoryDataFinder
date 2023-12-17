using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProcessMemoryDataFinder.API.Memory;

namespace ProcessMemoryDataFinder.API
{
    public class MemoryReaderManager : IDisposable
    {
        public delegate IntPtr FindPatternF(byte[] btPattern, string strMask, int nOffset, bool useMask);
        public delegate byte[] ReadDataF(IntPtr adress, uint size);

        private readonly string _processName;
        private readonly string _mainWindowTitleHint;
        private readonly SigScan _sigScan;
        private readonly MemoryReader _memoryReader;
        private SafeProcess _currentProcess;
        private Task ProcessWatcher;
        public int ProcessWatcherDelayMs { get; set; } = 1000;
        private CancellationTokenSource cts = new CancellationTokenSource();
        public event EventHandler ProcessChanged;
        public virtual SafeProcess CurrentProcess
        {
            get => _currentProcess;
            private set
            {
                _currentProcess = value;
                _sigScan.Process = value;
                ProcessChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public int IntPtrSize
        {
            get => _memoryReader.IntPtrSize;
            set
            {
                _memoryReader.IntPtrSize = value;
            }
        }

        public MemoryReaderManager(string processName, string mainWindowTitleHint)
        {
            _processName = processName;
            _mainWindowTitleHint = mainWindowTitleHint;
            ProcessWatcher = Task.Run(MonitorProcess, cts.Token);
            if (OperatingSystem.IsWindows())
                _memoryReader = new WindowsMemoryReader(IntPtr.Size);
            else if (OperatingSystem.IsLinux())
                _memoryReader = new LinuxMemoryReader(IntPtr.Size);
            else
                throw new NotImplementedException("Current operating system is not supported.");

            _sigScan = new SigScan(_memoryReader);
        }

        protected async Task MonitorProcess()
        {
            while (true)
            {
                if (cts.IsCancellationRequested)
                    return;

                if (CurrentProcess == null || CurrentProcess.HasExited())
                {
                    OpenProcess();
                }
                if (CurrentProcess != null)
                {
                    while (!CurrentProcess.Process.WaitForExit(1000))
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

            var pageExecuteRead = (uint)WindowsMemoryProtectionOptions.PAGE_EXECUTE_READ;
            IntPtr result;
            foreach (var memoryAddress in _memoryReader.ReadProcessMaps(CurrentProcess.Handle, CurrentProcess.PID))
            {
                if ((memoryAddress.Protect & pageExecuteRead) != 0)
                {
                    continue;
                }

                _sigScan.ResetRegion();
                _sigScan.Address = memoryAddress.BaseAddress;
                _sigScan.Size = (int)memoryAddress.RegionSize;
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
                    Console.WriteLine("found: "+ BitConverter.ToString(btPattern).Replace("-", ""));
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

            var bytes = new byte[size];
            if (_memoryReader.ReadProcessMemory(CurrentProcess.Handle, CurrentProcess.PID, address, size, bytes, out _))
                return bytes;

            return null;
        }

        private void OpenProcess()
        {
            try
            {
                Process[] p2 = Process.GetProcesses();
                IEnumerable<Process> p = Process.GetProcessesByName(_processName);
                if (!p.Any())
                    p = Process.GetProcessesByName(_processName + ".exe");

                if (!string.IsNullOrEmpty(_mainWindowTitleHint))
                    p = p.Where(x => x.MainWindowTitle.IndexOf(_mainWindowTitleHint, StringComparison.Ordinal) >= 0);

                var resolvedProcess = p.FirstOrDefault();
                if (resolvedProcess != null || CurrentProcess != null)
                    CurrentProcess = resolvedProcess.ToSafeProcess();

            }
            catch (Win32Exception)
            {
                CurrentProcess = null;
            }
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