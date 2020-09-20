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
#if x64
            var data = _readDataFunc(baseAddr, 8);
            if (data != null)
                return new IntPtr(BitConverter.ToInt64(data, 0));
#else
            var data = _readDataFunc(baseAddr, 4);
            if (data != null)
                return new IntPtr(BitConverter.ToInt32(data, 0));
#endif
            return IntPtr.Zero;
        }

        public IntPtr GetPointer()
        {
            return ReadPointer(ResolveAddress());
        }

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

        /// <summary>
        /// Get the number of elements and a pointer to the first element of an array like structure, supported structures at the moment are: a simple array, a List object, and a string.
        ///
        /// An string and a simple array are very similar but every so slightly different (the size of the number of elements fields)
        /// A List object has its own number of elements field and an internal array containing the elements itself
        /// </summary>
        /// <param name="isList"></param>
        /// <param name="isString"></param>
        /// <returns></returns>
        private (int numberOfElements, IntPtr firstElementPtr) GetArrayLikeHeader(bool isList, bool isString)
        {
            var pointer = ResolveAddress();
            if (pointer == IntPtr.Zero) return (-1, IntPtr.Zero);

            var address = ReadPointer(pointer);
            if (address == IntPtr.Zero) return (-1, IntPtr.Zero);

            int numberOfElements;
            IntPtr firstElementPtr;

            if (isList)
            {
                // its a list, not an array, read the length from the list object and get the firstElementPtr from the internal array
#if x64
                var numberOfElementsAddr = address + 0x18;
#else
                var numberOfElementsAddr = address + 0x0C;
#endif
                // in a list, regardless of platform, the size element is always 4 bytes
                var numberOfElementBytes = _readDataFunc(numberOfElementsAddr, 4);
                if (numberOfElementBytes == null || numberOfElementBytes.Length != 4) return (-1, IntPtr.Zero);
                numberOfElements = BitConverter.ToInt32(numberOfElementBytes, 0);

                // lets point to the first element in the internal array:
                // 1. skip VTable of list structure (4 or 8 bytes depending on platform)
                // 2. resolve pinter to internal array
                // 3. skip VTable and internal number of elements (both 4 or 8 bytes depending on platform)
#if x64
                var internalArray = ReadPointer(address + 8);
                firstElementPtr = internalArray + 16;
#else
                var internalArray = ReadPointer(address + 4);
                firstElementPtr = internalArray + 8;
#endif
            }
            else
            {
                // normal array, first element in the structure is the length, skip VTable
#if x64
                var numberOfElementsAddr = address + 8;
#else
                var numberOfElementsAddr = address + 4;
#endif
                // in an array structure the size of of the numberOfElements field depends on the platform (4 bytes or 8 bytes)
                // except for a string, then its always 4 bytes
                // first element in the array is after the numberOfElements field
#if x64
                // 64bit platform, unless string size bytes is 8
                if (isString)
                {
                    var numberOfElementBytes = _readDataFunc(numberOfElementsAddr, 4);
                    if (numberOfElementBytes == null || numberOfElementBytes.Length != 4) return (-1, IntPtr.Zero);
                    numberOfElements = BitConverter.ToInt32(numberOfElementBytes, 0);
                    firstElementPtr = numberOfElementsAddr + 4;
                }
                else
                {
                    var numberOfElementBytes = _readDataFunc(numberOfElementsAddr, 8);
                    if (numberOfElementBytes == null || numberOfElementBytes.Length != 8) return (-1, IntPtr.Zero);

                    var numberOfElementsLong = BitConverter.ToInt64(numberOfElementBytes, 0);
                    // ok, realistically we're probably never gonna get an array of more then 2^32-1 elements, so lets cast this down to an int
                    if (numberOfElementsLong > int.MaxValue)
                    {
                        // unable to read, lets just return nothing
                        return (-1, IntPtr.Zero);
                    }
                    numberOfElements = (int) numberOfElementsLong;
                    firstElementPtr = numberOfElementsAddr + 8;
                }
#else
                // 32bit platform, its always 4 bytes, regardless of the value of isString
                var numberOfElementBytes = _readDataFunc(numberOfElementsAddr, 4);
                if (numberOfElementBytes == null || numberOfElementBytes.Length != 4) return (-1, IntPtr.Zero);
                numberOfElements = BitConverter.ToInt32(numberOfElementBytes, 0);
                firstElementPtr = numberOfElementsAddr + 4;
#endif
            }

            return (numberOfElements, firstElementPtr);
        }

        public List<int> GetIntList()
        {
             var (numberOfElements, firstElementPtr) = GetArrayLikeHeader(true, false);
             if (numberOfElements < 0) return null;
             if (numberOfElements == 0) return new List<int>();

             var totalByteCount = 4 * numberOfElements;
             var bytes = _readDataFunc(firstElementPtr, (uint)totalByteCount);
             if (bytes == null || bytes.Length != totalByteCount) return null;

             var list = new List<int>(numberOfElements);
             for (var offset = 0; offset < totalByteCount; offset += 4)
             {
                 list.Add(BitConverter.ToInt32(bytes, offset));
             }

             return list;
        }

        public int[] GetIntArray()
        {
            var (numberOfElements, firstElementPtr) = GetArrayLikeHeader(false, false);
            if (numberOfElements < 0) return null;
            if (numberOfElements == 0) return Array.Empty<int>();

            var totalByteCount = 4 * numberOfElements;
            var bytes = _readDataFunc(firstElementPtr, (uint)totalByteCount);
            if (bytes == null || bytes.Length != totalByteCount) return null;

            var arr = new int[numberOfElements];
            for (int offset = 0, i = 0; i < numberOfElements; offset += 4, ++i)
            {
                arr[i] = BitConverter.ToInt32(bytes, offset);
            }

            return arr;
        }

        public string GetString()
        {
             var (numberOfElements, firstElementPtr) = GetArrayLikeHeader(false, true);
             if (numberOfElements <= 0) return string.Empty;
             
             /*
              * If there's a case where string can be longer than 2^18, this should get adjusted to bigger value.
              * In my testing in StreamCompanion(across whole userbase) this value can be [incorrectly] set to ridiculous value, sometimes even causing OutOfMemoryException
              */
             if (numberOfElements > 262144) return string.Empty;

             var totalByteCount = 2 * numberOfElements;
             var bytes = _readDataFunc(firstElementPtr, (uint) totalByteCount);
             if (bytes == null || bytes.Length != totalByteCount) return null;

             return Encoding.Unicode.GetString(bytes);
        }

        public override string ToString()
        {
            return
                $"Name:\"{Name}\", Pattern:\"{string.Join("", Pattern.Select(x => x.ToString("x2")))}\", " +
                $"Mask:\"{Mask}\", Offsets: \"{string.Join(",", PointerOffsets.Select(x=>x.ToString()))}\", Parent: \"{ParentSig}\"";
        }
    }
}