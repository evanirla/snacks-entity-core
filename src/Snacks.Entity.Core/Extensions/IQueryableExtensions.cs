using Microsoft.AspNetCore.Http;
using Snacks.Entity.Core.Helpers;
using System.Linq;

namespace Snacks.Entity.Core.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IQueryable"/>
    /// </summary>
    internal static class IQueryableExtensions
    {
        public static IQueryable<TEntity> ApplyQueryParameters<TEntity>(this IQueryable<TEntity> queryable, IQueryCollection queryParameters)
            where TEntity : class
        {
            var parameters = QueryableParameters.Build(queryParameters);
            return parameters.ApplyLinqExpressions<TEntity, IQueryable<TEntity>>(queryable);
        }
    }
}
