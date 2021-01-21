using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
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

            lock (_entityProperties)
            {
                if (_entityProperties.ContainsKey(typeof(TEntity)))
                {
                    properties = _entityProperties[typeof(TEntity)];
                }
                else
                {
                    properties = typeof(TEntity).GetProperties();
                    _entityProperties.Add(typeof(TEntity), properties);
                }
            }

            List<Expression<Func<TEntity, bool>>> expressions = new List<Expression<Func<TEntity, bool>>>();
            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");

            List<Tuple<string, string, StringValues>> otherParameters = new List<Tuple<string, string, StringValues>>();

            foreach (var param in queryParameters)
            {
                PropertyInfo property = null;
                string propertyName = string.Empty;
                string @operator = "=";

                if (_filterRegex.IsMatch(param.Key))
                {
                    Match filterMatch = _filterRegex.Match(param.Key);
                    GroupCollection filterGroups = filterMatch.Groups;

                    propertyName = filterGroups[1].Value;
                    @operator = filterGroups[2].Value;
                }
                else
                {
                    propertyName = param.Key;
                }

                property = properties.FirstOrDefault(x => x.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));

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

                    queryable = queryable.Where(Expression.Lambda<Func<TEntity, bool>>(equality, parameter));
                }
                else
                {
                    otherParameters.Add(new Tuple<string, string, StringValues>(propertyName, @operator, param.Value));
                }
            }

            foreach (var param in otherParameters)
            {
                if (param.Item1 == "orderby")
                {
                    bool descending = param.Item2 == "desc";

                    foreach (string propertyName in param.Item3)
                    {
                        PropertyInfo property = properties.FirstOrDefault(x => x.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));

                        if (property != null)
                        {
                            MemberExpression member = Expression.Property(parameter, property);

                            var lambda = Expression.Lambda<Func<TEntity, dynamic>>(member, parameter);

                            if (descending)
                            {
                                queryable = queryable.OrderByDescending(lambda);
                            }
                            else
                            {
                                queryable = queryable.OrderBy(lambda);
                            }
                        }
                    }
                }
            }

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
