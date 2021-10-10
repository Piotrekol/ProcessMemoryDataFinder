using System;
using System.Reflection;

namespace ProcessMemoryDataFinder.Structured
{
    public class PropInfo
    {
        public string Path { get; }
        public PropertyInfo PropertyInfo { get; }
        public Type PropType { get; }
        public Type UnderlyingPropType { get; }
        public bool IsClass { get; }
        public bool IsStringOrArrayOrList { get; }
        public bool IsNullable { get; }
        public string MemoryPath { get; }
        public bool IgnoreNullPtr { get; }
        public Action<object> Setter { get; }
        public Func<object> Getter { get; }
        public StructuredMemoryReader.ReadObject Reader { get; }

        public PropInfo(string path, PropertyInfo propertyInfo, Type propType, Type underlyingPropType, bool isClass, bool isStringOrArrayOrList,
            bool isNullable, string memoryPath, bool ignoreNullPtr, Action<object> setter, Func<object> getter, StructuredMemoryReader.ReadObject reader)
        {
            Path = path;
            PropertyInfo = propertyInfo;
            PropType = propType;
            UnderlyingPropType = underlyingPropType;
            IsClass = isClass;
            IsStringOrArrayOrList = isStringOrArrayOrList;
            IsNullable = isNullable;
            MemoryPath = memoryPath;
            IgnoreNullPtr = ignoreNullPtr;
            Setter = setter;
            Getter = getter;
            Reader = reader;
        }
    }
}