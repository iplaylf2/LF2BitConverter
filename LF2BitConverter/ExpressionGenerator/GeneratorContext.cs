using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace LF2BitConverter.ExpressionGenerator
{
    public struct GeneratorContext
    {
        public Boolean LittleEndian;
        public Dictionary<String, ParameterExpression> VariableMap;
        public List<Expression> Pretreatment;
        public List<(String, Expression)> MemberResult;
        public ConvertMember[] MemberArray;
    }
}
