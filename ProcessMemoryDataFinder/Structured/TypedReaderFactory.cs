using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ProcessMemoryDataFinder.API;

namespace ProcessMemoryDataFinder.Structured
{
    internal static class TypedReaderFactory
    {
        public static Func<IntPtr, bool>? CreateTypedReadAndSet<T>(
            PropertyInfo propertyInfo,
            object readObject,
            MemoryReaderManager memoryReader,
            IReadOnlyDictionary<Type, object> defaultValues)
            where T : unmanaged
        {
            var setter = TryCreateSetter<T>(propertyInfo, readObject);
            if (setter == null)
                return null;

            T defaultValue = defaultValues.TryGetValue(typeof(T), out var defObj) && defObj is T def ? def : default;
            var buffer = new byte[Unsafe.SizeOf<T>()];

            return (address) =>
            {
                if (address == IntPtr.Zero)
                {
                    setter(defaultValue);
                    return false;
                }

                if (memoryReader.ReadData(address, buffer))
                {
                    setter(MemoryMarshal.Read<T>(buffer));
                    return true;
                }

                setter(defaultValue);
                return false;
            };
        }

        public static Func<IntPtr, bool>? CreateTypedStringReadAndSet(
            PropertyInfo propertyInfo,
            object readObject,
            IObjectReader objectReader)
        {
            /// <summary>
            /// How often to re-verify string content when the pointer has changed recently or a read has failed.
            /// </summary>
            const int UnstableVerifyEvery = 3;
            /// <summary>
            /// After this many consecutive reads returning the same content, the string is considered stable and verification switches to the longer interval.
            /// </summary>
            const int StableThreshold = 5;
            /// <summary>
            /// How often to re-verify string content once stable, assuming no pointer changes.
            /// </summary>
            const int StableVerifyEvery = 120;

            var setter = TryCreateSetter<string?>(propertyInfo, readObject);
            if (setter == null)
                return null;

            IntPtr lastPtr = IntPtr.Zero;
            string? lastValue = null;
            int framesSinceVerify = int.MaxValue;
            int consecutiveSame = 0;

            return (fieldAddress) =>
            {
                if (fieldAddress == IntPtr.Zero)
                {
                    if (lastPtr != IntPtr.Zero)
                    {
                        setter(null);
                        lastPtr = IntPtr.Zero;
                        lastValue = null;
                        framesSinceVerify = int.MaxValue;
                        consecutiveSame = 0;
                    }
                    return true;
                }

                var currentPtr = objectReader.ReadPointer(fieldAddress);

                if (currentPtr != lastPtr)
                {
                    framesSinceVerify = int.MaxValue;
                    consecutiveSame = 0;
                }

                var verifyInterval = consecutiveSame < StableThreshold ? UnstableVerifyEvery : StableVerifyEvery;
                if (framesSinceVerify >= verifyInterval)
                {
                    var value = objectReader.ReadUnicodeString(fieldAddress);
                    framesSinceVerify = 0;

                    if (value != null)
                    {
                        consecutiveSame = (lastValue == value) ? consecutiveSame + 1 : 1;
                        lastPtr = currentPtr;
                        lastValue = value;
                        setter(value);
                    }
                    else
                    {
                        consecutiveSame = 0;
                    }
                }
                else
                {
                    framesSinceVerify++;
                }

                return true;
            };
        }

        public static Func<IntPtr, bool>? CreateTypedListReadAndSet(
            PropertyInfo propertyInfo,
            object readObject,
            IObjectReader objectReader)
        {
            var getter = TryCreateGetter<List<int>>(propertyInfo, readObject);
            var setter = TryCreateSetter<List<int>>(propertyInfo, readObject);
            if (getter == null || setter == null)
                return null;

            return (fieldAddress) =>
            {
                if (fieldAddress == IntPtr.Zero)
                {
                    setter(null);
                    return true;
                }

                var list = getter();
                if (list == null)
                {
                    list = new List<int>();
                    setter(list);
                }

                if (!objectReader.TryReadIntList(fieldAddress, list))
                    setter(null);

                return true;
            };
        }

        private static Action<T>? TryCreateSetter<T>(PropertyInfo prop, object target)
        {
            var setMethod = prop.GetSetMethod(nonPublic: true);
            return setMethod != null ? (Action<T>)setMethod.CreateDelegate(typeof(Action<T>), target) : null;
        }

        private static Func<T>? TryCreateGetter<T>(PropertyInfo prop, object target)
        {
            var getMethod = prop.GetGetMethod(nonPublic: true);
            return getMethod != null ? (Func<T>)getMethod.CreateDelegate(typeof(Func<T>), target) : null;
        }
    }
}
