﻿using System;
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
                D = new[] { 7, 8, 9, 10 }
            };
            var byets = BitConverterEX.LittleEndian.GetBytes(mock);
        }
    }

    class Mock
    {
        public Foo A;
        public Int32 B;
        public Foo[] C;
        public Int32[] D;
    }

    class Foo
    {
        public Int32 A;
        public Int32 B;
    }
}
