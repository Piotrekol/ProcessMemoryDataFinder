using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using ProcessMemoryDataFinder.Misc;

namespace ProcessMemoryDataFinder.API
{
    public abstract class MemoryReader
    {
        public delegate IntPtr FindPatternF(byte[] btPattern, string strMask, int nOffset, bool useMask, MEMORY_BASIC_INFORMATION? lastAdress, out MEMORY_BASIC_INFORMATION? foundAt);

        public delegate byte[] ReadDataF(IntPtr adress, uint size);

        private readonly MemoryProcessAddressFinder _internals = new MemoryProcessAddressFinder();

        private readonly string _processName;
        private readonly ProcessMemoryReader _reader = new ProcessMemoryReader();
        private readonly SigScan _sigScan = new SigScan();
        private Process _currentProcess;

        public MemoryReader(string processName)
        {
            _processName = processName;
        }

        protected virtual Process CurrentProcess
        {
            get => _currentProcess;
            set
            {
                _currentProcess = value;
                _sigScan.Process = value;
                _reader.ReadProcess = value;
                ProcessChanged();
            }
        }

        protected abstract void ProcessChanged();

        protected IntPtr FindPattern(byte[] btPattern, string strMask, int nOffset, bool useMask, MEMORY_BASIC_INFORMATION? lastAdress, out MEMORY_BASIC_INFORMATION? foundAt){
            var pageExecuteRead = (uint) MemoryProtectionOptions.PAGE_EXECUTE_READ;
            IntPtr result;
            GetMemoryAddresses();
            if (lastAdress.HasValue)
            {
                if (_internals.MemReg.Contains(lastAdress.Value))
                {
                    result = ScanMemoryRange(lastAdress.Value, out foundAt);
                    if (result != IntPtr.Zero)
                    {
                        return result;
                    }
                }
            }

            foreach (var memoryAdress in _internals.MemReg)
            {
                result = ScanMemoryRange(memoryAdress, out foundAt);
                if (result != IntPtr.Zero)
                {
                    return result;
                }
            }

            _internals.MemReg = new List<MEMORY_BASIC_INFORMATION>();
            foundAt = null;
            return IntPtr.Zero;

            IntPtr ScanMemoryRange(MEMORY_BASIC_INFORMATION memoryAdress, out MEMORY_BASIC_INFORMATION? _foundAt)
            {
                _foundAt = null;
                if ((memoryAdress.Protect & pageExecuteRead) != 0)
                {
                    return IntPtr.Zero;
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
                    _internals.MemReg = new List<MEMORY_BASIC_INFORMATION>();
                    _foundAt = memoryAdress;
                    return result;
                }

                return IntPtr.Zero;
            }
        }

        protected byte[] ReadData(IntPtr address, uint size)
        {
            if (address == IntPtr.Zero)
            {
                return null;
            }

            if (CurrentProcess == null || CurrentProcess.SafeHasExited())
            {
                OpenProcess();
            }

            if (CurrentProcess == null)
            {
                return null;
            }

            if (_reader.OpenProcess() == IntPtr.Zero)
            {
                return null;
            }

            int bytesNumber;
            var bytesRead =
                _reader.ReadProcessMemory(address, size,
                    out bytesNumber);
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
                if (CurrentProcess != null && !CurrentProcess.SafeHasExited())
                {
                    return;
                }

                var p = Process.GetProcessesByName(_processName);
                var resolvedProcess = p.Length == 0 ? null : p[0];
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

        private List<MEMORY_BASIC_INFORMATION> GetMemoryAddresses()
        {
            OpenProcess();
            if (CurrentProcess == null)
            {
                return new List<MEMORY_BASIC_INFORMATION>();
            }

            try
            {
                _internals.MemInfo(CurrentProcess.Handle);
                return _internals.MemReg;
            }
            catch
            {
                return new List<MEMORY_BASIC_INFORMATION>();
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
    }
}