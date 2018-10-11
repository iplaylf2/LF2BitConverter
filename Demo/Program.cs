using System;
using LF2BitConverter;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Person person = new Person
            {
                Age = 10,
                Name = new Byte[] { 1, 3, 4, 5 },
                Character = new[] { Character.幽默, Character.开朗 }
            };
            Byte[] bytes = BitConverterEX.LittleEndian.GetBytes(person);
            Int32 index = 0;
            Person newPerson = BitConverterEX.LittleEndian.ToObject<Person>(bytes, ref index);

            Mass mass = new Mass
            {
                people = new[] { person }
            };
            bytes = BitConverterEX.LittleEndian.GetBytes(mass);
            index = 0;
            Mass newMass = BitConverterEX.LittleEndian.ToObject<Mass>(bytes, ref index);
        }
    }
}
