using LF2BitConverter.ConvertMemberAttributeNS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Demo
{
    class Person
    {
        public Int32 Age { get; set; }

        public Int32 NameLength { get; set; }

        /// <summary>
        /// 支持字符串，可以指定编码
        /// 需要指定存放长度的成员
        /// </summary>
        [ConvertString("utf-8", nameof(NameLength))]
        public String Name { get; set; }

        /// <summary>
        /// 允许多个转换特性同时使用
        /// 支持固定长度
        /// CountBy.Item是计算成员转换前的实例个数
        /// </summary>
        [ConvertAs(typeof(Byte))]
        [ConvertArray(CountBy.Item, Length = 2)]
        public Character[] Character { get; set; }
    }
}
