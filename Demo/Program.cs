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
            var newPerson = BitConverterEX.LittleEndian.ToObject<Person>(bytes, ref index);

            var mass = new Mass
            {
                people = new[] { person }
            };
            bytes = BitConverterEX.LittleEndian.GetBytes(mass);
            index = 0;
            var newMass = BitConverterEX.LittleEndian.ToObject<Mass>(bytes, ref index);
        }
    }
}
