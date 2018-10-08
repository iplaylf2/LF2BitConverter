using System;
using System.Collections.Generic;
using System.Text;
using LF2BitConverter.ExpressionGenerator;

namespace LF2BitConverter.ConvertMemberAttributeNS
{
    public class ConvertAsAttribute : ConvertMemberAttribute
    {
        public ConvertAsAttribute(Type type)
        {
            ConvertType = type;
        }
        public override Type OnGetConvertType(Type lastResult)
        {
            return ConvertType;
        }

        private Type ConvertType;
    }
}
