using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using LF2BitConverter.ExpressionGenerator;

namespace LF2BitConverter.ConvertMemberAttributeNS
{
    public class ConvertStringAttribute : ConvertMemberAttribute
    {
        public ConvertStringAttribute(String name, String lengthFrom)
        {
            StringEncoding = Expression.Constant(Encoding.GetEncoding(name));
            LengthFrom = lengthFrom;
        }

        public override Expression OnCreateGetBytes(ParameterExpression obj, GeneratorContext context, Expression lastResult)
        {
            return Expression.Call(
                StringEncoding,
                nameof(Encoding.GetBytes),
                null,
                Expression.PropertyOrField(obj, Member.Name));
        }

        public override void AfterCreateGetBytes(GeneratorContext context)
        {
            var littleEndian = Member.GetLittleEndian(context.LittleEndian);

            var myIndex = context.MemberResult.FindIndex(item => item.Item1 == Member.Name);

            var mineVariable = context.VariableMap[Member.Name];
            context.Pretreatment.Add(
                Expression.Assign(
                    mineVariable,
                    context.MemberResult[myIndex].Item2));
            context.MemberResult[myIndex] = (Member.Name, mineVariable);

            var length = Expression.Property(
                 mineVariable,
                 nameof(Array.Length));


            var lengthIndex = context.MemberResult.FindIndex(item => item.Item1 == LengthFrom);
            var lengthMember = context.MemberArray.Single(member => member.Name == LengthFrom);
            context.MemberResult[lengthIndex] = (
                LengthFrom,
                Member.CreateGetBytes(lengthMember.ConvertType, length, littleEndian)
                );

        }

        public override Expression OnCreateToObject(ParameterExpression bytes, ParameterExpression startIndex, GeneratorContext context, Expression lastResult)
        {
            var length = context.VariableMap[LengthFrom];
            var result = Expression.Variable(typeof(String));
            return Expression.Block(
                new[] { result },
                new Expression[]
                {
                    Expression.Assign(
                        result,
                        Expression.Call(
                            StringEncoding,
                            nameof(Encoding.GetString),
                            null,
                            bytes,
                            startIndex,
                            length)),
                    Expression.AddAssign(
                        startIndex,
                        length),
                    result
                });
        }

        private readonly ConstantExpression StringEncoding;
        private readonly String LengthFrom;
    }
}
