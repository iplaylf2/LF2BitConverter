using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LF2BitConverter.Extenison
{
    class BitConverterR
    {
        public static byte[] GetBytes(bool value)
        {
            return BitConverter.GetBytes(value).Reverse().ToArray();
        }
        public static byte[] GetBytes(char value)
        {
            return BitConverter.GetBytes(value).Reverse().ToArray();
        }
        public static byte[] GetBytes(double value)
        {
            return BitConverter.GetBytes(value).Reverse().ToArray();
        }
        public static byte[] GetBytes(short value)
        {
            return BitConverter.GetBytes(value).Reverse().ToArray();
        }
        public static byte[] GetBytes(int value)
        {
            return BitConverter.GetBytes(value).Reverse().ToArray();
        }
        public static byte[] GetBytes(long value)
        {
            return BitConverter.GetBytes(value).Reverse().ToArray();
        }
        public static byte[] GetBytes(float value)
        {
            return BitConverter.GetBytes(value).Reverse().ToArray();
        }
        public static byte[] GetBytes(ushort value)
        {
            return BitConverter.GetBytes(value).Reverse().ToArray();
        }
        public static byte[] GetBytes(uint value)
        {
            return BitConverter.GetBytes(value).Reverse().ToArray();
        }
        public static byte[] GetBytes(ulong value)
        {
            return BitConverter.GetBytes(value).Reverse().ToArray();
        }

        public static bool ToBoolean(byte[] value, int startIndex)
        {
            return BitConverter.ToBoolean(value.Skip(startIndex).Take(sizeof(bool)).Reverse().ToArray(), 0);
        }
        public static char ToChar(byte[] value, int startIndex)
        {
            return BitConverter.ToChar(value.Skip(startIndex).Take(sizeof(char)).Reverse().ToArray(), 0);
        }
        public static double ToDouble(byte[] value, int startIndex)
        {
            return BitConverter.ToDouble(value.Skip(startIndex).Take(sizeof(double)).Reverse().ToArray(), 0);
        }
        public static short ToInt16(byte[] value, int startIndex)
        {
            return BitConverter.ToInt16(value.Skip(startIndex).Take(sizeof(short)).Reverse().ToArray(), 0);
        }
        public static int ToInt32(byte[] value, int startIndex)
        {
            return BitConverter.ToInt32(value.Skip(startIndex).Take(sizeof(int)).Reverse().ToArray(), 0);
        }
        public static long ToInt64(byte[] value, int startIndex)
        {
            return BitConverter.ToInt64(value.Skip(startIndex).Take(sizeof(long)).Reverse().ToArray(), 0);
        }
        public static float ToSingle(byte[] value, int startIndex)
        {
            return BitConverter.ToSingle(value.Skip(startIndex).Take(sizeof(float)).Reverse().ToArray(), 0);
        }
        public static ushort ToUInt16(byte[] value, int startIndex)
        {
            return BitConverter.ToUInt16(value.Skip(startIndex).Take(sizeof(ushort)).Reverse().ToArray(), 0);
        }
        public static uint ToUInt32(byte[] value, int startIndex)
        {
            return BitConverter.ToUInt32(value.Skip(startIndex).Take(sizeof(uint)).Reverse().ToArray(), 0);
        }
        public static ulong ToUInt64(byte[] value, int startIndex)
        {
            return BitConverter.ToUInt64(value.Skip(startIndex).Take(sizeof(ulong)).Reverse().ToArray(), 0);
        }
    }
}
