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

        public QueryableParameters()
        {
            Orders = new List<Tuple<string, SortOrder>>();
            Filters = new List<Tuple<string, Operator, StringValues>>();
        }

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
                                GetOperator(null, param.Value),
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

            foreach (var filter in queryableParameters.Filters)
            {
                PropertyInfo property = properties.FirstOrDefault(
                    x => x.Name.Equals(filter.Item1, StringComparison.InvariantCultureIgnoreCase));

                if (property != null)
                {
                    MemberExpression member = Expression.Property(parameter, property);

                    if (filter.Item2 == Operator.In)
                    {
                        Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                        dynamic valueList = Activator.CreateInstance(typeof(List<>).MakeGenericType(propertyType));

                        var values = filter.Item3
                            .Select(x => ConvertStringToPropertyType(x, property));

                        foreach (var value in values)
                        {
                            valueList.Add(Convert.ChangeType(value, propertyType));
                        }

                        Expression containsExpression = Expression.Call(
                            Expression.Constant(valueList),
                            valueList.GetType().GetMethod("Contains"),
                            member);

                        queryable = queryable.Where(Expression.Lambda<Func<TEntity, bool>>(containsExpression, parameter));
                    }
                    else
                    {
                        object value = ConvertStringToPropertyType(filter.Item3, property);
                        ConstantExpression constant = Expression.Constant(value);

                        BinaryExpression equality = filter.Item2 switch
                        {
                            Operator.Equal => Expression.Equal(member, constant),
                            Operator.NotEqual => Expression.NotEqual(member, constant),
                            Operator.LessThan => Expression.LessThan(member, constant),
                            Operator.LessThanOrEqual => Expression.LessThanOrEqual(member, constant),
                            Operator.GreaterThan => Expression.GreaterThan(member, constant),
                            Operator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(member, constant),
                            _ => throw new Exception("Operator not valid."),
                        };

                        queryable = queryable.Where(Expression.Lambda<Func<TEntity, bool>>(equality, parameter));
                    }
                }
            }

            foreach (var order in queryableParameters.Orders)
            {
                PropertyInfo property = properties
                    .FirstOrDefault(x => x.Name.Equals(order.Item1, StringComparison.InvariantCultureIgnoreCase));

                if (property != null)
                {
                    MemberExpression member = Expression.Property(parameter, property);

                    var lambda = Expression.Lambda<Func<TEntity, dynamic>>(member, parameter);

                    if (order.Item2 == SortOrder.Descending)
                    {
                        queryable = queryable.OrderByDescending(lambda);
                    }
                    else
                    {
                        queryable = queryable.OrderBy(lambda);
                    }
                }
            }

            if (queryableParameters.Offset.HasValue)
            {
                queryable = queryable.Skip(queryableParameters.Offset.Value);
            }

            if (queryableParameters.Limit.HasValue)
            {
                queryable = queryable.Take(queryableParameters.Limit.Value);
            }

            return queryable;
        }

        private static object ConvertStringToPropertyType(string value, PropertyInfo property)
        {
            Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            if (propertyType == typeof(DateTime))
            {
                return DateTime.Parse(value);
            }
            else if (propertyType == typeof(int))
            {
                return Convert.ToInt32(value);
            }
            else
            {
                return Convert.ChangeType(value, propertyType);
            }
        }
    }
}
