using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using LF2BitConverter.ExpressionGenerator;
using LF2BitConverter.Extenison;

namespace LF2BitConverter.ConvertMemberAttributeNS
{
    public class ConvertArrayAttribute : ConvertMemberAttribute
    {
        public String LengthFrom { get; set; }
        public Int32 Length { get; set; }

        public ConvertArrayAttribute(CountBy countBy)
        {
            CountBy = countBy;
        }

        public override Expression OnCreateGetBytes(ParameterExpression obj, GeneratorContext context, Expression lastResult)
        {
            if (!String.IsNullOrEmpty(LengthFrom) && CountBy == CountBy.Item)
            {
                var count = Expression.Variable(typeof(Int32));
                context.VariableMap.Add(ItemCountInGetBytes, count);

                context.Pretreatment.Add(
                    Expression.Assign(
                        count,
                        Expression.Property(
                            Expression.PropertyOrField(obj, Member.Name),
                            nameof(Array.Length))));
            }
            return lastResult;
        }

        public override void AfterAllCreateGetBytes(GeneratorContext context)
        {
            if (!String.IsNullOrEmpty(LengthFrom))
            {
                Expression length;
                if (CountBy == CountBy.Byte)
                {
                    var myIndex = context.MemberResult.FindIndex(item => item.Item1 == Member.Name);

                    var mineVariable = context.VariableMap[Member.Name];
                    context.Pretreatment.Add(
                        Expression.Assign(
                            mineVariable,
                            context.MemberResult[myIndex].Item2));
                    context.MemberResult[myIndex] = (Member.Name, mineVariable);

                    length = Expression.Property(
                       mineVariable,
                       nameof(Array.Length));
                }
                else
                {
                    length = context.VariableMap[ItemCountInGetBytes];
                }

                var lengthIndex = context.MemberResult.FindIndex(item => item.Item1 == LengthFrom);
                var lengthMember = context.MemberArray.Single(member => member.Name == LengthFrom);

                var littleEndian = context.MemberArray.Single(member => member.Name == LengthFrom)
                    .GetLittleEndian(context.LittleEndian);

                context.MemberResult[lengthIndex] = (
                    LengthFrom,
                    Member.CreateGetBytes(lengthMember.ConvertType, length, littleEndian)
                    );
            }
        }

        public override Func<Expression, Expression> OnGetLoopController(ParameterExpression bytes, ParameterExpression startIndex, GeneratorContext context, Func<Expression, Expression> lastResult)
        {
            var loopBreak = Expression.Label();
            if (CountBy == CountBy.Byte)
            {
                var limitVariable = Expression.Variable(typeof(Int32));
                Expression limit;
                if (String.IsNullOrEmpty(LengthFrom))
                {
                    limit = Expression.Add(
                        startIndex,
                        Expression.Constant(Length));
                }
                else
                {
                    limit = Expression.Add(
                        startIndex,
                        Expression.Convert(
                            context.VariableMap[LengthFrom],
                            typeof(Int32)));
                }

                return body =>
                    Expression.Block(
                        new[] { limitVariable },
                        new Expression[]
                        {
                            Expression.Assign(
                                limitVariable,
                                limit),
                            Expression.Loop(
                                Expression.IfThenElse(
                                    Expression.Equal(startIndex,limitVariable),
                                    Expression.Break(loopBreak),
                                    body),
                                loopBreak)
                        });
            }
            else
            {
                var limitVariable = Expression.Variable(typeof(Int32));
                var countVariable = Expression.Variable(typeof(Int32));
                Expression limit;
                if (String.IsNullOrEmpty(LengthFrom))
                {
                    limit = Expression.Constant(Length);
                }
                else
                {
                    limit = Expression.Convert(context.VariableMap[LengthFrom], typeof(Int32));
                }

                return body =>
                Expression.Block(
                    new[] { limitVariable, countVariable },
                    new Expression[]
                    {
                        Expression.Assign(
                            limitVariable,
                            limit),
                        Expression.Assign(
                            countVariable,
                            Expression.Constant(0)),
                            Expression.Loop(
                                Expression.IfThenElse(
                                    Expression.Equal(countVariable,limitVariable),
                                    Expression.Break(loopBreak),
                                    Expression.Block(
                                        body,
                                        Expression.PostIncrementAssign(countVariable))),
                                loopBreak)
                    });
            }
        }

        private readonly CountBy CountBy;
        private readonly String ItemCountInGetBytes = new Guid().ToString();

    }

    public enum CountBy
    {
        Byte, Item
    }
}