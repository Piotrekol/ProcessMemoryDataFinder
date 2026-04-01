using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using ProcessMemoryDataFinder.API;

namespace ProcessMemoryDataFinder
{
    public interface IObjectReader
    {
        List<int> ReadIntList(IntPtr baseAddress);
        List<uint> ReadUIntList(IntPtr baseAddress);
        int[] ReadIntArray(IntPtr baseAddress);
        string ReadUnicodeString(IntPtr baseAddress);
        byte[] ReadStringBytes(IntPtr baseAddress, int bytesPerCharacter = 2);
        IntPtr ReadPointer(IntPtr baseAddr);
        int IntPtrSize { get; set; }
        bool TryReadIntList(IntPtr baseAddress, List<int> target);
    }

    public class ObjectReader : IObjectReader
    {
        private const int MaxInt32ElementCount = int.MaxValue / 4;

        private readonly MemoryReaderManager _memoryReader;

        /// <summary>
        /// Size of pointers in searched process
        /// </summary>
        public int IntPtrSize { get; set; } = IntPtr.Size;

        private bool IsX64 => IntPtrSize == 8;
        private int ListArrayFirstElementOffset = 0;

        public ObjectReader(MemoryReaderManager memoryReader)
        {
            _memoryReader = memoryReader;
            ListArrayFirstElementOffset = OperatingSystem.IsLinux() ? 4 : 0;
        }

        public List<uint> ReadUIntList(IntPtr baseAddress)
        {
            var readResult = ReadListBytes(baseAddress, 4);
            if (readResult.Bytes == null) return null;

            var list = new List<uint>(readResult.NumberOfElements);
            for (var offset = 0; offset < readResult.NumberOfElements * 4; offset += 4)
            {
                list.Add(BitConverter.ToUInt32(readResult.Bytes, offset));
            }

            return list;
        }

        public List<int> ReadIntList(IntPtr baseAddress)
        {
            var readResult = ReadListBytes(baseAddress, 4);
            if (readResult.Bytes == null) return null;

            var list = new List<int>(readResult.NumberOfElements);
            for (var offset = 0; offset < readResult.NumberOfElements * 4; offset += 4)
            {
                list.Add(BitConverter.ToInt32(readResult.Bytes, offset));
            }

            return list;
        }

