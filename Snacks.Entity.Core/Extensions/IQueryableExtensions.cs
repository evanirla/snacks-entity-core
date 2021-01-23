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
    enum SortOrder 
    { 
        Ascending, 
        Descending 
    }

    enum Operator 
    { 
        Equal,
        NotEqual,
        Like,
        In,
        LessThan, 
        LessThanOrEqual, 
        GreaterThan,
        GreaterThanOrEqual
    }

    class QueryableParameters
    {
        const string ORDERBY = "orderby";
        const string LIMIT = "limit";
        const string OFFSET = "offset";
        static readonly Regex _paramRegex = new Regex(@"(.*?)\[(.*?)\]", RegexOptions.IgnoreCase);

        public int? Limit { get; set; }
        public int? Offset { get; set; }
        public List<Tuple<string, SortOrder>> Orders { get; set; }
        public List<Tuple<string, Operator, StringValues>> Filters { get; set; }

        public static QueryableParameters Build(IQueryCollection queryParameters)
        {
            QueryableParameters queryableParameters = new QueryableParameters();

            foreach (KeyValuePair<string, StringValues> param in queryParameters)
            {
                if (param.Key == LIMIT)
                {
                    int.TryParse(param.Value, out int value);
                    queryableParameters.Limit = value;
                    continue;
                }
                else if (param.Key == OFFSET)
                {
                    int.TryParse(param.Value, out int value);
                    queryableParameters.Offset = value;
                    continue;
                }

                Match paramMatch = _paramRegex.Match(param.Key);

                if (paramMatch.Success)
                {
                    string key = paramMatch.Groups[1].Value;
                    string @operator = paramMatch.Groups[2].Value;

                    if (key == ORDERBY)
                    {
                        queryableParameters.Orders.Add(
                            new Tuple<string, SortOrder>(param.Value, GetSortOrder(@operator)));
                    }
                    else
                    {
                        queryableParameters.Filters.Add(
                            new Tuple<string, Operator, StringValues>(
                                key, 
                                GetOperator(@operator, param.Value), 
                                param.Value));
                    }
                }
                else
                {
                    if (param.Key == ORDERBY)
                    {
                        queryableParameters.Orders.Add(
                            new Tuple<string, SortOrder>(param.Value, default));
                    }
                    else
                    {
                        queryableParameters.Filters.Add(
                            new Tuple<string, Operator, StringValues>(
                                param.Key,
                                default,
                                param.Value));
                    }
                }
            }

            return queryableParameters;
        }

        static SortOrder GetSortOrder(string sortOrder)
        {
            return sortOrder switch
            {
                "desc" => SortOrder.Descending,
                "asc" => SortOrder.Ascending,
                _ => default,
            };
        }

        static Operator GetOperator(string @operator, StringValues strings)
        {
            switch (@operator)
            {
                case "!":
                    return Operator.NotEqual;
                case "lt":
                    return Operator.LessThan;
                case "lte":
                    return Operator.GreaterThan;
                case "gt":
                    return Operator.GreaterThan;
                case "gte":
                    return Operator.GreaterThanOrEqual;
                default:
                    if (strings.Count > 1)
                    {
                        return Operator.In;
                    }
                    return default;
            }
        }
    }

    public static class IQueryableExtensions
    {
        private static readonly Regex _filterRegex = new Regex(@"(.*?)\[(.*?)\]", RegexOptions.IgnoreCase);
        private static readonly Dictionary<Type, PropertyInfo[]> _entityProperties = new Dictionary<Type, PropertyInfo[]>();
        
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

            QueryableParameters queryableParameters = QueryableParameters.Build(queryParameters);

            for (var filter = queryParameters.)

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
