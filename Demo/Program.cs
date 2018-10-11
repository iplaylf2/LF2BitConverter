using System;
using LF2BitConverter;
using LF2BitConverter.ConvertMemberAttributeNS;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var person = new Person
            {
                Age = 10,
                Name = new Byte[] { 1, 3, 4, 5 },
                Character = new[] { Character.幽默, Character.开朗 }
            };
            var bytes = BitConverterEX.LittleEndian.GetBytes(person);
            var index = 0;
            var obj = BitConverterEX.LittleEndian.ToObject<Person>(bytes, ref index);
        }
    }
}
