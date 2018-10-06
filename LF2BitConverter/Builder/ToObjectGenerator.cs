using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Text;
using LF2BitConverter.ConvertMemberNS;

namespace LF2BitConverter.Builder
{
    class ToObjectGenerator : ExpressionGenerator
    {
        public ToObjectGenerator(Type convertType, ConvertMember[] convertMemberArray, ConverterBuilderAssistant assistant, Boolean littleEndian)
           : base(convertType, convertMemberArray, assistant, littleEndian)
        {
        }

        protected override LambdaExpression CreateExpression()
        {
            var bytes = Expression.Parameter(typeof(Byte[]));
            var startIndex = Expression.Parameter(typeof(Int32).MakeByRefType());

            var convertMemberMap = ConvertMemberArray.ToDictionary(member => member.Name);

            var value = Expression.Variable(ConvertType);
            var orderToObject = new List<Expression>
            {
                Expression.Assign(
                    value,
                    Expression.New(ConvertType))
            };
            orderToObject.AddRange(ConvertMemberArray.Select(member =>
            {
                Expression toObject;
                if (member.IsPrimitive)
                {
                    if (member.HasToObjectDependency)
                    {
                        var dependency = convertMemberMap[member.ToObjectDependency];
                        var dependencyMember = Expression.PropertyOrField(value, dependency.Name);
                        toObject = member.CreateToObject(bytes, startIndex, LittleEndian, dependencyMember);
                    }
                    else
                    {
                        toObject = member.CreateToObject(bytes, startIndex, LittleEndian);
                    }
                }
                else
                {
                    var builder = Assistant.GetBuilder(member.OrginType);
                    var toObjectMethod = LittleEndian ?
                        builder.LittleEndianToObject.GetOrCreateExpression()
                        : builder.BigEndianToObject.GetOrCreateExpression();

                    if (member.HasToObjectDependency)
                    {
                        var dependency = convertMemberMap[member.ToObjectDependency];
                        var dependencyMember = Expression.PropertyOrField(value, dependency.Name);
                        toObject = member.CreateToObject(bytes, startIndex, toObjectMethod, dependencyMember);
                    }
                    else
                    {
                        toObject = member.CreateToObject(bytes, startIndex, toObjectMethod);
                    }
                }
                return Expression.Assign(
                    Expression.PropertyOrField(value, member.Name),
                    toObject);
            }));

            return Expression.Lambda(
                typeof(ToObjectDelegate<>).MakeGenericType(ConvertType),
                Expression.Block(orderToObject.Concat(new[]
                {
                    value
                })),
                new[] { bytes, startIndex });
        }
    }
}
