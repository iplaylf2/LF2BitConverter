using LF2BitConverter.ConvertMemberNS;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace LF2BitConverter.Builder
{
    abstract class ExpressionGenerator
    {
        public ExpressionGenerator(Type type, ConvertMember[] convertMemberArray, ConverterBuilderAssistant assistant, Boolean littleEndian)
        {
            ConvertType = type;
            ConvertMemberArray = convertMemberArray;
            Assistant = assistant;
            LittleEndian = littleEndian;
        }

        public LambdaExpression GetOrCreateExpression()
        {
            if (Cache == null)
            {
                Cache = CreateExpression();
            }
            return Cache;
        }

        protected readonly Type ConvertType;
        protected readonly ConvertMember[] ConvertMemberArray;
        protected readonly ConverterBuilderAssistant Assistant;
        protected readonly Boolean LittleEndian;

        protected abstract LambdaExpression CreateExpression();

        private LambdaExpression Cache;
    }
}
