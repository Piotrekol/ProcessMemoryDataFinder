using System;
using System.Collections.Generic;
using System.Linq;
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
        private IObjectReader _objectReader;

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

        public void SetObjectReader(IObjectReader objectReader)
        {
            _objectReader = objectReader;
            ParentSig?.SetObjectReader(objectReader);
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

        private IntPtr ReadPointer(IntPtr baseAddr) => _objectReader.ReadPointer(baseAddr);

        public IntPtr GetPointer() => _objectReader.ReadPointer(ResolveAddress());

        public bool GetBoolean()
        {
            var data = GetValue(1);
            if (data != null)
                return BitConverter.ToBoolean(data, 0);
            return false;
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

        public byte GetByte()
        {
            var data = GetValue(1);
            if (data != null)
                return data[0];
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

        public List<int> GetIntList() => _objectReader.ReadIntList(ResolveAddress());

        public int[] GetIntArray() => _objectReader.ReadIntArray(ResolveAddress());

        public string GetString() => _objectReader.ReadUnicodeString(ResolveAddress());

        public override string ToString()
        {
            return
                $"Name:\"{Name}\", Pattern:\"{string.Join("", Pattern.Select(x => x.ToString("x2")))}\", " +
                $"Mask:\"{Mask}\", Offsets: \"{string.Join(",", PointerOffsets.Select(x => x.ToString()))}\", Parent: \"{ParentSig}\"";
        }
    }
}