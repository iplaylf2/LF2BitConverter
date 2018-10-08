using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using LF2BitConverter.ExpressionGenerator;
using LF2BitConverter.Extenison;

namespace LF2BitConverter.ConvertMemberAttributeNS
{
    public class ConvertArrayAttribute : ConvertMemberAttribute
    {
        public CountBy CountBy { get; }
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

        public override void AfterCreateGetBytes(GeneratorContext context)
        {
            var littleEndian = Member.GetLittleEndian(context.LittleEndian);
            if (!String.IsNullOrEmpty(LengthFrom))
            {
                if (CountBy == CountBy.Byte)
                {
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
                    context.MemberResult[lengthIndex] = (LengthFrom,
                        Expression.Call(
                            BitConverterSelector.SelectGetBytes(typeof(Int32), littleEndian),
                            length));
                }
                else
                {
                    var length = context.VariableMap[ItemCountInGetBytes];
                    var index = context.MemberResult.FindIndex(item => item.Item1 == LengthFrom);
                    context.MemberResult[index] = (LengthFrom,
                        Expression.Call(
                            BitConverterSelector.SelectGetBytes(typeof(Int32), littleEndian),
                            length));
                }
            }
        }

        private String ItemCountInGetBytes = new Guid().ToString();

    }

    public enum CountBy
    {
        Byte, Item
    }
}
