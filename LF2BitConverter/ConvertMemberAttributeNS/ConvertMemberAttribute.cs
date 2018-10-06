using LF2BitConverter.Builder;
using LF2BitConverter.ConvertMemberNS;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LF2BitConverter.ConvertMemberAttributeNS
{
    public abstract class ConvertMemberAttribute : Attribute
    {
        protected ConvertMemberAttribute()
        {
        }

        internal abstract ConvertMember Analyse(MemberInfo member);
    }
}
