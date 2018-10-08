using System;
using LF2BitConverter.Builder;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using LF2BitConverter.Extenison;
using LF2BitConverter.ConvertMemberAttributeNS;

namespace LF2BitConverter.ExpressionGenerator
{
    public sealed class ConvertMember
    {
        public String Name { get; }
        public Type Type { get; }
        public Type ConvertType { get; }

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

        public Expression CreateToObject(Type convertType, Type originType, ParameterExpression bytes, ParameterExpression startIndex, Boolean littleEndian, Func<Expression, LoopExpression> loopController)
        {
            if (originType.IsArray)
            {
                var elementType = originType.GetElementType();

                var list = Expression.Variable(typeof(List<>).MakeGenericType(elementType));

                return Expression.Block(
                    new[] { list },
                    new Expression[]
                    {
                        Expression.Assign(
                            list,
                            Expression.New(typeof(List<>).MakeGenericType(elementType))),
                        loopController(Expression.Call(list,nameof(List<Object>.Add),null,
                        SingleToObject(convertType,elementType,bytes,startIndex,littleEndian))),
                        Expression.Call(list,nameof(List<Object>.ToArray),null)
                    });
            }
            else
            {
                return SingleToObject(convertType, originType, bytes, startIndex, littleEndian);
            }
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
            foreach (var attribute in ConvertMemberAttributeArray)
            {
                attribute.SetConvertMember(this);
            }

            if (Type.IsArray)
            {
                ConvertType = Type.GetElementType();
            }
            else
            {
                ConvertType = Type;
            }
            ConvertType = ConvertType.IsEnum ? typeof(Int32) : ConvertType;
            ConvertType = ConvertMemberAttributeArray.Aggregate(ConvertType, (type, attribute) => attribute.OnGetConvertType(type));
        }

        internal Expression CreateGetBytes(Expression value, Boolean littleEndian, GeneratorContext context)
        {
            littleEndian = ConvertMemberAttributeArray.Aggregate(littleEndian, (result, attribute) => attribute.OnGetLittleEndian(result));

            var bytes = CreateGetBytes(ConvertType, value, littleEndian);

            return ConvertMemberAttributeArray.Aggregate(bytes, (result, attribute) => attribute.OnCreateGetBytes(value, littleEndian, context, result));
        }

        internal void AfterCreateGetBytes(GeneratorContext context)
        {
            foreach (var attribute in ConvertMemberAttributeArray)
            {
                attribute.AfterCreateGetBytes(context);
            }
        }

        internal Expression CreateToObject(ParameterExpression bytes, ParameterExpression startIndex, Boolean littleEndian, GeneratorContext context)
        {
            littleEndian = ConvertMemberAttributeArray.Aggregate(littleEndian, (result, attribute) => attribute.OnGetLittleEndian(result));

            var loopBreak = Expression.Label();
            Func<Expression, LoopExpression> loopController = (Expression block) => Expression.Loop(Expression.Break(loopBreak), loopBreak);
            loopController = ConvertMemberAttributeArray.Aggregate(loopController, (result, member) => member.OnGetLoopController(bytes, startIndex, littleEndian, context, result));
            var obj = CreateToObject(ConvertType, Type, bytes, startIndex, littleEndian, loopController);

            return ConvertMemberAttributeArray.Aggregate(obj, (result, attribute) => attribute.OnCreateToObject(bytes, startIndex, littleEndian, context, result));
        }

        internal void AfterToObject(GeneratorContext context)
        {
            foreach (var attribute in ConvertMemberAttributeArray)
            {
                attribute.AfterToObject(context);
            }
        }

        private readonly ConverterBuilderAssistant Assistant;
        private readonly ConvertMemberAttribute[] ConvertMemberAttributeArray;

        private Expression SingleGetBytes(Type convertType, Expression value, Boolean littleEndian)
        {
            var valueVariable = Expression.Variable(convertType);
            BinaryExpression confirmValue;
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
                Expression getBytes;
                if (ConvertType == typeof(Byte))
                {
                    getBytes = Expression.NewArrayInit(ConvertType, valueVariable);
                }
                else
                {
                    getBytes = Expression.Call(BitConverterSelector.SelectGetBytes(convertType, littleEndian), valueVariable);
                }

                return Expression.Block(
                    new[] { valueVariable },
                    new Expression[]
                    {
                        confirmValue,
                        getBytes
                    });
            }
            else
            {
                var getBytesMethod = Assistant.GetBuilder(convertType).GetOrCreateGetBytes(littleEndian);
                return Expression.Block(
                    new[] { valueVariable },
                    new Expression[]
                    {
                        confirmValue,
                        Expression.Invoke(getBytesMethod,valueVariable)
                    });
            }
        }

        private Expression SingleToObject(Type convertType, Type originType, ParameterExpression bytes, ParameterExpression startIndex, Boolean littleEndian)
        {
            Expression obj;
            if (convertType.IsPrimitive)
            {
                var objResult = Expression.Variable(convertType);

                Expression toObject;
                if (ConvertType == typeof(Byte))
                {
                    toObject = Expression.ArrayAccess(bytes, startIndex);
                }
                else
                {
                    toObject = Expression.Call(BitConverterSelector.SelectToObject(convertType, littleEndian), bytes, startIndex);
                }

                obj = Expression.Block(
                    new[] { objResult },
                    new Expression[]
                    {
                        Expression.Assign(objResult,toObject),
                        Expression.AddAssign(
                            startIndex,
                            Expression.Constant(Marshal.SizeOf(convertType))),
                        objResult
                   });
            }
            else
            {
                var toObjectMethod = Assistant.GetBuilder(convertType).GetOrCreateToObject(littleEndian);
                obj = Expression.Invoke(toObjectMethod, bytes, startIndex);
            }
            if (convertType == originType)
            {
                return obj;
            }
            else
            {
                return Expression.Convert(obj, originType);
            }
        }
    }
}
