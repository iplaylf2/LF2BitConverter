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

        public Byte[] GetBytes(T obj)
        {
            return GetBytesDelegate(obj);
        }

        public T ToObject(Byte[] bytes, ref Int32 startIndex)
        {
            return ToObjectDelegate(bytes, ref startIndex);
        }

        private readonly GetBytesDelegate<T> GetBytesDelegate;
        private readonly ToObjectDelegate<T> ToObjectDelegate;
    }

    delegate Byte[] GetBytesDelegate<T>(T obj);
    delegate T ToObjectDelegate<T>(Byte[] bytes, ref Int32 startIndex);
}
