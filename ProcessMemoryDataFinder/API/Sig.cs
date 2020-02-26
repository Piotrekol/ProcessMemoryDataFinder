using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessMemoryDataFinder.API
{
    public class Sig
    {
        /// <summary>
        ///     Address found by pattern searcher + offset
        /// </summary>
        public IntPtr Address = IntPtr.Zero;

        public string Mask;
        public int Offset;
        public byte[] Pattern;
    }

    public class SigEx : Sig
    {
        private MemoryReader.FindPatternF _findPatternFunc;
        private MemoryReader.ReadDataF _readDataFunc;

        public string Name { get; set; }
        /// <summary>
        ///     Final address - after traversing pointers.
        /// </summary>
        private IntPtr _resolvedAddress = IntPtr.Zero;

        /// <summary>
        ///     Resolved address of parent will be used instead of pattern.
        /// </summary>
        public SigEx ParentSig;

        /// <summary>
        ///     If false, <see cref="Sig.Mask" /> won't be used to find specified <see cref="Sig.Pattern" /> address and much
        ///     quicker way of scanning memory will be used.
        /// </summary>
        public bool UseMask { get; set; } = true;

        /// <summary>
        ///     Pointer offsets needed to resolve final address
        /// </summary>
        public List<int> PointerOffsets { get; set; } = new List<int>();

        public void SetFindPatternF(MemoryReader.FindPatternF f)
        {
            _findPatternFunc = f;
            ParentSig?.SetFindPatternF(f);
        }

        public void SetReadDataF(MemoryReader.ReadDataF f)
        {
            _readDataFunc = f;
            ParentSig?.SetReadDataF(f);
        }

        /// <summary>
        ///     Resets this <see cref="SigEx" /> instance tree.
        /// </summary>
        public void Reset()
        {
            Address = IntPtr.Zero;
            _resolvedAddress = IntPtr.Zero;
            ParentSig?.Reset();
        }

        /// <summary>
        ///     Resets the final resolved address of this <see cref="SigEx" /> tree, forcing signatures to re-traverse pointers.
        /// </summary>
        public void ResetPointer()
        {
            _resolvedAddress = IntPtr.Zero;
            ParentSig?.ResetPointer();
        }

        protected IntPtr ResolveAddress()
        {
            if (_resolvedAddress != IntPtr.Zero)
                return _resolvedAddress;
            var addr = IntPtr.Zero;
            if (ParentSig != null)
            {
                addr = ParentSig.ResolveAddress();
                addr += Offset;
            }

            if (Pattern != null)
            {
                if (Address != IntPtr.Zero)
                {
                    addr = Address;
                }
                else
                {
                    try
                    {
                        Address = _findPatternFunc(Pattern, Mask, Offset, UseMask);
                    }
                    catch (InvalidOperationException)
                    {
                        return IntPtr.Zero;
                    }

                    addr = Address;
                }
            }

            if (addr != IntPtr.Zero) _resolvedAddress = ResolveChainOfPointers(addr);
            return _resolvedAddress;
        }

        private IntPtr ResolveChainOfPointers(IntPtr baseAddress)
        {
            if (PointerOffsets?.Count == 0) return baseAddress;

            var pointer = baseAddress;
            pointer = ReadPointer(pointer);

            for (var i = 0; i < PointerOffsets.Count - 1; i++)
            {
                var offset = PointerOffsets[i];
                pointer = ReadPointer(pointer + offset);
            }

            pointer = pointer + PointerOffsets[PointerOffsets.Count - 1];
            return pointer;
        }

        private byte[] GetValue(uint size)
        {
            var address = ResolveAddress();
            return _readDataFunc(address, size);
        }

        private IntPtr ReadPointer(IntPtr baseAddr)
        {
            var data = _readDataFunc(baseAddr, 4);
            if (data != null)
                return new IntPtr(BitConverter.ToInt32(data, 0));
            return IntPtr.Zero;
        }

        public IntPtr GetPointer()
        {
            return ReadPointer(ResolveAddress());
        }

        public int GetInt()
        {
            var data = GetValue(4);
            if (data != null)
                return BitConverter.ToInt32(data, 0);
            return -1;
        }

        public ushort GetUShort()
        {
            var data = GetValue(2);
            if (data != null)
                return BitConverter.ToUInt16(data, 0);
            return 0;
        }

        public double GetDouble()
        {
            var data = GetValue(8);
            if (data != null)
                return BitConverter.ToDouble(data, 0);
            return 0;
        }

        public float GetFloat()
        {
            var data = GetValue(4);
            if (data != null)
                return BitConverter.ToSingle(data, 0);
            return -1;
        }

        private Tuple<int, IntPtr> GetArrayHeader(bool skip = false)
        {
            var pointer = ResolveAddress();
            if (pointer == IntPtr.Zero) return null;

            var address = ReadPointer(pointer);
            if (address == IntPtr.Zero) return null;

            IntPtr arrayAddress;
            if (skip)
            {
                arrayAddress = address;
            }
            else
            {
                arrayAddress = ReadPointer(address + 4);
                if (arrayAddress == IntPtr.Zero) return null;
            }

            byte[] rawNumberOfElements;
            if (skip)
                rawNumberOfElements = _readDataFunc(arrayAddress + 4, 4); //Amount of allocated space (char[] (strings))
            else
                rawNumberOfElements = _readDataFunc(address + 12, 4); //Array.Count()

            if (rawNumberOfElements == null) return null;
            var numberOfElements = BitConverter.ToInt32(rawNumberOfElements, 0);

            return new Tuple<int, IntPtr>(numberOfElements, arrayAddress);
        }

        public List<int> GetIntList()
        {
            var headerResult = GetArrayHeader();
            if (headerResult == null) return null;
            if (headerResult.Item1 <= 0) return new List<int>();

            var ret = new List<int>();
            var expectedSize = 4 * (uint) headerResult.Item1;

            var memoryFragment = _readDataFunc(headerResult.Item2 + 8, expectedSize);

            if (memoryFragment == null || memoryFragment.Length != expectedSize)
                return null;

            for (int i = 0; i < headerResult.Item1; i++)
            {
                ret.Add(BitConverter.ToInt32(memoryFragment, i * 4));
            }

            return ret;
        }
        public string GetString()
        {

            var headerResult = GetArrayHeader(true);
            if (headerResult == null) return string.Empty;
            if (headerResult.Item1 == 0) return string.Empty;

            var stringLength = headerResult.Item1 * 2;
            /*
             * If there's a case where string can be longer than 2^18, this should get adjusted to bigger value.
             * In my testing in StreamCompanion(across whole userbase) this value can be [incorrectly] set to ridiculous value, sometimes even causing OutOfMemoryException
             */
            if (stringLength > 262144) return string.Empty;

            var stringData = _readDataFunc(headerResult.Item2 + 8, (uint)stringLength);
            if (stringData == null) return string.Empty;

            var result = Encoding.Unicode.GetString(stringData);
            return result;
        }
    }
}