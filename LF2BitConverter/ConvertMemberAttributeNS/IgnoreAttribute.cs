using System;
using System.Collections.Generic;
using System.Text;
using LF2BitConverter.ExpressionGenerator;

namespace LF2BitConverter.ConvertMemberAttributeNS
{
    public class IgnoreAttribute : ConvertMemberAttribute
    {
        public override void AfterCreateGetBytes(GeneratorContext context)
        {
            var assgin = context.MemberResult.Find(item => item.Item1 == Member.Name);
            context.MemberResult.Remove(assgin);
        }

        public override void AfterToObject(GeneratorContext context)
        {
            var assgin = context.MemberResult.Find(item => item.Item1 == Member.Name);
            context.MemberResult.Remove(assgin);
        }
    }
}
