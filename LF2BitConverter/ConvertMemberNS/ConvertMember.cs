using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace LF2BitConverter.ConvertMemberNS
{
    abstract class ConvertMember
    {
        public Type OrginType { get; }
        public Boolean IsPrimitive { get; }

        public String Name { get; }
        public String GetBytesDependency { get; }
        public Boolean HasGetBytesDependency { get; }
        public String ToObjectDependency { get; }
        public Boolean HasToObjectDependency { get; }

        public virtual Expression CreateGetBytes(ParameterExpression value, Boolean littleEndian)
        {
            throw new NotImplementedException();
        }

        public virtual Expression CreateGetBytes(ParameterExpression value, Boolean littleEndian, ParameterExpression dependency)
        {
            throw new NotImplementedException();
        }

        public virtual Expression CreateGetBytes(ParameterExpression value, LambdaExpression getBytesMethod)
        {
            throw new NotImplementedException();
        }

        public virtual Expression CreateGetBytes(ParameterExpression value, LambdaExpression getBytesMethod, ParameterExpression dependency)
        {
            throw new NotImplementedException();
        }

        public virtual Expression CreateToObject(ParameterExpression bytes, ParameterExpression startIndex, Boolean littleEndian)
        {
            throw new NotImplementedException();
        }

        public virtual Expression CreateToObject(ParameterExpression bytes, ParameterExpression startIndex, Boolean littleEndian, Expression dependency)
        {
            throw new NotImplementedException();
        }

        public virtual Expression CreateToObject(ParameterExpression bytes, ParameterExpression startIndex, LambdaExpression toObjectMethod)
        {
            throw new NotImplementedException();
        }

        public virtual Expression CreateToObject(ParameterExpression bytes, ParameterExpression startIndex, LambdaExpression toObjectMethod, Expression dependency)
        {
            throw new NotImplementedException();
        }
    }
}
