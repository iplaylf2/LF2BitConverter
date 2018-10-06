using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using LF2BitConverter.ConvertMemberNS;

namespace LF2BitConverter.Builder
{
    class GetBytesGenerator : ExpressionGenerator
    {
        public GetBytesGenerator(Type convertType, ConvertMember[] convertMemberArray, ConverterBuilderAssistant assistant, Boolean littleEndian)
            : base(convertType, convertMemberArray, assistant, littleEndian)
        {
        }

        protected override LambdaExpression CreateExpression()
        {
            var value = Expression.Parameter(ConvertType);

            var allMemberGetBytes = new List<Expression>();
            var memberMap = new Dictionary<ConvertMember, ParameterExpression>();

            var convertMemberMap = ConvertMemberArray.ToDictionary(member => member.Name);
            var record = new HashSet<ConvertMember>();
            void memberGetBytes(ConvertMember member)
            {
                record.Add(member);
                Expression getBytes;
                if (member.IsPrimitive)
                {
                    if (member.HasGetBytesDependency)
                    {
                        var dependency = convertMemberMap[member.GetBytesDependency];
                        memberGetBytes(dependency);
                        getBytes = member.CreateGetBytes(value, LittleEndian, memberMap[dependency]);
                    }
                    else
                    {
                        getBytes = member.CreateGetBytes(value, LittleEndian);
                    }
                }
                else
                {
                    var builder = Assistant.GetBuilder(member.OrginType);
                    var getBytesMethod = LittleEndian ?
                        builder.LittleEndianGetBytes.GetOrCreateExpression()
                        : builder.BigEndianGetBytes.GetOrCreateExpression();
                    if (member.HasGetBytesDependency)
                    {
                        var dependency = convertMemberMap[member.GetBytesDependency];
                        memberGetBytes(dependency);
                        getBytes = member.CreateGetBytes(value, getBytesMethod, memberMap[dependency]);
                    }
                    else
                    {
                        getBytes = member.CreateGetBytes(value, getBytesMethod);
                    }
                }

                var memberVariable = Expression.Variable(typeof(Byte[]));
                memberMap.Add(member, memberVariable);

                allMemberGetBytes.Add(Expression.Assign(memberVariable, getBytes));
            }
            foreach (var member in ConvertMemberArray)
            {
                if (record.Contains(member))
                {
                    continue;
                }
                else
                {
                    memberGetBytes(member);
                }
            }

            var length = memberMap.Values.Aggregate<ParameterExpression, Expression>(Expression.Constant(0), (sum, member) => Expression.Add(sum, member));

            var list = Expression.Variable(typeof(List<Byte>), "list");
            var addRange = new List<Expression>
            {
                Expression.Assign(
                    list,
                    Expression.New(
                        typeof(List<Byte>).GetConstructor(new[] { typeof(Int32) }),
                        new[] { length }))
            };

            addRange.AddRange(ConvertMemberArray.Select(
                member =>
                Expression.Call(
                    list,
                    typeof(List<Byte>).GetMethod(nameof(List<Byte>.AddRange)),
                    memberMap[member])));

            return Expression.Lambda(
                typeof(GetBytesDelegate<>).MakeGenericType(ConvertType),
                Expression.Block(
                    memberMap.Values.Concat(new[] { list }),
                    allMemberGetBytes.Concat(addRange).Concat(new[]
                    {
                        Expression.Call(
                            typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray)).MakeGenericMethod(typeof(Byte)),
                            list)
                    })),
                value);
        }
    }
}