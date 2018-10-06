using System;
using System.Reflection;
using System.Linq;
using LF2BitConverter.ConvertMemberAttributeNS;
using LF2BitConverter.ConvertMemberNS;

namespace LF2BitConverter.Builder
{
    class ConverterBuilder
    {
        public GetBytesGenerator LittleEndianGetBytes { get; }
        public GetBytesGenerator BigEndianGetBytes { get; }
        public ToObjectGenerator LittleEndianToObject { get; }
        public ToObjectGenerator BigEndianToObject { get; }

        public ConverterBuilder(Type type, ConverterBuilderAssistant assistant)
        {
            ConvertType = type;
            Assistant = assistant;
            var convertMembers = ConvertMemberAnalyse();
            LittleEndianGetBytes = new GetBytesGenerator(type, convertMembers, Assistant, true);
            BigEndianGetBytes = new GetBytesGenerator(type, convertMembers, Assistant, false);
            LittleEndianToObject = new ToObjectGenerator(type, convertMembers, Assistant, true);
            BigEndianToObject = new ToObjectGenerator(type, convertMembers, Assistant, false);
        }

        public ConverterUint Build(Boolean littleEndian)
        {
            return new ConverterUint
            {
                GetBytes = BuildGetBytes(littleEndian),
                ToObject = BuildToObejct(littleEndian)
            };
        }

        private readonly Type ConvertType;
        private readonly ConverterBuilderAssistant Assistant;

        private ConvertMember[] ConvertMemberAnalyse()
        {
            var validMembers = ConvertType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(member =>
                    {
                        switch (member)
                        {
                            case PropertyInfo property:
                                return property.CanRead && property.CanWrite;
                            case FieldInfo field:
                                return !field.IsInitOnly;
                            default:
                                return false;
                        }
                    });
            var convertMembers = validMembers.Select(member =>
            {
                var convertMemberAttribute = member.GetCustomAttribute<ConvertMemberAttribute>();
                if (convertMemberAttribute == null)
                {
                    return new NormalConvertAttribute().Analyse(member);
                }
                else
                {
                    return convertMemberAttribute.Analyse(member);
                }
            });
            foreach (var member in convertMembers.Where(m => !m.IsPrimitive))
            {
                Assistant.CheckCycle(member.OrginType);
            }
            return convertMembers.ToArray();
        }

        private Delegate BuildGetBytes(Boolean littleEndian)
        {
            return (littleEndian ? LittleEndianGetBytes : BigEndianGetBytes).GetOrCreateExpression().Compile();
        }

        private Delegate BuildToObejct(Boolean littleEndian)
        {
            return (littleEndian ? LittleEndianToObject : BigEndianToObject).GetOrCreateExpression().Compile();
        }
    }
}
