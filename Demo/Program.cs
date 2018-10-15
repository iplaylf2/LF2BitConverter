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
                Name = "小明",
                Character = new[] { Character.幽默, Character.开朗 }
            };
            Byte[] bytes_1 = BitConverterEX.LittleEndian.GetBytes(person);
            Int32 index_1 = 0;
            Person newPerson = BitConverterEX.LittleEndian.ToObject<Person>(bytes_1, ref index_1);

            Mass mass = new Mass
            {
                people = new[] { person }
            };
            Byte[] bytes_2 = BitConverterEX.LittleEndian.GetBytes(mass);
            Int32 index_2 = 0;
            Mass newMass = BitConverterEX.LittleEndian.ToObject<Mass>(bytes_2, ref index_2);
        }
    }
}
