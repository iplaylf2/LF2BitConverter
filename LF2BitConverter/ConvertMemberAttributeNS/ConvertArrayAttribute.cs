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

        public override void AfterCreateGetBytes(GeneratorContext context)
        {
        }
    }

    public enum CountBy
    {
        Byte, Item
    }
}
