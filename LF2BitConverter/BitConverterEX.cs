using System;
using System.Collections.Generic;
using System.Text;

namespace LF2BitConverter
{
    public sealed class BitConverterEX
    {
        public static BitConverterEX LittleEndian { get; } = new BitConverterEX(true);
        public static BitConverterEX BigEndian { get; } = new BitConverterEX(false);

        public Byte[] GetBytes<T>(T obj)
        {
            var getBytes = ConverterService.GetGetBytes<T>();
            return getBytes(obj);
        }

        public T ToObject<T>(Byte[] bytes, ref Int32 startIndex)
        {
            var toObject = ConverterService.GetToObject<T>();
            return toObject(bytes, ref startIndex);
        }

        internal BitConverterEX(Boolean littleEndian)
        {
            ConverterService = new ConverterService(littleEndian);
        }

        private ConverterService ConverterService;
    }
}
