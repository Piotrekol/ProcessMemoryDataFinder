using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ProcessMemoryDataFinder.API;
using ProcessMemoryDataFinder.Structured.Tokenizer;

namespace ProcessMemoryDataFinder.Structured
{
    public class StructuredMemoryReader : IDisposable, IStructuredMemoryReader
    {
        protected MemoryReader _memoryReader;
        protected AddressFinder _addressFinder;
        private AddressTokenizer _addressTokenizer = new AddressTokenizer();
        protected IObjectReader ObjectReader;

        /// <summary>
        /// Should memory read times be tracked and saved in <see cref="ReadTimes"/>?
        /// </summary>
        public bool WithTimes { get; set; }
        /// <summary>
        /// When <see cref="WithTimes"/> is true, stores per-prop read times
        /// </summary>
        public Dictionary<string, double> ReadTimes { get; } = new Dictionary<string, double>();

        public bool CanRead => _memoryReader.CurrentProcess != null;

        public event EventHandler<(object readObject, string propPath)> InvalidRead;
        public event EventHandler<(object readObject, string propPath)> IgnoredInvalidRead;

        public bool AbortReadOnInvalidValue { get; set; } = true;

        public int ProcessWatcherDelayMs
        {
            get => _memoryReader.ProcessWatcherDelayMs;
            set => _memoryReader.ProcessWatcherDelayMs = value;
        }

        private class DummyAddressChecker
        {
            [MemoryAddress("")] public int? AddressProp { get; set; }
        }

        protected class TypeCacheEntry
        {
            public Type Type { get; }
            public string ClassPath { get; }
            public List<PropInfo> Props { get; }
            public PropInfo ReadCheckerPropertyInfo { get; set; }

            public TypeCacheEntry(Type type, string classPath, List<PropInfo> props)
            {
                Type = type;
                ClassPath = classPath;
                Props = props;
            }
        }

        private Dictionary<object, TypeCacheEntry> typeCache =
            new Dictionary<object, TypeCacheEntry>();
        protected Dictionary<Type, object> DefaultValues = new Dictionary<Type, object>
        {
            { typeof(int) , -1 },
            { typeof(short) , (short)-1 },
            { typeof(ushort) , (ushort)0 },
            { typeof(float) , -1f },
            { typeof(double) , -1d },
            { typeof(bool) , false },
            { typeof(string) , null },
            { typeof(int[]) , null },
            { typeof(List<int>) , null },
        };
        protected Dictionary<Type, uint> SizeDictionary = new Dictionary<Type, uint>
        {
            { typeof(int) , 4 },
            { typeof(short) , 2 },
            { typeof(ushort) , 2 },
            { typeof(float) , 8 },
            { typeof(double) , 8 },
            { typeof(long) , 8 },
            { typeof(bool) , 1 },

            { typeof(string) , 0 },
            { typeof(int[]) , 0 },
            { typeof(List<int>) , 0 },
        };
        protected Dictionary<Type, ReadObject> ReadHandlers;

        public StructuredMemoryReader(string processName, Dictionary<string, string> baseAdresses, ProcessTargetOptions processTargetOptions, MemoryReader memoryReader = null, IObjectReader objectReader = null)
        {
            _memoryReader = memoryReader ?? new MemoryReader(processTargetOptions);
            ObjectReader = objectReader ?? new ObjectReader(_memoryReader);
            _addressFinder = new AddressFinder(_memoryReader, ObjectReader, baseAdresses);

            ReadHandlers = new Dictionary<Type, ReadObject>()
            {
                {typeof(int), (address,prop)=>ReadValueObject(address,prop, v => BitConverter.ToInt32(v, 0)) },
                {typeof(float), (address,prop)=>ReadValueObject(address,prop,v=> BitConverter.ToSingle(v, 0)) },
                {typeof(double), (address,prop)=>ReadValueObject(address,prop,v=> BitConverter.ToDouble(v, 0)) },
                {typeof(short), (address,prop)=>ReadValueObject(address,prop,v=> BitConverter.ToInt16(v, 0)) },
                {typeof(ushort), (address,prop)=>ReadValueObject(address,prop,v=> BitConverter.ToUInt16(v, 0)) },
                {typeof(bool), (address,prop)=>ReadValueObject(address,prop,v=> BitConverter.ToBoolean(v, 0)) },
                {typeof(long), (address,prop)=>ReadValueObject(address,prop,v=> BitConverter.ToInt64(v, 0)) },
                {typeof(string), (address,prop)=>ObjectReader.ReadUnicodeString(address) },
                {typeof(int[]), (address,prop)=>ObjectReader.ReadIntArray(address) },
                {typeof(List<int>), (address,prop)=>ObjectReader.ReadIntList(address) },
            };
        }

