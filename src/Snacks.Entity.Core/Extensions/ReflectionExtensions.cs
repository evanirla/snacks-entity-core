using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Snacks.Entity.Core.Extensions
{
    internal static class MethodInfoExtensions
    {
        public static MethodCallExpression GetLinqExpression(this MethodInfo methodInfo, Expression instance, params Expression[] parameters)
        {
            if (methodInfo.IsStatic)
            {
                var combinedParams = new List<Expression>();
                combinedParams.Add(instance);
                combinedParams.AddRange(parameters);
                return Expression.Call(null, methodInfo, combinedParams);
            }
            else
            {
                return Expression.Call(instance, methodInfo, parameters);
            }
        }
    }
}