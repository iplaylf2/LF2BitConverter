using LF2BitConverter.ExpressionGenerator;
using System;
using System.Linq.Expressions;

namespace LF2BitConverter.ConvertMemberAttributeNS
{
    public class ConvertMemberAttribute : Attribute
    {
        public ConvertMemberAttribute()
        {
        }

        public virtual Type OnGetConvertType(Type lastResult)
        {
            return lastResult;
        }

        public virtual Boolean OnGetLittleEndian(Boolean lastResult)
        {
            return lastResult;
        }

        public virtual Expression OnCreateGetBytes(Expression value, Boolean littleEndian, GeneratorContext context, Expression lastResult)
        {
            return lastResult;
        }

        public virtual void AfterCreateGetBytes(GeneratorContext context)
        {
        }

        public virtual Func<Expression, LoopExpression> OnGetLoopController(ParameterExpression bytes, ParameterExpression startIndex, Boolean littleEndian, GeneratorContext context, Func<Expression, LoopExpression> lastResult)
        {
            return lastResult;
        }

        public virtual Expression OnCreateToObject(ParameterExpression bytes, ParameterExpression startIndex, Boolean littleEndian, GeneratorContext context, Expression lastResult)
        {
            return lastResult;
        }

        public virtual void AfterToObject(GeneratorContext context)
        {
        }

        internal void SetConvertMember(ConvertMember member)
        {
            Member = member;
        }

        protected ConvertMember Member;
    }
}
