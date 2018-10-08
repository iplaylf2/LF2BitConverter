using LF2BitConverter.ExpressionGenerator;
using System;
using System.Linq.Expressions;

namespace LF2BitConverter.ConvertMemberAttributeNS
{
    public class ConvertMemberAttribute : Attribute
    {
        public virtual Type OnGetConvertType(ConvertMember member, Type lastResult)
        {
            return lastResult;
        }

        public virtual Boolean OnGetLittleEndian(ConvertMember member, Boolean lastResult)
        {
            return lastResult;
        }

        public virtual Expression OnCreateGetBytes(ConvertMember member, Expression value, Boolean littleEndian, GeneratorContext context, Expression lastResult)
        {
            return lastResult;
        }

        public virtual void AfterCreateGetBytes(ConvertMember member, GeneratorContext context)
        {
        }

        public virtual Func<Expression, LoopExpression> OnGetLoopController(ConvertMember member, ParameterExpression bytes, ParameterExpression startIndex, Boolean littleEndian, GeneratorContext context, Func<Expression, LoopExpression> result)
        {
            return result;
        }

        public virtual Expression OnCreateToObject(ConvertMember member, ParameterExpression bytes, ParameterExpression startIndex, Boolean littleEndian, GeneratorContext context, Expression result)
        {
            return result;
        }

        public virtual void AfterToObject(ConvertMember member, GeneratorContext context)
        {
        }
    }
}
