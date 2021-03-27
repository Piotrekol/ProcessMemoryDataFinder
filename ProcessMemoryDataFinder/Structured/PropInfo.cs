using System;
using System.Reflection;

namespace ProcessMemoryDataFinder.Structured
{
    public class PropInfo
    {
        public string Path { get; }
        public PropertyInfo PropertyInfo { get; }
        public Type PropType { get; }
        public bool IsClass { get; }
        public bool IsStringOrArrayOrList { get; }
        public bool IsNullable { get; }
        public string MemoryPath { get; }
        public Action<object> Setter { get; }
        public Func<object> Getter { get; }

        public PropInfo(string path, PropertyInfo propertyInfo, Type propType, bool isClass, bool isStringOrArrayOrList,
            bool isNullable, string memoryPath, Action<object> setter, Func<object> getter)
        {
            Path = path;
            PropertyInfo = propertyInfo;
            PropType = propType;
            IsClass = isClass;
            IsStringOrArrayOrList = isStringOrArrayOrList;
            IsNullable = isNullable;
            MemoryPath = memoryPath;
            Setter = setter;
            Getter = getter;
        }
    }
}