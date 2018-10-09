using LF2BitConverter.Builder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace LF2BitConverter
{
    class ConverterService
    {
        public ConverterService(Boolean littleEndian)
        {
            LittleEndian = littleEndian;
        }

        public GetBytesDelegate<T> GetGetBytes<T>()
        {
            var type = typeof(T);
            if (!GetBytesMap.ContainsKey(type))
            {
                var builder = BuilderManager.GetOrAddBuilder(type);
                GetBytesMap.TryAdd(type, builder.BuildGetBytes(LittleEndian));
            }
            return (GetBytesDelegate<T>)GetBytesMap[type];
        }

        public ToObjectDelegate<T> GetToObject<T>()
        {
            var type = typeof(T);
            if (!ToObjectMap.ContainsKey(type))
            {
                var builder = BuilderManager.GetOrAddBuilder(type);
                ToObjectMap.TryAdd(type, builder.BuildToObject(LittleEndian));
            }
            return (ToObjectDelegate<T>)ToObjectMap[type];
        }

        private readonly static ConverterBuilderManager BuilderManager = new ConverterBuilderManager();
        private readonly ConcurrentDictionary<Type, Delegate> GetBytesMap = new ConcurrentDictionary<Type, Delegate>();
        private readonly ConcurrentDictionary<Type, Delegate> ToObjectMap = new ConcurrentDictionary<Type, Delegate>();
        private readonly Boolean LittleEndian;
    }

    delegate Byte[] GetBytesDelegate<T>(T obj);
    delegate T ToObjectDelegate<T>(Byte[] bytes, ref Int32 startIndex);
}