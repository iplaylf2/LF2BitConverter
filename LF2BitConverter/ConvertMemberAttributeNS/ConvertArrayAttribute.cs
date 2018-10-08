using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using LF2BitConverter.ExpressionGenerator;

namespace LF2BitConverter.ConvertMemberAttributeNS
{
    public class ConvertArrayAttribute : ConvertMemberAttribute
    {
        public CountBy CountBy { get; }
        public String LengthFrom { get; set; }
        public Int32 Length { get; set; }

        public ConvertArrayAttribute(CountBy countBy)
        {
            CountBy = CountBy;
        }

        public override Expression OnCreateGetBytes(ParameterExpression value, bool littleEndian, GeneratorContext context, Expression lastResult)
        {
            if (String.IsNullOrEmpty(LengthFrom) && CountBy == CountBy.Item)
            {
                var count = Expression.Variable(typeof(Int32));
                context.VariableMap.Add(ItemCountInGetBytesKey, count);
            }
            return lastResult;
        }

        public override void AfterCreateGetBytes(GeneratorContext context)
        {
        }

        private Expression ItemCountInGetBytes;
        private String ItemCountInGetBytesKey = Guid.NewGuid().ToString();

    }

    public enum CountBy
    {
        Byte, Item
    }
}
