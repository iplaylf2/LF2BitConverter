using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LF2BitConverter.Extenison
{
    static class BitConverterSelector
    {
        public static MethodInfo SelectGetBytes(Type type, Boolean littleEndian)
        {
            return (littleEndian == LittleEndian ? typeof(BitConverter) : typeof(BitConverterR)).GetMethod(nameof(BitConverter.GetBytes), new[] { type });
        }

        public static MethodInfo SelectToObject(Type type, Boolean littleEndian)
        {
            return (littleEndian == LittleEndian ? typeof(BitConverter) : typeof(BitConverterR)).GetMethods()
                .Where(m => m.ReturnType == type)
                .Single(m => m.GetParameters().FirstOrDefault()?.ParameterType == typeof(Byte[]));
        }

        private static Boolean LittleEndian = BitConverter.IsLittleEndian;
    }
}
