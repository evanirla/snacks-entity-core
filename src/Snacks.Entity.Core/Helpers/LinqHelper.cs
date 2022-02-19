using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Snacks.Entity.Core.Helpers
{
    internal static class LinqHelper
    {
        private static readonly Dictionary<Type, MethodInfo> _methodCache = new();
        static LinqHelper()
        {

        }

        public static MethodInfo GetMethod<TEntity, TEnumerable>(string name) where TEnumerable : IEnumerable<TEntity>
        {
            if (_methodCache.ContainsKey(typeof(TEnumerable)))
            {
                return _methodCache[typeof(TEnumerable)];
            }
            else
            {
                var method = typeof(TEnumerable).GetMethods().SingleOrDefault(x => x.Name == name);
                if (method == default)
                {
                    // fallback to extension methods
                    method = typeof(Enumerable).GetMethods().FirstOrDefault(x => x.Name == name);
                    method = method.MakeGenericMethod(typeof(TEntity));
                }

                _methodCache.Add(typeof(TEnumerable), method);
                return method;
            }
        }
    }
}