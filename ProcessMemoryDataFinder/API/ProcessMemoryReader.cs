using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ProcessMemoryDataFinder.Misc;

namespace ProcessMemoryDataFinder.API
{
    internal class ProcessMemoryReader
    {
        private Process _mReadProcess;
        public IntPtr m_hProcess = IntPtr.Zero;

        private Process m_ReadProcess
        {
            get => _mReadProcess;
            set
            {
                _mReadProcess = value;
                if (m_hProcess != IntPtr.Zero)
                    CloseHandle();
                m_hProcess = IntPtr.Zero;
            }
        }

        public Process ReadProcess
        {
            get => m_ReadProcess;
            set => m_ReadProcess = value;
        }

        /// <summary>
        ///     Closes the process handle if it was open.
        /// </summary>
        /// <exception cref="Exception">CloseHandle failed</exception>
        public void CloseHandle()
        {
            if (ProcessMemoryReaderApi.CloseHandle(m_hProcess) == 0) throw new Exception("CloseHandle failed");
        }

        /// <summary>
        /// </summary>
        /// <returns>IntPtr of the opened process or IntPtr. Zero if open failed.</returns>
        public IntPtr OpenProcess()
        {
            if (m_ReadProcess != null)
                if (!_mReadProcess.SafeHasExited() && m_hProcess == IntPtr.Zero)
                    try
                    {
                        m_hProcess = ProcessMemoryReaderApi.OpenProcess(0x38, 1, (uint) m_ReadProcess.Id);
                    }
                    catch (Win32Exception)
                    {
                        // Most likely "Access denied"
                        //TODO: stop swallowing this exception and throw it like a man(and handle it elsewhere as necessary)
                        return IntPtr.Zero;
                    }

            return m_hProcess;
        }

        /// <summary>
        ///     Reads the process memory.
        /// </summary>
        /// <param name="memoryAddress">The memory address.</param>
        /// <param name="bytesToRead">The bytes to read.</param>
        /// <param name="bytesRead">The bytes read.</param>
        /// <returns></returns>
        /// <exception cref="Win32Exception"></exception>
        public byte[] ReadProcessMemory(IntPtr memoryAddress, uint bytesToRead, out uint bytesRead)
        {
            try
            {
                if (m_hProcess == IntPtr.Zero)
                {
                    bytesRead = 0;
                    return null;
                }

                var buffer = new byte[bytesToRead];
                if (
                    ProcessMemoryReaderApi.ReadProcessMemory(m_hProcess, memoryAddress, buffer, bytesToRead,
                        out var lpNumberOfBytesRead) == 0)
                {
                    bytesRead = 0;
                    return null;
                }

                // lpNumberOfBytesRead is an IntPtr here so technically if we are 32bit platform we should be calling ToInt32()
                // but that doesnt matter here since we just want a uint value, which on a 32bit platform will work just fine (just tiny bit of overhead of first going to a int64)
                // and on a 64bit platform we dont have to worry about the value being larger then uint max, since we gave it a uint bytesToRead
                // so this cast should never fail
                bytesRead = (uint)lpNumberOfBytesRead.ToInt64();
                return buffer;
            }
            catch
            {
                bytesRead = 0;
                return null;
            }
        }

        private class ProcessMemoryReaderApi
        {
            public const uint PROCESS_VM_OPERATION = 8;
            public const uint PROCESS_VM_READ = 0x10;
            public const uint PROCESS_VM_WRITE = 0x20;

            [DllImport("kernel32.dll")]
            public static extern int CloseHandle(IntPtr hObject);

            [DllImport("kernel32.dll")]
            public static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern int ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In] [Out] byte[] buffer,
                uint size, out IntPtr lpNumberOfBytesRead);
        }
    }
}