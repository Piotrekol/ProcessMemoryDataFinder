using ProcessMemoryDataFinder.API.Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessMemoryDataFinder.API;

public class MemoryReaderManager : IDisposable
{
    public delegate IntPtr FindPatternF(byte[] btPattern, string strMask, int nOffset, bool useMask);
    public delegate byte[] ReadDataF(IntPtr adress, uint size);
    private readonly ProcessTargetOptions _processTargetOptions;
    private readonly SigScan _sigScan;
    private readonly MemoryReader _memoryReader;
    private SafeProcess _currentProcess;
    private readonly Task ProcessWatcher;
    public int ProcessWatcherDelayMs { get; set; } = 1000;
    private readonly CancellationTokenSource cts = new();
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
        get => _memoryReader.IntPtrSize; set => _memoryReader.IntPtrSize = value;
    }

    public MemoryReaderManager(ProcessTargetOptions processTargetOptions)
    {
        _processTargetOptions = processTargetOptions;
        ProcessWatcher = Task.Run(MonitorProcess, cts.Token);

        if (OperatingSystem.IsWindows())
        {
            _memoryReader = new WindowsMemoryReader(IntPtr.Size);
        }
        else if (OperatingSystem.IsLinux())
        {
            _memoryReader = new LinuxMemoryReader(IntPtr.Size);
        }
        else
        {
            throw new NotImplementedException("Current operating system is not supported.");
        }
      
        _sigScan = new SigScan(_memoryReader);
    }

    protected async Task MonitorProcess()
    {
        while (true)
        {
            if (cts.IsCancellationRequested)
            {
                return;
            }

            if (CurrentProcess == null || CurrentProcess.HasExited())
            {
                OpenProcess();
            }

            if (CurrentProcess != null)
            {
                while (!CurrentProcess.Process.WaitForExit(1000))
                {
                    if (cts.IsCancellationRequested)
                    {
                        return;
                    }
                }

                CurrentProcess = null;
            }
            else
            {
                await Task.Delay(ProcessWatcherDelayMs);
            }
        }
    }

    public IntPtr FindPattern(byte[] btPattern, string strMask, int nOffset, bool useMask)
    {
        if (CurrentProcess == null)
        {
            return IntPtr.Zero;
        }

        uint pageExecuteRead = (uint)WindowsMemoryProtectionOptions.PAGE_EXECUTE_READ;
        IntPtr result;
        foreach (MEMORY_BASIC_INFORMATION memoryAddress in _memoryReader.ReadProcessMaps(CurrentProcess.Handle, CurrentProcess.PID))
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

        byte[] bytes = new byte[size];
        if (_memoryReader.ReadProcessMemory(CurrentProcess.Handle, CurrentProcess.PID, address, size, bytes, out _))
        {
            return bytes;
        }

        return null;
    }

    public bool ReadData(IntPtr address, Span<byte> buffer)
    {
        if (address == IntPtr.Zero || CurrentProcess == null)
            return false;

        return _memoryReader.ReadProcessMemory(CurrentProcess.Handle, CurrentProcess.PID, address, (uint)buffer.Length, buffer, out _);
    }

    private void OpenProcess()
    {
        try
        {
            IEnumerable<Process> processes = Process.GetProcessesByName(_processTargetOptions.ProcessName);

            if (!string.IsNullOrEmpty(_processTargetOptions.MainWindowTitleHint))
            {
                processes = processes.Where(process => process.MainWindowTitle.IndexOf(_processTargetOptions.MainWindowTitleHint, StringComparison.Ordinal) >= 0);
            }

            if (_processTargetOptions.Target64Bit.HasValue)
            {
                if (_processTargetOptions.Target64Bit.Value)
                {
                    processes = processes.Where(process => IsWow64Process(process.SafeHandle, out bool isWow64Process) && !isWow64Process);
                }
                else
                {
                    processes = processes.Where(process => IsWow64Process(process.SafeHandle, out bool isWow64Process) && isWow64Process);
                }
            }

            Process resolvedProcess = processes.FirstOrDefault();

            if (resolvedProcess is not null || CurrentProcess is not null)
            {
                CurrentProcess = resolvedProcess.ToSafeProcess();
            }
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

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool IsWow64Process(
        [In] Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid hProcess,
        [Out, MarshalAs(UnmanagedType.Bool)] out bool wow64Process
    );
}