        public void AddReadHandlers(Dictionary<Type, ReadObject> readHandlers)
        {
            if (readHandlers != null)
            {
                foreach (var handler in readHandlers)
                    ReadHandlers.Add(handler.Key, handler.Value);
            }
        }

        public bool TryReadProperty(object readObj, string propertyName, out object result)
        {
            var cacheEntry = typeCache[PrepareReadObject(readObj, string.Empty)];
            var propInfo = cacheEntry.Props.FirstOrDefault(p => p.PropertyInfo.Name == propertyName);
            if (propInfo == null)
            {
                result = null;
                return false;
            }

            var resolvedProp = ResolveProp(readObj, IntPtr.Zero, propInfo, cacheEntry);
            result = resolvedProp.PropValue;
            return !resolvedProp.InvalidRead;
        }

        public bool TryRead<T>(T readObj) where T : class
        {
            if (readObj == null)
                throw new NullReferenceException();

            _addressFinder.ResetGroupReadCache();
            return TryInternalRead(readObj);
        }

        protected bool TryInternalRead<T>(T readObj, IntPtr? classAddress = null, string classPath = null) where T : class
        {
            var cacheEntry = typeCache[PrepareReadObject(readObj, classPath)];

            if (cacheEntry.ReadCheckerPropertyInfo != null)
            {
                var resolvedProp = ResolveProp(readObj, classAddress, cacheEntry.ReadCheckerPropertyInfo, cacheEntry);
                SetPropValue(cacheEntry.ReadCheckerPropertyInfo, resolvedProp.PropValue);
                classAddress = resolvedProp.ClassAdress;
                if (resolvedProp.InvalidRead || resolvedProp.PropValue == null || (int?)resolvedProp.PropValue == 0)
                    return false;
            }

            foreach (var prop in cacheEntry.Props)
            {
                Stopwatch readStopwatch = null;
                if (WithTimes)
                    readStopwatch = Stopwatch.StartNew();

                var result = ResolveProp(readObj, classAddress, prop, cacheEntry);
                classAddress = result.ClassAdress;
                if (result.InvalidRead)
                {
                    if (prop.IgnoreNullPtr)
                        IgnoredInvalidRead?.Invoke(this, (readObj, prop.Path));
                    else if (AbortReadOnInvalidValue)
                    {
                        readStopwatch?.Stop();
                        InvalidRead?.Invoke(this, (readObj, prop.Path));
                        return false;
                    }
                }

                SetPropValue(prop, result.PropValue);
                if (WithTimes && readStopwatch != null)
                {
                    readStopwatch.Stop();
                    var readTimeMs = readStopwatch.ElapsedTicks / (double)TimeSpan.TicksPerMillisecond;
                    ReadTimes[prop.Path] = readTimeMs;
                }
            }

            return true;
        }

        private (IntPtr? ClassAdress, bool InvalidRead, object PropValue) ResolveProp<T>(T readObj, IntPtr? classAddress, PropInfo prop, TypeCacheEntry cacheEntry) where T : class
        {
            if (prop.IsClass && !prop.IsStringOrArrayOrList)
            {
                var propValue = prop.PropertyInfo.GetValue(readObj);
                if (propValue == null)
                    return (classAddress, false, null);

                IntPtr? address = prop.MemoryPath == null
                    ? (IntPtr?)null
                    : ResolvePath(cacheEntry.ClassPath, prop.MemoryPath, classAddress).FinalAddress;

                if (address == IntPtr.Zero)
                    return (classAddress, !prop.IgnoreNullPtr, propValue);

                var readSuccessful = TryInternalRead(propValue, address, prop.Path);
                return (classAddress, !readSuccessful, propValue);
            }

            var result = ReadValueForPropInMemory(classAddress, prop, cacheEntry);
            return (result.ClassAddress, result.InvalidRead, result.Result);
        }

        private (object Result, IntPtr ClassAddress, bool InvalidRead) ReadValueForPropInMemory(IntPtr? classAddress, PropInfo prop, TypeCacheEntry cacheEntry)
        {
            var (propAddress, newClassAddress) = ResolvePath(cacheEntry.ClassPath, prop.MemoryPath, classAddress);
            var result = propAddress == IntPtr.Zero
                ? null
                : prop.Reader(propAddress, prop);

            return (result, newClassAddress, (prop.IsClass || prop.IsNullable) ? false : result == null);
        }

        private void SetPropValue(PropInfo prop, object result)
        {
            if (result != null || prop.IsNullable)
                prop.Setter(result);
            else if (DefaultValues.ContainsKey(prop.PropType))
                prop.Setter(DefaultValues[prop.PropType]);
        }

