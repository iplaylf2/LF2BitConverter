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

        public Boolean GetLittleEndian(Boolean littleEndian)
        {
            switch (Endian)
            {
                case Endian.Dynamic:
                    return littleEndian;
                case Endian.Little:
                    return true;
                case Endian.Big:
                    return false;
                default:
                    throw new NotImplementedException();
            }
        }

        public Expression CreateGetBytes(Expression value, Boolean littleEndian)
        {
            var convertType = value.Type;
            if (convertType.IsArray)
            {
                var valueVariable = Expression.Variable(value.Type);
                if (convertType == typeof(Byte[]))
                {
                    return Expression.Block(
                        new[] { valueVariable },
                        new Expression[]
                        {
                            Expression.Assign(valueVariable,value),
                            valueVariable
                        });
                }

                var elementType = convertType.GetElementType();
                if (elementType.IsPrimitive)
                {
                    var elementSize = Marshal.SizeOf(elementType);

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
                                         Expression.Constant(elementSize),
                                         Expression.Property(valueVariable,nameof(Array.Length))))),
                             ExpressionExtension.Foreach(valueVariable,
                             item=>Expression.Block(
                                 Expression.Call(
                                     SingleGetBytes(item,littleEndian),
                                     nameof(Array.CopyTo),null,bytesResult,index),
                                 Expression.AddAssign(index,Expression.Constant(elementSize)))),
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
                                SingleGetBytes(item,littleEndian))),
                            Expression.Call(list,nameof(List<Byte>.ToArray),null)
                        });
                }
            }
            else
            {
                return SingleGetBytes(value, littleEndian);
            }
        }

        public Expression CreateToObject(Type convertType, ParameterExpression bytes, ParameterExpression startIndex, Boolean littleEndian, Func<Expression, Expression> loopController)
        {
            if (convertType.IsArray)
            {
                var elementType = convertType.GetElementType();

                var list = Expression.Variable(typeof(List<>).MakeGenericType(elementType));

                return Expression.Block(
                    new[] { list },
                    new Expression[]
                    {
                        Expression.Assign(
                            list,
                            Expression.New(typeof(List<>).MakeGenericType(elementType))),
                        loopController(Expression.Call(list,nameof(List<Object>.Add),null,
                        SingleToObject(elementType,bytes,startIndex,littleEndian))),
                        Expression.Call(list,nameof(List<Object>.ToArray),null)
                    });
            }
            else
            {
                return SingleToObject(convertType, bytes, startIndex, littleEndian);
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

            Endian = ConvertMemberAttributeArray.Aggregate(Endian.Dynamic, (endian, attribute) => attribute.OnGetEndian(endian));
        }

        internal Expression CreateGetBytes(ParameterExpression obj, GeneratorContext context)
        {
            var value = Expression.PropertyOrField(obj, Name);
            var littleEndian = GetLittleEndian(context.LittleEndian);
            var bytes = CreateGetBytes(value, littleEndian);

            return ConvertMemberAttributeArray.Aggregate(bytes, (result, attribute) => attribute.OnCreateGetBytes(obj, context, result));
        }

        internal void AfterAllCreateGetBytes(GeneratorContext context)
        {
            foreach (var attribute in ConvertMemberAttributeArray)
            {
                attribute.AfterAllCreateGetBytes(context);
            }
        }

        internal Expression CreateToObject(ParameterExpression bytes, ParameterExpression startIndex, GeneratorContext context)
        {
            var loopBreak = Expression.Label();
            Func<Expression, Expression> loopController = (Expression block) => Expression.Loop(Expression.Break(loopBreak), loopBreak);
            loopController = ConvertMemberAttributeArray.Aggregate(loopController, (result, member) => member.OnGetLoopController(bytes, startIndex, context, result));
            var littleEndian = GetLittleEndian(context.LittleEndian);
            var obj = CreateToObject(Type, bytes, startIndex, littleEndian, loopController);

            return ConvertMemberAttributeArray.Aggregate(obj, (result, attribute) => attribute.OnCreateToObject(bytes, startIndex, context, result));
        }

        internal void AfterAllCreateToObject(GeneratorContext context)
        {
            foreach (var attribute in ConvertMemberAttributeArray)
            {
                attribute.AfterAllCreateToObject(context);
            }
        }

        private readonly ConverterBuilderAssistant Assistant;
        private readonly ConvertMemberAttribute[] ConvertMemberAttributeArray;
        private readonly Endian Endian;

        private Expression SingleGetBytes(Expression value, Boolean littleEndian)
        {
            var convertType = value.Type;

            var valueVariable = Expression.Variable(convertType);

            if (convertType.IsPrimitive)
            {
                Expression getBytes;
                if (convertType == typeof(Byte))
                {
                    getBytes = Expression.NewArrayInit(convertType, valueVariable);
                }
                else
                {
                    getBytes = Expression.Call(BitConverterSelector.SelectGetBytes(convertType, littleEndian), valueVariable);
                }

                return Expression.Block(
                    new[] { valueVariable },
                    new Expression[]
                    {
                        Expression.Assign(valueVariable, value),
                        getBytes
                    });
            }
            else
            {
                var getBytesLambda = Assistant.GetOrAddBuilder(convertType).GetOrCreateGetBytes(littleEndian);
                return Expression.Block(
                    new[] { valueVariable },
                    new Expression[]
                    {
                        Expression.Assign(valueVariable, value),
                        Expression.Invoke(getBytesLambda,valueVariable)
                    });
            }
        }

        private Expression SingleToObject(Type convertType, ParameterExpression bytes, ParameterExpression startIndex, Boolean littleEndian)
        {
            if (convertType.IsPrimitive)
            {
                var objResult = Expression.Variable(convertType);

                Expression toObject;
                if (convertType == typeof(Byte))
                {
                    toObject = Expression.ArrayAccess(bytes, startIndex);
                }
                else
                {
                    toObject = Expression.Call(BitConverterSelector.SelectToObject(convertType, littleEndian), bytes, startIndex);
                }

                return Expression.Block(
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
                var toObjectLambda = Assistant.GetOrAddBuilder(convertType).GetOrCreateToObject(littleEndian);
                return Expression.Invoke(toObjectLambda, bytes, startIndex);
            }
        }
    }
}
