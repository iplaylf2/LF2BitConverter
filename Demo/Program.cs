using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using LF2BitConverter;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var mock = new Mock
            {
                A = new Foo
                {
                    A = 1,
                    B = 2
                },
                B = 3,
                C = new[]
                {
                    new Foo
                    {
                        A=4,
                        B=5
                    },
                    new Foo
                    {
                        A=6,
                        B=7
                    }
                },
                D = new[] { 7, 8, 9, 10 },
                E = Bar.B
            };
            var byets = BitConverterEX.LittleEndian.GetBytes(mock);
            var index = 0;
            var obj = BitConverterEX.LittleEndian.ToObject<Mock>(byets,ref index);
        }
    }

    class Mock
    {
        public Foo A;
        public Int32 B;
        public Foo[] C;
        public Int32[] D;
        public Bar E;
    }

    class Foo
    {
        public Int32 A;
        public Int32 B;
    }

    enum Bar
    {
        A, B
    }
}
