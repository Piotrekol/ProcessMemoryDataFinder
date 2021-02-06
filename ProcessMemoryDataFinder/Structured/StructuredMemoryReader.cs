using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProcessMemoryDataFinder.API;
using ProcessMemoryDataFinder.Structured.Tokenizer;

namespace ProcessMemoryDataFinder.Structured
{
    public class StructuredMemoryReader : IDisposable
    {
        private MemoryReader _memoryReader;
        private AddressFinder _addressFinder;
        private AddressTokenizer _addressTokenizer = new AddressTokenizer();
        protected IObjectReader ObjectReader;
        private Dictionary<object, (Type Type, string ClassPath, List<PropInfo> Props)> typeCache =
            new Dictionary<object, (Type Type, string ClassPath, List<PropInfo> Props)>();
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

        public Dictionary<Type, uint> SizeDictionary = new Dictionary<Type, uint>
        {
            { typeof(int) , 4 },
            { typeof(short) , 2 },
            { typeof(ushort) , 2 },
            { typeof(float) , 8 },
            { typeof(double) , 8 },
            { typeof(bool) , 1 },

            { typeof(string) , 0 },
            { typeof(int[]) , 0 },
            { typeof(List<int>) , 0 },
        };
        public StructuredMemoryReader(string processName, Dictionary<string, string> baseAdresses, string mainWindowTitleHint = null)
        {
            _memoryReader = new MemoryReader(processName, mainWindowTitleHint);
            _addressFinder = new AddressFinder(_memoryReader, baseAdresses);
            ObjectReader = new ObjectReader(_memoryReader);
        }

        /// <summary>
        /// Reads data contained at <see cref="pattern"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public object Read<T>(string pattern) => ReadObjectAt(ResolvePath(null, pattern), typeof(T));

        /// <summary>
        /// Recursively reads all props in <see cref="T"/> hierarchy marked with <see cref="MemoryAddressAttribute"/>
        /// </summary>
        /// <typeparam name="T">class to read</typeparam>
        /// <param name="readObj"><see cref="T"/></param>
        /// <returns></returns>
        public T Read<T>(T readObj) where T : class
        {
            if (readObj == null)
                throw new NullReferenceException();

            _addressFinder.ResetGroupReadCache();
            return InternalRead(readObj);
        }

        protected T InternalRead<T>(T readObj) where T : class
        {
            var cacheEntry = typeCache[PrepareReadObject(readObj)];
            foreach (var prop in cacheEntry.Props)
            {
                if (prop.IsClass && !prop.IsStringOrArrayOrList)
                {
                    var propValue = prop.PropertyInfo.GetValue(readObj);
                    if (propValue == null)
                        continue;

                    InternalRead(propValue);
                }
                else
                {
                    var result = ReadObjectAt(ResolvePath(cacheEntry.ClassPath, prop.MemoryPath), prop.PropType);
                    prop.Setter(result ?? DefaultValues[prop.PropType]);
                }
            }
            return readObj;
        }

        private T PrepareReadObject<T>(T readObject)
        {
            if (typeCache.ContainsKey(readObject)) return readObject;

            var type = readObject.GetType();
            var classProps = type.GetProperties();
            var classMemoryPath = (string)readObject.GetType().GetCustomAttributesData().FirstOrDefault()?.ConstructorArguments[0].Value;
            typeCache[readObject] = (type, classMemoryPath, new List<PropInfo>());
            foreach (var prop in classProps)
            {
                var p = prop.GetCustomAttribute(typeof(MemoryAddressAttribute), true);
                if (p is MemoryAddressAttribute memoryAddressAttribute)
                {
                    Action<object> setterInvoker = v => prop.SetValue(readObject, v);

                    if (prop.Name.Contains("HitErrors"))
                    {
                        var a = prop.PropertyType.GetGenericTypeDefinition();
                        var ba = a== typeof(List<>);
                    }
                    typeCache[readObject].Props.Add(new PropInfo($"{type.Name}.{prop.Name}", prop, prop.PropertyType, prop.PropertyType.IsClass,
                        prop.PropertyType == typeof(string) || (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                        , memoryAddressAttribute.RelativePath, setterInvoker));
                }
            }

            return readObject;
        }

        protected virtual object ReadObjectAt(IntPtr finalAddress, Type type)
        {
            if (finalAddress == IntPtr.Zero) return null;

            if (type == typeof(string))
                return ObjectReader.ReadUnicodeString(finalAddress);
            if (type == typeof(int[]))
                return ObjectReader.ReadIntArray(finalAddress);
            if (type == typeof(List<int>))
                return ObjectReader.ReadIntList(finalAddress);

            var propValue = _memoryReader.ReadData(finalAddress, SizeDictionary[type]);
            if (propValue == null)
                return null;
            
            if (type == typeof(int))
                return BitConverter.ToInt32(propValue, 0);
            if (type == typeof(float))
                return BitConverter.ToSingle(propValue, 0);
            if (type == typeof(double))
                return BitConverter.ToDouble(propValue, 0);
            if (type == typeof(ushort))
                return BitConverter.ToUInt16(propValue, 0);
            if (type == typeof(bool))
                return BitConverter.ToBoolean(propValue, 0);
            return null;
        }

        private IntPtr ResolvePath(string classMemoryPath, string propMemoryPath)
        {
            IntPtr classAddress = IntPtr.Zero;
            if (classMemoryPath != null)
            {
                var tokenizedClass = _addressTokenizer.Tokenize(classMemoryPath);
                classAddress = _addressFinder.FindAddress(tokenizedClass, IntPtr.Zero);
                if (classAddress == IntPtr.Zero) return IntPtr.Zero;
            }

            var tokenizedProp = _addressTokenizer.Tokenize(propMemoryPath);
            var finalAddress = _addressFinder.FindAddress(tokenizedProp, classAddress);
            if (finalAddress == IntPtr.Zero) return IntPtr.Zero;
            return finalAddress;
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
    }
}