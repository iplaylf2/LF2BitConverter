using System;
using System.Collections.Generic;
using System.Text;

namespace LF2BitConverter.Builder
{
    class ConverterBuilderAssistant
    {
        public ConverterBuilderAssistant(Type convertType, ConverterBuilderManager builderManager)
        {
            Record.Add(convertType);
            BuilderManager = builderManager;
        }

        public ConverterBuilder GetOrAddBuilder(Type convertType)
        {
            if (!BuilderManager.ContainsBuilder(convertType))
            {
                if (Record.Contains(convertType))
                {
                    throw new Exception("不允许循环引用");
                }
                else
                {
                    Record.Add(convertType);
                    BuilderManager.AddBuilder(convertType, this);
                }
            }
            return BuilderManager.GetBuilder(convertType);
        }

        private readonly ConverterBuilderManager BuilderManager;
        private readonly HashSet<Type> Record = new HashSet<Type>();
    }
}
