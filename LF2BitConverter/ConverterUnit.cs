using System;
using System.Collections.Generic;
using System.Text;

namespace LF2BitConverter
{
    struct ConverterUint
    {
        public Delegate GetBytes;
        public Delegate ToObject;

        public ConverterUnit<T> MakeGenericType<T>()
        {
            return new ConverterUnit<T>(GetBytes, ToObject);
        }
    }

    class ConverterUnit<T>
    {
        public ConverterUnit(Delegate getBytes, Delegate toObject)
        {
            GetBytesDelegate = (GetBytesDelegate<T>)getBytes;
            ToObjectDelegate = (ToObjectDelegate<T>)toObject;
        }

        public Byte[] GetBytes(T value)
        {
            return GetBytesDelegate(value);
        }

        public T ToObject(Byte[] value, ref Int32 startIndex)
        {
            return ToObjectDelegate(value, ref startIndex);
        }

        private readonly GetBytesDelegate<T> GetBytesDelegate;
        private readonly ToObjectDelegate<T> ToObjectDelegate;
    }

    delegate Byte[] GetBytesDelegate<T>(T value);
    delegate T ToObjectDelegate<T>(Byte[] value, ref Int32 startIndex);
}
