using System;
using System.Reflection;
using System.Linq;
using LF2BitConverter.ConvertMemberAttributeNS;
using System.Linq.Expressions;
using LF2BitConverter.ExpressionGenerator;
using System.Collections.Generic;

namespace LF2BitConverter.Builder
{
    class ConverterBuilder
    {
        public ConverterBuilder(Type type, ConverterBuilderAssistant assistant)
        {
            ConvertType = type;
            Assistant = assistant;
            ConvertMemberArray = ConvertMemberAnalyse();
        }

        public ConverterUint Build(Boolean littleEndian)
        {
            return new ConverterUint
            {
                GetBytes = GetOrCreateGetBytes(littleEndian).Compile(),
                //ToObject = GetOrCreateToObject(littleEndian).Compile()
            };
        }

        public LambdaExpression GetOrCreateGetBytes(Boolean littleEndian)
        {
            if (littleEndian)
            {
                if (LittleEndianGetBytes == null)
                {
                    LittleEndianGetBytes = CreateGetBytes(littleEndian);
                }
                return LittleEndianGetBytes;
            }
            else
            {
                if (BigEndianGetBytes == null)
                {
                    BigEndianGetBytes = CreateGetBytes(littleEndian);
                }
                return BigEndianGetBytes;
            }
        }

        public LambdaExpression GetOrCreateToObject(Boolean littleEndian)
        {
            if (littleEndian)
            {
                if (LittleEndianToObject == null)
                {
                    LittleEndianToObject = CreateToObject(littleEndian);
                }
                return LittleEndianToObject;
            }
            else
            {
                if (BigEndianToObject == null)
                {
                    BigEndianToObject = CreateToObject(littleEndian);
                }
                return BigEndianToObject;
            }
        }

        private readonly Type ConvertType;
        private readonly ConverterBuilderAssistant Assistant;
        private readonly ConvertMember[] ConvertMemberArray;
        private LambdaExpression LittleEndianGetBytes;
        private LambdaExpression BigEndianGetBytes;
        private LambdaExpression LittleEndianToObject;
        private LambdaExpression BigEndianToObject;

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
                    }).ToArray();
            foreach (var member in validMembers)
            {
                switch (member)
                {
                    case PropertyInfo property:
                        Assistant.CheckCycle(property.PropertyType);
                        break;
                    case FieldInfo field:
                        Assistant.CheckCycle(field.FieldType);
                        break;
                    default:
                        break;

                }
            }
            return validMembers.Select(member => new ConvertMember(member, Assistant)).ToArray();
        }

        private LambdaExpression CreateGetBytes(Boolean littleEndian)
        {
            var obj = Expression.Parameter(ConvertType);

            var context = new GeneratorContext
            {
                Assignment = new List<(string, BinaryExpression)>(),
                Pretreatment = new List<Expression>(),
                VariableMap = new Dictionary<string, ParameterExpression>()
            };

            foreach (var member in ConvertMemberArray)
            {
                var memberVariable = Expression.Variable(typeof(Byte[]));
                context.VariableMap.Add(member.Name, memberVariable);

                var memberValue = Expression.PropertyOrField(obj, member.Name);
                var bytes = member.CreateGetBytes(memberValue, littleEndian, context);
                var assign = Expression.Assign(memberVariable, bytes);
                context.Assignment.Add((member.Name, assign));
            }

            foreach (var member in ConvertMemberArray)
            {
                member.AfterCreateGetBytes(context);
            }

            var bytesResult = Expression.Variable(typeof(Byte[]));
            var index = Expression.Variable(typeof(Int32));

            var orderMembers = ConvertMemberArray.Select(member => context.VariableMap[member.Name]).ToArray();

            var length = orderMembers.Aggregate((Expression)Expression.Constant(0),
                (sum, member) =>
                Expression.Add(
                    sum,
                    Expression.Property(member, nameof(Array.Length))));

            var merge = Expression.Block(
                new[] { bytesResult, index },
                new Expression[]
                {
                    Expression.Assign(
                        bytesResult,
                        Expression.NewArrayBounds(typeof(Byte),length)),
                    Expression.Assign(
                        index,
                        Expression.Constant(0))
                }.Concat(orderMembers.Select(member =>
                Expression.Block(
                    Expression.Call(member, nameof(Array.CopyTo), null, bytesResult, index),
                    Expression.AddAssign(
                        index,
                        Expression.Property(member, nameof(Array.Length)))))
                 ).Concat(new[]
                 {
                     bytesResult
                 }));

            return Expression.Lambda(
                typeof(GetBytesDelegate<>).MakeGenericType(ConvertType),
                Expression.Block(
                    context.VariableMap.Values,
                    context.Pretreatment
                    .Concat(context.Assignment.Select(assign => assign.Item2))
                    .Concat(new[] { merge })),
                obj);
        }

        private LambdaExpression CreateToObject(Boolean littleEndian)
        {
            var bytes = Expression.Parameter(typeof(Byte[]));
            var startIndex = Expression.Parameter(typeof(Int32).MakeByRefType());

            var context = new GeneratorContext
            {
                Assignment = new List<(string, BinaryExpression)>(),
                Pretreatment = new List<Expression>(),
                VariableMap = new Dictionary<string, ParameterExpression>()
            };

            foreach (var member in ConvertMemberArray)
            {
                var memberVariable = Expression.Variable(member.Type);
                context.VariableMap.Add(member.Name, memberVariable);

                var obj = member.CreateToObject(bytes, startIndex, littleEndian, context);
                var assign = Expression.Assign(memberVariable, obj);
                context.Assignment.Add((member.Name, assign));
            }

            foreach (var member in ConvertMemberArray)
            {
                member.AfterToObject(context);
            }

            var objectResult = Expression.Variable(ConvertType);

            var merge = Expression.Block(
                new[] { objectResult },
                new Expression[]
                {
                    Expression.Assign(objectResult,Expression.New(ConvertType))
                }.Concat(
                ConvertMemberArray.Select(member =>
                Expression.Assign(
                    Expression.PropertyOrField(objectResult, member.Name),
                    context.VariableMap[member.Name])
                )
                ).Concat(new[]
                {
                    objectResult
                }));

            return Expression.Lambda(
                typeof(ToObjectDelegate<>).MakeGenericType(ConvertType),
                Expression.Block(
                    context.VariableMap.Values,
                    context.Pretreatment
                    .Concat(context.Assignment.Select(assign => assign.Item2))
                    .Concat(new[] { merge })),
               bytes, startIndex);
        }
    }
}
