using System;
using System.Collections.Generic;
using System.Text;
using LF2BitConverter.ExpressionGenerator;

namespace LF2BitConverter.ConvertMemberAttributeNS
{
    public class ConvertIgnoreAttribute : ConvertMemberAttribute
    {
        public override void AfterAllCreateGetBytes(GeneratorContext context)
        {
            var assgin = context.MemberResult.Find(item => item.Item1 == Member.Name);
            context.MemberResult.Remove(assgin);
        }

        public override void AfterAllCreateToObject(GeneratorContext context)
        {
            var assgin = context.MemberResult.Find(item => item.Item1 == Member.Name);
            context.MemberResult.Remove(assgin);
        }
    }
}
