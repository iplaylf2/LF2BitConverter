using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LF2BitConverter.Extenison
{
    class ExpressionExtension
    {
        public static BlockExpression Foreach(Expression list, Func<ParameterExpression, Expression> body)
        {
            var elementType = list.Type.GetElementType();

            var item = Expression.Variable(elementType, "item");
            var enumerator = Expression.Variable(typeof(IEnumerator<>).MakeGenericType(elementType), "enumerator");
            var breakLabel = Expression.Label("loopBreak");

            return Expression.Block(
                //var enumerator;
                new[] { enumerator },
                //enumerator=list.GetEnumerator();
                Expression.Assign(
                    enumerator,
                    Expression.Call(
                        list,
                        typeof(IEnumerable<>).MakeGenericType(elementType).GetMethod(nameof(IEnumerable<object>.GetEnumerator)))),
                //loop
                Expression.Loop(
                    Expression.IfThenElse(
                        //if(enumerator.MoveNext())
                        Expression.Call(
                            enumerator,
                            typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext))),
                        //true
                        Expression.Block(
                            new[] { item },
                            //var item=enumerator.Current;
                            Expression.Assign(
                                item,
                                Expression.Property(enumerator, nameof(IEnumerator<object>.Current))),
                            //{body}
                            body(item)),
                        //false break;
                        Expression.Break(breakLabel)),
                    breakLabel));
        }
    }
}
