using System;
using System.Collections.Concurrent;

namespace LF2BitConverter.Builder
{
    class ConverterBuilderManager
    {
        public Boolean ContainsBuilder(Type convertType)
        {
            return BuilderMap.ContainsKey(convertType);
        }

        public ConverterBuilder GetOrAddBuilder(Type convertType)
        {
            if (!ContainsBuilder(convertType))
            {
                AddBuilder(convertType, new ConverterBuilderAssistant(convertType, this));
            }
            return BuilderMap[convertType];
        }

        public void AddBuilder(Type convertType, ConverterBuilderAssistant assistant)
        {
            BuilderMap.TryAdd(convertType, new ConverterBuilder(convertType, assistant));
        }

        public ConverterBuilder GetAddBuilder(Type convertType)
        {
            return BuilderMap[convertType];
        }

        private readonly ConcurrentDictionary<Type, ConverterBuilder> BuilderMap = new ConcurrentDictionary<Type, ConverterBuilder>();
    }
}