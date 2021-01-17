using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Snacks.Entity.Core.Extensions
{
    public static class IQueryableExtensions
    {
        private static Regex _filterRegex = new Regex(@"(.*?)\[(.*?)\]", RegexOptions.IgnoreCase);
        private static Dictionary<Type, PropertyInfo[]> _entityProperties = new Dictionary<Type, PropertyInfo[]>();
        
        public static IQueryable<TEntity> ApplyQueryParameters<TEntity>(this IQueryable<TEntity> queryable, IQueryCollection queryParameters)
            where TEntity : class
        {
            PropertyInfo[] properties;

            if (_entityProperties.ContainsKey(typeof(TEntity)))
            {
                properties = _entityProperties[typeof(TEntity)];
            }
            else
            {
                properties = typeof(TEntity).GetProperties();
                _entityProperties.Add(typeof(TEntity), properties);
            }

            List<Expression<Func<TEntity, bool>>> expressions = new List<Expression<Func<TEntity, bool>>>();
            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");

            foreach (var param in queryParameters)
            {
                PropertyInfo property = null;
                string @operator = "=";

                if (_filterRegex.IsMatch(param.Key))
                {
                    Match filterMatch = _filterRegex.Match(param.Key);
                    GroupCollection filterGroups = filterMatch.Groups;

                    property = properties.FirstOrDefault(x => x.Name.Equals(filterGroups[1].Value, StringComparison.InvariantCultureIgnoreCase));
                }
                else
                {
                    property = properties.FirstOrDefault(x => x.Name.Equals(param.Key, StringComparison.InvariantCultureIgnoreCase));
                }

                if (property != null)
                {
                    MemberExpression member = Expression.Property(parameter, property);
                    Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    object castedValue = null;

                    if (propertyType == typeof(DateTime))
                    {
                        castedValue = DateTime.Parse(param.Value);
                    }
                    else if (propertyType == typeof(int))
                    {
                        castedValue = Convert.ToInt32(param.Value);
                    }
                    else
                    {
                        castedValue = Convert.ChangeType(param.Value, propertyType);
                    }

                    ConstantExpression constant = Expression.Constant(castedValue);

                    BinaryExpression equality = default;

                    equality = @operator switch
                    {
                        "lt" => Expression.LessThan(member, constant),
                        "lte" => Expression.LessThanOrEqual(member, constant),
                        "gt" => Expression.GreaterThan(member, constant),
                        "gte" => Expression.GreaterThanOrEqual(member, constant),
                        _ => throw new Exception("Operator not valid."),
                    };

                    queryable = queryable.Where(Expression.Lambda<Func<TEntity, bool>>(equality, new[] { parameter }));
                }
            }

            // apply order by?

            if (queryParameters.ContainsKey("offset"))
            {
                int offset = Convert.ToInt32(queryParameters["offset"]);
                queryable = queryable.Take(offset);
            }

            if (queryParameters.ContainsKey("limit"))
            {
                int limit = Convert.ToInt32(queryParameters["limit"]);
                queryable = queryable.Take(limit);
            }

            return queryable;
        }
    }
}
