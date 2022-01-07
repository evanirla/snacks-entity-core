using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Snacks.Entity.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Snacks.Entity.Core.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IQueryable"/>
    /// </summary>
    internal static class IQueryableExtensions
    {
        private static IQueryable<TEntity> ApplyQueryParameters<TEntity>(this IQueryable<TEntity> queryable, IQueryCollection queryParameters)
            where TEntity : class
        {
            var parameters = QueryableParameters.Build(queryParameters);
            return parameters.ApplyLinqExpressions<TEntity, IQueryable<TEntity>>(queryable);
        }
    }
}
