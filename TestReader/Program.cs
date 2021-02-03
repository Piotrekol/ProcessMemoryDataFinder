using System;
using System.Collections.Generic;
using System.Threading;
using ProcessMemoryDataFinder.API;

namespace Test
{
    class Program
    {
        internal static string IntArrToString(IEnumerable<int> arr) => arr == null ? "NULL" : $"[{string.Join(",", arr)}]";
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var reader = new ThingReader();
            while (true)
            {
                Thread.Sleep(100);
                var i = reader.GetThingInt();
                var s = reader.GetThingString();
                var list = reader.GetThingIntList();
                var arr = reader.GetThingIntArray();
                Console.WriteLine($"i: {i} | s: {s} | a: {IntArrToString(arr)} | l: {IntArrToString(list)}");
            }
        }
    }

    public class ThingReader : MemoryReaderEx
    {
        private readonly object _lockingObject = new object();

        public ThingReader() : base("TestApp")
        {
            CreateSignatures();
        }

        private void CreateSignatures()
        {
            Signatures.Add((int)SignatureNames.ThingBase, new SigEx
            {
                Name = "ThingBase",
                Pattern = UnpackStr("A6ADEB393E0370C0"),
#if x64
                Offset = -0x20,
#else
                Offset = -0x04,
#endif
                UseMask = false
            });
            Signatures.Add((int)SignatureNames.ThingInt, new SigEx
            {
                Name = "ThingInt",
                ParentSig = Signatures[(int)SignatureNames.ThingBase],
#if x64
                Offset = 0x28,
#else
                Offset = 0x18,
#endif
                UseMask = false
            });
            Signatures.Add((int)SignatureNames.ThingIntList, new SigEx
            {
                Name = "ThingIntList",
                ParentSig = Signatures[(int)SignatureNames.ThingBase],
#if x64
                Offset = 0x10,
#else
                Offset = 0x10,
#endif
                UseMask = false
            });
            Signatures.Add((int)SignatureNames.ThingIntArray, new SigEx
            {
                Name = "ThingIntArray",
                ParentSig = Signatures[(int)SignatureNames.ThingBase],
#if x64
                Offset = 0x08,
#else
                Offset = 0x0C,
#endif
                UseMask = false
            });
            Signatures.Add((int)SignatureNames.ThingString, new SigEx
            {
                Name = "ThingString",
                ParentSig = Signatures[(int)SignatureNames.ThingBase],
#if x64
                Offset = 0x18,
#else
                Offset = 0x14,
#endif
                UseMask = false
            });
        }


        public int GetThingInt() => GetInt((int) SignatureNames.ThingInt);
        public List<int> GetThingIntList() => GetIntList((int)SignatureNames.ThingIntList);
        public int[] GetThingIntArray() => GetIntArray((int)SignatureNames.ThingIntArray);
        public string GetThingString() => GetString((int) SignatureNames.ThingString);

        protected override int GetInt(int signatureId)
        {
            lock (_lockingObject)
            {
                ResetPointer(signatureId);
                return base.GetInt(signatureId);
            }
        }
        protected override string GetString(int signatureId)
        {
            lock (_lockingObject)
            {
                ResetPointer(signatureId);
                return base.GetString(signatureId);
            }
        }
        protected override List<int> GetIntList(int signatureId)
        {
            lock (_lockingObject)
            {
                ResetPointer(signatureId);
                return base.GetIntList(signatureId);
            }
        }
        protected override int[] GetIntArray(int signatureId)
        {
            lock (_lockingObject)
            {
                ResetPointer(signatureId);
                return base.GetIntArray(signatureId);
            }
        }
    }

    internal enum SignatureNames
    {
        ThingBase,
        ThingInt,
        ThingIntArray,
        ThingIntList,
        ThingString
    }
}
