using LF2BitConverter.ConvertMemberAttributeNS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Demo
{
    /// <summary>
    /// 个人
    /// </summary>
    class Person
    {
        public Int32 Age { get; set; }

        public Int32 NameLength { get; set; }

        /// <summary>
        /// 不定长度的数组需要指定存放长度的字段
        /// CountBy.Byte是计算字段转换为Bytes时的长度
        /// </summary>
        [ConvertArray(CountBy.Byte, LengthFrom = nameof(NameLength))]
        public Byte[] Name { get; set; }

        /// <summary>
        /// 允许多个转换特性同时使用
        /// 支持固定长度。
        /// CountBy.Item是计算字段转换前的实例个数
        /// </summary>
        [ConvertAs(typeof(Byte))]
        [ConvertArray(CountBy.Item, Length = 2)]
        public Character[] Character { get; set; }
    }
}
