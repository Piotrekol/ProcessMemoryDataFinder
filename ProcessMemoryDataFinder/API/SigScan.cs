using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ProcessMemoryDataFinder.Misc;

namespace ProcessMemoryDataFinder.API
{
    //Slightly modified (by Piotrekol) signature scanner (added ability of scanning memory without need of mask usage)
    //
    //Original changelog and credits follow:
    //
    // sigScan C# Implementation - Written by atom0s [aka Wiccaan]
    // Class Version: 2.0.0
    //
    // [ CHANGE LOG ] -------------------------------------------------------------------------
    //
    //      2.0.0
    //          - Updated to no longer require unsafe or fixed code.
    //          - Removed unneeded methods and code.
    //
    //      1.0.0
    //          - First version written and release.
    //
    // [ CREDITS ] ----------------------------------------------------------------------------
    //
    // sigScan is based on the FindPattern code written by
    // dom1n1k and Patrick at GameDeception.net
    //
    // Full credit to them for the purpose of this code. I, atom0s, simply
    // take credit for converting it to C#.
    internal class SigScan
    {
        /// <summary>
        /// ReadProcessMemory
        /// 
        ///     API import definition for ReadProcessMemory.
        /// </summary>
        /// <param name="hProcess">Handle to the process we want to read from.</param>
        /// <param name="lpBaseAddress">The base address to start reading from.</param>
        /// <param name="lpBuffer">The return buffer to write the read data to.</param>
        /// <param name="dwSize">The size of data we wish to read.</param>
        /// <param name="lpNumberOfBytesRead">The number of bytes successfully read.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out int lpNumberOfBytesRead
            );

        /// <summary>
        /// m_vDumpedRegion
        /// 
        ///     The memory dumped from the external process.
        /// </summary>
        private byte[] m_vDumpedRegion;

        /// <summary>
        /// m_vProcess
        /// 
        ///     The process we want to read the memory of.
        /// </summary>
        private Process m_vProcess;

        /// <summary>
        /// m_vAddress
        /// 
        ///     The starting address we want to begin reading at.
        /// </summary>
        private IntPtr m_vAddress;

        /// <summary>
        /// m_vSize
        /// 
        ///     The number of bytes we wish to read from the process.
        /// </summary>
        private Int32 m_vSize;

        private List<Sig> m_SigQueue;


        #region "sigScan Class Construction"
        /// <summary>
        /// SigScan
        /// 
        ///     Main class constructor that uses no params. 
        ///     Simply initializes the class properties and 
        ///     expects the user to set them later.
        /// </summary>
        public SigScan()
        {
            m_vProcess = null;
            m_vAddress = IntPtr.Zero;
            m_vSize = 0;
            m_vDumpedRegion = null;
            m_SigQueue = new List<Sig>();
        }
        /// <summary>
        /// SigScan
        /// 
        ///     Overloaded class constructor that sets the class
        ///     properties during construction.
        /// </summary>
        /// <param name="proc">The process to dump the memory from.</param>
        /// <param name="addr">The started address to begin the dump.</param>
        /// <param name="size">The size of the dump.</param>
        public SigScan(Process proc, IntPtr addr, int size)
        {
            m_vProcess = proc;
            m_vAddress = addr;
            m_vSize = size;
            m_SigQueue = new List<Sig>();
        }
        #endregion

        #region "sigScan Class Private Methods"
        