        private T PrepareReadObject<T>(T readObject, string classPath)
        {
            if (typeCache.ContainsKey(readObject)) return readObject;

            var type = readObject.GetType();
            var classProps = type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var classMemoryAddressAttribute = type.GetCustomAttribute(typeof(MemoryAddressAttribute), true) as MemoryAddressAttribute;

            typeCache[readObject] = new TypeCacheEntry(type, classMemoryAddressAttribute?.RelativePath, new List<PropInfo>());

            if (classMemoryAddressAttribute != null && classMemoryAddressAttribute.CheckClassAddress)
            {
                var dummyProp = typeof(DummyAddressChecker).GetProperty(nameof(DummyAddressChecker.AddressProp));
                if (string.IsNullOrEmpty(classMemoryAddressAttribute.CheckClassAddressPropName))
                    typeCache[readObject].ReadCheckerPropertyInfo = CreatePropInfo(readObject, dummyProp, classPath, type.Name, true);
                else
                    typeCache[readObject].ReadCheckerPropertyInfo = CreatePropInfo(readObject, classProps.First(x => x.Name == classMemoryAddressAttribute.CheckClassAddressPropName), classPath, type.Name);
            }

            foreach (var prop in classProps.Where(x => x.Name != classMemoryAddressAttribute?.CheckClassAddressPropName))
            {
                var propInfo = CreatePropInfo(readObject, prop, classPath, type.Name);
                if (propInfo != null)
                    typeCache[readObject].Props.Add(propInfo);
            }

            return readObject;
        }

        private PropInfo CreatePropInfo<T>(T readObject, PropertyInfo propertyInfo, string classPath, string parentTypeName, bool classAddressGetter = false)
        {
            if (!(propertyInfo.GetCustomAttribute(typeof(MemoryAddressAttribute), true) is MemoryAddressAttribute memoryAddressAttribute))
                return null;

            string finalPath = $"{parentTypeName}.{propertyInfo.Name}";
            if (!string.IsNullOrEmpty(classPath))
                finalPath = $"{classPath}.{finalPath}";

            var underlyingType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;

            var isString = propertyInfo.PropertyType == typeof(string);

            if (classAddressGetter)
                return new PropInfo(finalPath, propertyInfo, propertyInfo.PropertyType, underlyingType, propertyInfo.PropertyType.IsClass,
                    isString || (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>)),
                    Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null, "", false, (v) => { }, () => null, ReadHandlers[underlyingType]);

            return new PropInfo(finalPath, propertyInfo, propertyInfo.PropertyType, underlyingType, propertyInfo.PropertyType.IsClass,
                isString || (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>)),
                Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null, memoryAddressAttribute.RelativePath, memoryAddressAttribute.IgnoreNullPtr, (v) => propertyInfo.SetValue(readObject, v),
                () => propertyInfo.GetValue(readObject), ReadHandlers.ContainsKey(underlyingType) ? ReadHandlers[underlyingType] : (propertyInfo.PropertyType.IsClass ? null : throw new NotImplementedException($"Reading of {underlyingType.FullName} is not implemented. Add read handler for this type.")));
        }

        private (IntPtr FinalAddress, IntPtr ClassAddress) ResolvePath(string classMemoryPath, string propMemoryPath,
            IntPtr? providedClassAddress)
        {
            IntPtr classAddress = IntPtr.Zero;
            if (providedClassAddress.HasValue && providedClassAddress.Value != IntPtr.Zero)
                classAddress = providedClassAddress.Value;
            else if (classMemoryPath != null)
            {
                var tokenizedClass = _addressTokenizer.Tokenize(classMemoryPath);
                classAddress = _addressFinder.FindAddress(tokenizedClass, IntPtr.Zero);
                if (classAddress == IntPtr.Zero) return (IntPtr.Zero, IntPtr.Zero);
            }

            IntPtr finalAddress = IntPtr.Zero;
            if (propMemoryPath != null)
            {
                var tokenizedProp = _addressTokenizer.Tokenize(propMemoryPath);
                finalAddress = _addressFinder.FindAddress(tokenizedProp, classAddress);
            }

            return (finalAddress, classAddress);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _memoryReader?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private object ReadValueObject<T>(IntPtr finalAddress, PropInfo propInfo, Func<byte[], T> converter)
        {
            var propValue = _memoryReader.ReadData(finalAddress, SizeDictionary[propInfo.UnderlyingPropType]);
            if (propValue == null)
                return null;

            return converter(propValue);
        }

        public delegate object ReadObject(IntPtr finalAddress, PropInfo propInfo);
    }
}