        public bool TryReadIntList(IntPtr baseAddress, List<int> target)
        {
            var (numberOfElements, firstElementPtr) = GetArrayLikeHeader(true, false, baseAddress);
            if (numberOfElements < 0) return false;

            target.Clear();
            if (numberOfElements == 0) return true;
            if (numberOfElements > MaxInt32ElementCount) return false;
            
            int totalByteCount = 4 * numberOfElements;
            byte[] rented = ArrayPool<byte>.Shared.Rent(totalByteCount);
            try
            {
                if (!_memoryReader.ReadData(firstElementPtr, rented.AsSpan(0, totalByteCount)))
                    return false;

                if (target.Capacity < numberOfElements)
                    target.Capacity = numberOfElements;

                var ints = MemoryMarshal.Cast<byte, int>(rented.AsSpan(0, totalByteCount));
                for (int i = 0; i < ints.Length; i++)
                    target.Add(ints[i]);

                return true;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }

        protected (int NumberOfElements, byte[] Bytes) ReadListBytes(IntPtr baseAddress, int entrySize)
        {
            var (numberOfElements, firstElementPtr) = GetArrayLikeHeader(true, false, baseAddress);
            if (numberOfElements < 0) return (-1, null);
            if (numberOfElements == 0) return (0, Array.Empty<byte>());
            
            var maxElements = int.MaxValue / entrySize;
            if (numberOfElements > maxElements) return (-1, null);

            var totalByteCount = entrySize * numberOfElements;
            var bytes = _memoryReader.ReadData(firstElementPtr, (uint)totalByteCount);
            if (bytes == null || bytes.Length != totalByteCount) return (-1, null);

            return (numberOfElements, bytes);
        }

        public int[] ReadIntArray(IntPtr baseAddress)
        {
            var (numberOfElements, firstElementPtr) = GetArrayLikeHeader(false, false, baseAddress);
            if (numberOfElements < 0) return null;
            if (numberOfElements == 0) return Array.Empty<int>();
            if (numberOfElements > MaxInt32ElementCount) return null;

            var totalByteCount = 4 * numberOfElements;
            var bytes = _memoryReader.ReadData(firstElementPtr, (uint)totalByteCount);
            if (bytes == null || bytes.Length != totalByteCount) return null;

            var arr = new int[numberOfElements];
            for (int offset = 0, i = 0; i < numberOfElements; offset += 4, ++i)
            {
                arr[i] = BitConverter.ToInt32(bytes, offset);
            }

            return arr;
        }

        public string ReadUnicodeString(IntPtr baseAddress)
        {
            var (numberOfElements, firstElementPtr) = GetArrayLikeHeader(false, true, baseAddress);
            if (numberOfElements <= 0 || numberOfElements > 262144) return null;

            int totalByteCount = 2 * numberOfElements;
            byte[] rented = ArrayPool<byte>.Shared.Rent(totalByteCount);

            try
            {
                if (!_memoryReader.ReadData(firstElementPtr, rented.AsSpan(0, totalByteCount)))
                {
                    return null;
                }

                return Encoding.Unicode.GetString(rented.AsSpan(0, totalByteCount));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }

        public byte[] ReadStringBytes(IntPtr baseAddress, int bytesPerCharacter = 2)
        {
            var (numberOfElements, firstElementPtr) = GetArrayLikeHeader(false, true, baseAddress);
            if (numberOfElements <= 0) return null;

            /*
             * If there's a case where string can be longer than 2^18, this should get adjusted to bigger value.
             * In my testing in StreamCompanion(across whole userbase) this value can be [incorrectly] set to ridiculous value, sometimes even causing OutOfMemoryException
             */
            if (numberOfElements > 262144) return null;

            var totalByteCount = bytesPerCharacter * numberOfElements;
            var bytes = _memoryReader.ReadData(firstElementPtr, (uint)totalByteCount);
            if (bytes == null || bytes.Length != totalByteCount) return null;

            return bytes; //Encoding.Unicode.GetString(bytes);
        }

        /// <summary>
        /// Get the number of elements and a pointer to the first element of an array like structure, supported structures at the moment are: a simple array, a List object, and a string.
        /// 
        /// An string and a simple array are very similar but every so slightly different (the size of the number of elements fields)
        /// A List object has its own number of elements field and an internal array containing the elements itself
        /// </summary>
        /// <param name="isList"></param>
        /// <param name="isString"></param>
        /// <param name="baseAddress"></param>
        /// <returns></returns>
        protected (int numberOfElements, IntPtr firstElementPtr) GetArrayLikeHeader(bool isList, bool isString,
            IntPtr baseAddress)
        {
            var address = ReadPointer(baseAddress);
            if (address == IntPtr.Zero) return (-1, IntPtr.Zero);

            int numberOfElements;
            IntPtr firstElementPtr, numberOfElementsAddr;

            if (isList)
            {
                // its a list, not an array, read the length from the list object and get the firstElementPtr from the internal array
                numberOfElementsAddr = address + 3 * IntPtrSize; //(IsX64 ? 0x18 : 0x0C);

                // in a list, regardless of platform, the size element is always 4 bytes
                Span<byte> countBuf = stackalloc byte[4];
                if (!_memoryReader.ReadData(numberOfElementsAddr, countBuf)) return (-1, IntPtr.Zero);

                numberOfElements = MemoryMarshal.Read<int>(countBuf);

                // lets point to the first element in the internal array:
                // 1. skip VTable of list structure (4 or 8 bytes depending on platform)
                // 2. resolve pinter to internal array
                // 3. skip VTable and internal number of elements (both 4 or 8 bytes depending on platform)
                var internalArray = ReadPointer(address + IntPtrSize);
                firstElementPtr = internalArray + 2 * IntPtrSize + ListArrayFirstElementOffset;// IsX64 ? internalArray +16 : internalArray + 8
            }
            else
            {
                // normal array, first element in the structure is the length, skip VTable
                numberOfElementsAddr = address + IntPtrSize;

                // in an array structure the size of of the numberOfElements field depends on the platform (4 bytes or 8 bytes)
                // except for a string, then its always 4 bytes
                // first element in the array is after the numberOfElements field
                if (IsX64)
                {
                    // 64bit platform, unless string size bytes is 8
                    if (isString)
                    {
                        Span<byte> countBuf = stackalloc byte[4];
                        if (!_memoryReader.ReadData(numberOfElementsAddr, countBuf)) return (-1, IntPtr.Zero);

                        numberOfElements = MemoryMarshal.Read<int>(countBuf);
                        firstElementPtr = numberOfElementsAddr + 4;
                    }
                    else
                    {
                        Span<byte> countBuf = stackalloc byte[8];
                        if (!_memoryReader.ReadData(numberOfElementsAddr, countBuf)) return (-1, IntPtr.Zero);

                        var numberOfElementsLong = MemoryMarshal.Read<long>(countBuf);
                        // ok, realistically we're probably never gonna get an array of more then 2^32-1 elements, so lets cast this down to an int
                        if (numberOfElementsLong > int.MaxValue)
                        {
                            // unable to read, lets just return nothing
                            return (-1, IntPtr.Zero);
                        }
                        numberOfElements = (int)numberOfElementsLong;
                        firstElementPtr = numberOfElementsAddr + 8 + ListArrayFirstElementOffset;
                    }
                }
                else
                {
                    // 32bit platform, its always 4 bytes, regardless of the value of isString
                    Span<byte> countBuf = stackalloc byte[4];
                    if (!_memoryReader.ReadData(numberOfElementsAddr, countBuf)) return (-1, IntPtr.Zero);

                    numberOfElements = MemoryMarshal.Read<int>(countBuf);
                    firstElementPtr = numberOfElementsAddr + 4;
                }
            }

            return (numberOfElements, firstElementPtr);
        }

        public IntPtr ReadPointer(IntPtr baseAddr)
        {
            Span<byte> data = stackalloc byte[8];
            if (!_memoryReader.ReadData(baseAddr, data[..IntPtrSize]))
                return IntPtr.Zero;

            if (IsX64)
                return new IntPtr(MemoryMarshal.Read<long>(data));

            var rawPtr = MemoryMarshal.Read<uint>(data);

            if (rawPtr > IntPtrExtensions.MaxValue)
                return IntPtr.Zero;

            return new IntPtr(rawPtr);
        }
    }
}