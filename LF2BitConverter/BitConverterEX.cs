using System;
using System.Collections.Generic;
using System.Text;

namespace LF2BitConverter
{
    /// <summary>
    /// BitConverter扩展版。用来Byte[]与对象互转。
    /// </summary>
    public sealed class BitConverterEX
    {
        public static BitConverterEX LittleEndian { get; } = new BitConverterEX(true);
        public static BitConverterEX BigEndian { get; } = new BitConverterEX(false);

        public Byte[] GetBytes<T>(T value)
        {
            var converter = ConverterService.GetConverter<T>();
            return converter.GetBytes(value);
        }

        public T ToObject<T>(Byte[] value, ref Int32 startIndex)
        {
            var converter = ConverterService.GetConverter<T>();
            return converter.ToObject(value, ref startIndex);
        }

        internal BitConverterEX(Boolean littleEndian)
        {
            ConverterService = new ConverterService(littleEndian);
        }

        private ConverterService ConverterService;
    }
}