        /// <summary>
        /// DumpMemory
        /// 
        ///     Internal memory dump function that uses the set class
        ///     properties to dump a memory region.
        /// </summary>
        /// <returns>Boolean based on RPM results and valid properties.</returns>
        private bool DumpMemory()
        {
            try
            {
                // Checks to ensure we have valid data.
                if (m_vProcess == null)
                    return false;
                if (m_vProcess.SafeHasExited())
                    return false;
                if (m_vAddress == IntPtr.Zero)
                    return false;
                if (m_vSize == 0)
                    return false;

                // Create the region space to dump into.
                m_vDumpedRegion = new byte[m_vSize];

                bool bReturn = false;
                int nBytesRead = 0;

                // Dump the memory.
                bReturn = ReadProcessMemory(
                    m_vProcess.Handle, m_vAddress, m_vDumpedRegion, m_vSize, out nBytesRead
                    );

                // Validation checks.
                if (bReturn == false || nBytesRead != m_vSize)
                    return false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// MaskCheck
        /// 
        ///     Compares the current pattern byte to the current memory dump
        ///     byte to check for a match. Uses wildcards to skip bytes that
        ///     are deemed unneeded in the compares.
        /// </summary>
        /// <param name="nOffset">Offset in the dump to start at.</param>
        /// <param name="btPattern">Pattern to scan for.</param>
        /// <param name="strMask">Mask to compare against.</param>
        /// <returns>Boolean depending on if the pattern was found.</returns>
        private bool MaskCheck(int nOffset, byte[] btPattern, string strMask)
        {
            // Loop the pattern and compare to the mask and dump.
            for (int x = 0; x < btPattern.Length; x++)
            {
                // If the mask char is a wildcard, just continue.
                if (strMask[x] == '?')
                    continue;

                // If the mask char is not a wildcard, ensure a match is made in the pattern.
                if ((strMask[x] == 'x') && (btPattern[x] != m_vDumpedRegion[nOffset + x]))
                    return false;
            }

            // The loop was successful so we found the pattern.
            return true;
        }
        #endregion

        #region "sigScan Class Public Methods"
        /// <summary>
        /// FindPattern
        /// 
        ///     Attempts to locate the given pattern inside the dumped memory region
        ///     compared against the given mask. If the pattern is found, the offset
        ///     is added to the located address and returned to the user.
        /// </summary>
        /// <param name="btPattern">Byte pattern to look for in the dumped region.</param>
        /// <param name="strMask">The mask string to compare against.</param>
        /// <param name="nOffset">The offset added to the result address.</param>
        /// <returns>IntPtr - zero if not found, address if found.</returns>
        public IntPtr FindPattern(byte[] btPattern, string strMask, int nOffset)
        {
            try
            {
                // Dump the memory region if we have not dumped it yet.
                if (m_vDumpedRegion == null || m_vDumpedRegion.Length == 0)
                {
                    if (!DumpMemory())
                        return IntPtr.Zero;
                }

                // Ensure the mask and pattern lengths match.
                if (strMask.Length != btPattern.Length)
                    return IntPtr.Zero;

                // Loop the region and look for the pattern.
                for (int x = 0; x < m_vDumpedRegion.Length; x++)
                {
                    if (MaskCheck(x, btPattern, strMask))
                    {
                        // The pattern was found, return it.
                        return m_vAddress + (x + nOffset);
                    }
                }

                // Pattern was not found.
                return IntPtr.Zero;
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// FindPattern
        /// 
        ///     Attempts to locate all the patterns in SigQueue inside the dumped
        ///     memory region compared against the given mask. If the pattern is
        ///     found, the offset is added to the located address and the Sig's
        ///     Address will be set.
        /// </summary>
        public void FindPattern()
        {
            try
            {
                // Dump the memory region if we have not dumped it yet.
                if (m_vDumpedRegion == null || m_vDumpedRegion.Length == 0)
                {
                    if (!DumpMemory())
                        return;
                }

                // Ensure the mask and pattern lengths match.
                foreach (var sig in SigQueue)
                {
                    if(sig.Mask.Length != sig.Pattern.Length)
                        return;
                }
                

                // Loop the region and look for the patterns.
                for (int x = 0; x < m_vDumpedRegion.Length; x++)
                {
                    foreach (var sig in m_SigQueue)
                    {
                        if (MaskCheck(x, sig.Pattern, sig.Mask))
                        {
                            // The pattern was found, set it.
                            sig.Address = m_vAddress + (x + sig.Offset);
                        }
                    }
                }

            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// ResetRegion
        /// 
        ///     Resets the memory dump array to nothing to allow
        ///     the class to redump the memory.
        /// </summary>
        public void ResetRegion()
        {
            m_vDumpedRegion = null;
        }
        #endregion

        #region "sigScan Class Properties"
        public Process Process
        {
            get { return m_vProcess; }
            set { m_vProcess = value; }
        }
        public IntPtr Address
        {
            get { return m_vAddress; }
            set { m_vAddress = value; }
        }
        public Int32 Size
        {
            get { return m_vSize; }
            set { m_vSize = value; }
        }
        public List<Sig> SigQueue
        {
            get { return m_SigQueue; }
            set { m_SigQueue = value; }
        }
        #endregion
        /// <summary>
        /// Faster FindPattern implementation that doesn't use mask<para/>
        /// Instead of 4~5s scans with mask this can do same byte length scan in about half a second
        /// </summary>
        public IntPtr FindPattern(byte[] patternBytes, int nOffset)
        {
            // Dump the memory region if we have not dumped it yet.
            if (m_vDumpedRegion == null || m_vDumpedRegion.Length == 0)
            {
                if (!DumpMemory())
                    return IntPtr.Zero;
            }
            var result = Scan(m_vDumpedRegion, patternBytes);
            if (result == -1)
                return IntPtr.Zero;

            return m_vAddress + (nOffset + result);
        }

        private static int Scan(byte[] sIn, byte[] sFor)
        {
            if (sIn == null)
            {
                return -1;
            }

            int[] sBytes = new int[256];
            int end = sFor.Length - 1;
            for (int i = 0; i < 256; i++)
            {
                sBytes[i] = sFor.Length;
            }

            for (int i = 0; i < end; i++)
            {
                sBytes[sFor[i]] = end - i;
            }

            int pool = 0;
            while (pool <= sIn.Length - sFor.Length)
            {
                for (int i = end; (sIn[pool + i] == sFor[i]); i--)
                {
                    if (i == 0)
                    {
                        return pool;
                    }
                }

                pool += sBytes[sIn[pool + end]];
            }
            return -1;
        }
    }

    
}