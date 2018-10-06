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

        public ConverterUnit<T> GetConverter<T>()
        {
            var type = typeof(T);
            if (!ConverterMap.ContainsKey(type))
            {
                var builder = BuilderManager.GetOrAddBuilder(type);
                var converter = builder.Build(LittleEndian);
                ConverterMap.TryAdd(type, converter.MakeGenericType<T>());
            }
            return (ConverterUnit<T>)ConverterMap[type];
        }

        private readonly static ConverterBuilderManager BuilderManager = new ConverterBuilderManager();
        private readonly ConcurrentDictionary<Type, Object> ConverterMap = new ConcurrentDictionary<Type, Object>();
        private readonly Boolean LittleEndian;
    }
}