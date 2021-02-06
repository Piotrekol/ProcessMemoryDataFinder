using System;
using System.Collections.Generic;
using System.Text;
using ProcessMemoryDataFinder.API;

namespace ProcessMemoryDataFinder
{
    public interface IObjectReader
    {
        List<int> ReadIntList(IntPtr baseAddress);
        int[] ReadIntArray(IntPtr baseAddress);
        string ReadUnicodeString(IntPtr baseAddress);
        byte[] ReadStringBytes(IntPtr baseAddress, int bytesPerCharacter = 2);
        IntPtr ReadPointer(IntPtr baseAddr);
    }

    public class ObjectReader : IObjectReader
    {
        private readonly MemoryReader _memoryReader;

        public ObjectReader(MemoryReader memoryReader)
        {
            _memoryReader = memoryReader;
        }

        public List<int> ReadIntList(IntPtr baseAddress)
        {
            var (numberOfElements, firstElementPtr) = GetArrayLikeHeader(true, false, baseAddress);
            if (numberOfElements < 0) return null;
            if (numberOfElements == 0) return new List<int>();

            var totalByteCount = 4 * numberOfElements;
            var bytes = _memoryReader.ReadData(firstElementPtr, (uint)totalByteCount);
            if (bytes == null || bytes.Length != totalByteCount) return null;

            var list = new List<int>(numberOfElements);
            for (var offset = 0; offset < totalByteCount; offset += 4)
            {
                list.Add(BitConverter.ToInt32(bytes, offset));
            }

            return list;
        }

        public int[] ReadIntArray(IntPtr baseAddress)
        {
            var (numberOfElements, firstElementPtr) = GetArrayLikeHeader(false, false, baseAddress);
            if (numberOfElements < 0) return null;
            if (numberOfElements == 0) return Array.Empty<int>();

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
            var bytes = ReadStringBytes(baseAddress);
            if (bytes == null || bytes.Length == 0) return null;

            return Encoding.Unicode.GetString(bytes);
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
            //var pointer = ReadPointer(baseAddress);
            //if (pointer == IntPtr.Zero) return (-1, IntPtr.Zero);

            var address = ReadPointer(baseAddress);
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
                var numberOfElementBytes = _memoryReader.ReadData(numberOfElementsAddr, 4);
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
                var numberOfElementBytes = _memoryReader.ReadData(numberOfElementsAddr, 4);
                if (numberOfElementBytes == null || numberOfElementBytes.Length != 4) return (-1, IntPtr.Zero);
                numberOfElements = BitConverter.ToInt32(numberOfElementBytes, 0);
                firstElementPtr = numberOfElementsAddr + 4;
#endif
            }

            return (numberOfElements, firstElementPtr);
        }

        public IntPtr ReadPointer(IntPtr baseAddr)
        {
#if x64
            var data = _memoryReader.ReadData(baseAddr, 8);
            if (data != null)
                return new IntPtr(BitConverter.ToInt64(data, 0));
#else
            var data = _memoryReader.ReadData(baseAddr, 4);
            if (data != null)
                return new IntPtr(BitConverter.ToInt32(data, 0));
#endif
            return IntPtr.Zero;
        }
    }
}