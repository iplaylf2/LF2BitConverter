using System;
using System.Collections.Generic;
using System.Text;

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
    }

    public enum CountBy
    {
        Byte, Item
    }
}
