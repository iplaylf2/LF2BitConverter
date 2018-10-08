using System;
using LF2BitConverter.Builder;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using LF2BitConverter.ExpressionGenerator;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using LF2BitConverter.Extenison;

namespace LF2BitConverter.ConvertMemberAttributeNS
{
    public sealed class ConvertMember
    {
        public Type Type { get; }
        public String Name { get; }

        public Expression CreateGetBytes(Type convertType, Expression value, Boolean littleEndian)
        {
            if (value.Type.IsArray)
            {
                var valueVariable = Expression.Variable(value.Type);
                if (convertType.IsPrimitive)
                {
                    var bytesResult = Expression.Variable(typeof(Byte[]));
                    var index = Expression.Variable(typeof(Int32));

                    return Expression.Block(
                         new[] { valueVariable, bytesResult, index },
                         new Expression[]
                         {
                             Expression.Assign(valueVariable,value),
                             Expression.Assign(
                                 index,
                                 Expression.Constant(0)),
                             Expression.Assign(
                                 bytesResult,
                                 Expression.NewArrayBounds(
                                     typeof(Byte),
                                     Expression.Multiply(
                                         Expression.Constant(Marshal.SizeOf(convertType)),
                                         Expression.PropertyOrField(valueVariable,nameof(Array.Length))))),
                             ExpressionExtension.Foreach(valueVariable,
                             item=>Expression.Block(
                                 Expression.Call(
                                     SingleGetBytes(convertType,item,littleEndian),
                                     nameof(Array.CopyTo),null,bytesResult,index),
                                 Expression.AddAssign(index,Expression.Constant(Marshal.SizeOf(convertType))))),
                             bytesResult
                         });
                }
                else
                {
                    var list = Expression.Variable(typeof(List<Byte>));

                    return Expression.Block(
                        new[] { valueVariable, list },
                        new Expression[]
                        {
                            Expression.Assign(valueVariable,value),
                            Expression.Assign(
                                list,
                                Expression.New(typeof(List<Byte>))),
                            ExpressionExtension.Foreach(valueVariable,
                            item=>Expression.Call(
                                list,nameof(List<Byte>.AddRange),null,
                                SingleGetBytes(convertType,item,littleEndian))),
                            Expression.Call(list,nameof(List<Byte>.ToArray),null)
                        });
                }
            }
            else
            {
                return SingleGetBytes(convertType, value, littleEndian);
            }
        }

        public Expression CreateToObject(Type convertType, ParameterExpression bytes, ParameterExpression startIndex, Boolean littleEndian)
        {
            throw new NotImplementedException();
        }

        internal ConvertMember(MemberInfo member, ConverterBuilderAssistant assistant)
        {
            switch (member)
            {
                case PropertyInfo property:
                    Type = property.PropertyType;
                    break;
                case FieldInfo field:
                    Type = field.FieldType;
                    break;
                default:
                    throw new NotImplementedException();
            }
            Name = member.Name;
            Assistant = assistant;
            ConvertMemberAttributeArray = member.GetCustomAttributes<ConvertMemberAttribute>().DefaultIfEmpty(new ConvertMemberAttribute()).ToArray();

            if (Type.IsArray)
            {
                ConvertType = Type.GetElementType();
            }
            else
            {
                ConvertType = Type;
            }
            ConvertType = ConvertType.IsEnum ? typeof(Int32) : ConvertType;
            ConvertType = ConvertMemberAttributeArray.Aggregate(ConvertType, (type, attribute) => attribute.OnGetConvertType(this, type));
        }

        internal Expression CreateGetBytes(Expression value, Boolean littleEndian, GeneratorContext context)
        {
            littleEndian = ConvertMemberAttributeArray.Aggregate(littleEndian, (result, attribute) => attribute.OnGetLittleEndian(this, result));

            var bytes = CreateGetBytes(ConvertType, value, littleEndian);

            return ConvertMemberAttributeArray.Aggregate(bytes, (result, attribute) => attribute.OnCreateGetBytes(this, ConvertType, value, littleEndian, context, result));
        }

        internal void AfterCreateGetBytes(GeneratorContext context)
        {
            foreach (var attribute in ConvertMemberAttributeArray)
            {
                attribute.AfterCreateGetBytes(this, context);
            }
        }

        internal Expression CreateToObject(ParameterExpression values, ParameterExpression startIndex, Boolean littleEndian, GeneratorContext context)
        {
            throw new NotImplementedException();
        }

        internal void AfterToObject(GeneratorContext context)
        {
            foreach (var attribute in ConvertMemberAttributeArray)
            {
                attribute.AfterToObject(this, context);
            }
        }

        private readonly ConverterBuilderAssistant Assistant;
        private readonly ConvertMemberAttribute[] ConvertMemberAttributeArray;
        private readonly Type ConvertType;

        private Expression ArrayGetBytes(Type convertType, Expression value, Boolean littleEndian)
        {
            throw new NotImplementedException();
        }

        private Expression SingleGetBytes(Type convertType, Expression value, Boolean littleEndian)
        {
            var valueVariable = Expression.Variable(convertType);
            Expression confirmValue;
            if (convertType == value.Type)
            {
                confirmValue = Expression.Assign(valueVariable, value);
            }
            else
            {
                confirmValue = Expression.Assign(
                    valueVariable,
                    Expression.Convert(value, convertType));
            }
            if (convertType.IsPrimitive)
            {
                return Expression.Block(
                    new[] { valueVariable },
                    new Expression[]
                    {
                        confirmValue,
                        Expression.Call(BitConverterSelector.SelectGetBytes(convertType,littleEndian),valueVariable)
                    });
            }
            else
            {
                var getBytes = Assistant.GetBuilder(convertType).GetOrCreateGetBytes(littleEndian);
                return Expression.Block(
                    new[] { valueVariable },
                    new Expression[]
                    {
                        confirmValue,
                        Expression.Invoke(getBytes,valueVariable)
                    });
            }
        }
    }
}
