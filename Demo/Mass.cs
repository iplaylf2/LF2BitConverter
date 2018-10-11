using LF2BitConverter.ConvertMemberAttributeNS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Demo
{
    class Mass
    {
        public Int32 Count { get; set; }

        [ConvertArray(CountBy.Item, LengthFrom = nameof(Count))]
        public Person[] people;
    }
}
