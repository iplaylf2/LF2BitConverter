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

        public virtual Endian OnGetEndian(Endian lastResult)
        {
            return lastResult;
        }

        public virtual Expression OnCreateGetBytes(ParameterExpression obj, GeneratorContext context, Expression lastResult)
        {
            return lastResult;
        }

        public virtual void AfterCreateGetBytes(GeneratorContext context)
        {
        }

        public virtual Func<Expression, LoopExpression> OnGetLoopController(ParameterExpression bytes, ParameterExpression startIndex, GeneratorContext context, Func<Expression, LoopExpression> lastResult)
        {
            return lastResult;
        }

        public virtual Expression OnCreateToObject(ParameterExpression bytes, ParameterExpression startIndex, GeneratorContext context, Expression lastResult)
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
