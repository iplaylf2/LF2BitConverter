using System;
using System.Collections.Generic;
using System.Text;

namespace LF2BitConverter.ConvertMemberAttributeNS
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class FixedAmountAttribute : ConvertMemberAttribute
    {
    }
}
