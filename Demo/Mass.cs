using LF2BitConverter.ConvertMemberAttributeNS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Demo
{
    class Mass
    {
        public Int32 Count { get; set; }

        /// <summary>
        /// 支持不定数量的数组，需要指定存放数量的成员
        /// </summary>
        [ConvertArray(CountBy.Item, LengthFrom = nameof(Count))]
        public Person[] people;
    }
}
