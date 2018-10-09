using System;
using System.Reflection;
using System.Linq;
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

        public Delegate BuildGetBytes(Boolean littleEndian)
        {
            return GetOrCreateGetBytes(littleEndian).Compile();
        }

        public Delegate BuildToObject(Boolean littleEndian)
        {
            return GetOrCreateToObject(littleEndian).Compile();
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
                LittleEndian = littleEndian,
                MemberResult = new List<(string, Expression)>(),
                Pretreatment = new List<Expression>(),
                VariableMap = ConvertMemberArray.ToDictionary(member => member.Name, _ => Expression.Variable(typeof(Byte[]))),
                MemberArray = ConvertMemberArray
            };

            foreach (var member in ConvertMemberArray)
            {
                var bytes = member.CreateGetBytes(obj, context);
                context.MemberResult.Add((member.Name, bytes));
            }

            foreach (var member in ConvertMemberArray)
            {
                member.AfterCreateGetBytes(context);
            }

            var bytesResult = Expression.Variable(typeof(Byte[]));
            var index = Expression.Variable(typeof(Int32));

            var assignmentMembers = context.MemberResult.Select(item => item.Item1).ToArray();

            var length = assignmentMembers
                .Aggregate((Expression)Expression.Constant(0),
                (sum, member) =>
                Expression.Add(
                    sum,
                    Expression.Property(context.VariableMap[member], nameof(Array.Length))));

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
                }.Concat(assignmentMembers.Select(
                    member =>
                    Expression.Block(
                        Expression.Call(context.VariableMap[member], nameof(Array.CopyTo), null, bytesResult, index),
                        Expression.AddAssign(
                            index,
                            Expression.Property(context.VariableMap[member], nameof(Array.Length)))))
                 ).Concat(new[]
                 {
                     bytesResult
                 }));

            return Expression.Lambda(
                typeof(GetBytesDelegate<>).MakeGenericType(ConvertType),
                Expression.Block(
                    context.VariableMap.Values,
                    context.Pretreatment
                    .Concat(context.MemberResult.Select(result => Expression.Assign(context.VariableMap[result.Item1], result.Item2)))
                    .Concat(new[] { merge })),
                obj);
        }

        private LambdaExpression CreateToObject(Boolean littleEndian)
        {
            var bytes = Expression.Parameter(typeof(Byte[]));
            var startIndex = Expression.Parameter(typeof(Int32).MakeByRefType());

            var context = new GeneratorContext
            {
                LittleEndian = littleEndian,
                MemberResult = new List<(string, Expression)>(),
                Pretreatment = new List<Expression>(),
                VariableMap = ConvertMemberArray.ToDictionary(member => member.Name, member => Expression.Variable(member.Type)),
                MemberArray = ConvertMemberArray
            };

            foreach (var member in ConvertMemberArray)
            {
                var obj = member.CreateToObject(bytes, startIndex, context);
                context.MemberResult.Add((member.Name, obj));
            }

            foreach (var member in ConvertMemberArray)
            {
                member.AfterToObject(context);
            }

            var objectResult = Expression.Variable(ConvertType);

            var assignmentMembers = context.MemberResult.Select(item => item.Item1).ToArray();

            var merge = Expression.Block(
                new[] { objectResult },
                new Expression[]
                {
                    Expression.Assign(objectResult,Expression.New(ConvertType))
                }.Concat(
                assignmentMembers.Select(
                    member =>
                    Expression.Assign(
                        Expression.PropertyOrField(objectResult, member),
                        context.VariableMap[member]))
                ).Concat(new[]
                {
                    objectResult
                }));

            return Expression.Lambda(
                typeof(ToObjectDelegate<>).MakeGenericType(ConvertType),
                Expression.Block(
                    context.VariableMap.Values,
                    context.Pretreatment
                    .Concat(context.MemberResult.Select(result => Expression.Assign(context.VariableMap[result.Item1], result.Item2)))
                    .Concat(new[] { merge })),
               bytes, startIndex);
        }
    }
}
