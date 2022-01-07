using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Snacks.Entity.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Snacks.Entity.Core.Helpers
{
    internal enum SortOrder
    {
        Ascending,
        Descending
    }

    internal enum Operator
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

    internal class QueryableParameters
    {
        public class Order
        {
            public string PropertyName { get; set; }
            public SortOrder SortOrder { get; set; }

            public Expression<Func<TEntity, dynamic>> GetExpression<TEntity>() where TEntity : class
            {
                PropertyInfo[] properties = typeof(TEntity).GetProperties();
                ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");
                PropertyInfo property = properties.FirstOrDefault(
                    x => x.Name.Equals(PropertyName, StringComparison.InvariantCultureIgnoreCase));

                if (property != null)
                {
                    MemberExpression member = Expression.Property(parameter, property);

                    return Expression.Lambda<Func<TEntity, dynamic>>(member, parameter);
                }

                return default;
            }
        }

        public class Filter
        {
            public string PropertyName { get; set; }
            public Operator Operator { get; set; }
            public StringValues Values { get; set; }

            public Expression<Func<TEntity, bool>> GetExpression<TEntity>() where TEntity : class
            {
                PropertyInfo[] properties = typeof(TEntity).GetProperties();
                ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");
                PropertyInfo property = properties.FirstOrDefault(
                    x => x.Name.Equals(PropertyName, StringComparison.InvariantCultureIgnoreCase));

                if (property != null)
                {
                    MemberExpression member = Expression.Property(parameter, property);

                    if (Operator == Operator.In)
                    {
                        Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                        dynamic valueList = Activator.CreateInstance(typeof(List<>).MakeGenericType(propertyType));

                        var values = Values
                            .Select(x => x.ConvertToPropertyType(property));

                        foreach (var value in values)
                        {
                            valueList.Add(Convert.ChangeType(value, propertyType));
                        }

                        Expression containsExpression = Expression.Call(
                            Expression.Constant(valueList),
                            valueList.GetType().GetMethod("Contains"),
                            member);

                        return Expression.Lambda<Func<TEntity, bool>>(containsExpression, parameter);
                    }
                    else
                    {
                        object value = Values.ToString().ConvertToPropertyType(property);
                        ConstantExpression constant = Expression.Constant(value);

                        BinaryExpression equality = Operator switch
                        {
                            Operator.Equal => Expression.Equal(member, constant),
                            Operator.NotEqual => Expression.NotEqual(member, constant),
                            Operator.LessThan => Expression.LessThan(member, constant),
                            Operator.LessThanOrEqual => Expression.LessThanOrEqual(member, constant),
                            Operator.GreaterThan => Expression.GreaterThan(member, constant),
                            Operator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(member, constant),
                            _ => throw new Exception("Operator not valid."),
                        };

                        return Expression.Lambda<Func<TEntity, bool>>(equality, parameter);
                    }
                }

                return default;
            }
        }

        const string ORDERBY = "orderby";
        const string LIMIT = "limit";
        const string OFFSET = "offset";
        static readonly Regex _paramRegex = new Regex(@"(.*?)\[(.*?)\]", RegexOptions.IgnoreCase);

        public int? Limit { get; set; }
        public int? Offset { get; set; }
        public List<Order> Orders { get; set; }
        public List<Filter> Filters { get; set; }

        public QueryableParameters()
        {
            Orders = new List<Order>();
            Filters = new List<Filter>();
        }

        public LambdaExpression ApplyLinqExpressions<TEntity, TEnumerable>(LambdaExpression expression)
            where TEntity : class
            where TEnumerable : IEnumerable<TEntity>
        {
            PropertyInfo[] properties = typeof(TEntity).GetProperties();
            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");

            foreach (var filter in Filters)
            {
                var lambda = filter.GetExpression<TEntity>();
                expression = Expression.Lambda(
                    LinqHelper.GetMethod<TEntity, TEnumerable>("Where")
                        .GetLinqExpression(expression.Body, lambda),
                    expression.Parameters.FirstOrDefault()
                );
            }

            foreach (var order in Orders)
            {
                var lambda = order.GetExpression<TEntity>();

                if (order.SortOrder == SortOrder.Descending)
                {
                    expression = Expression.Lambda(
                        LinqHelper.GetMethod<TEntity, TEnumerable>("OrderByDescending")
                            .GetLinqExpression(expression.Body, lambda),
                        expression.Parameters.FirstOrDefault()
                    );
                }
                else
                {
                    expression = Expression.Lambda(
                        LinqHelper.GetMethod<TEntity, TEnumerable>("OrderBy")
                            .GetLinqExpression(expression.Body, lambda),
                        expression.Parameters.FirstOrDefault()
                    );
                }
            }

            if (Offset.HasValue)
            {
                expression = Expression.Lambda(
                    LinqHelper.GetMethod<TEntity, TEnumerable>("Skip")
                        .GetLinqExpression(expression.Body, Expression.Constant(Offset.Value)),
                    expression.Parameters.FirstOrDefault()
                );
            }

            if (Limit.HasValue)
            {
                expression = Expression.Lambda(
                    LinqHelper.GetMethod<TEntity, TEnumerable>("Take")
                        .GetLinqExpression(expression.Body, Expression.Constant(Limit.Value)),
                    expression.Parameters.FirstOrDefault()
                );
            }

            return expression;
        }

        public TQueryable ApplyLinqExpressions<TEntity, TQueryable>(TQueryable queryable)
            where TEntity : class
            where TQueryable : class, IQueryable<TEntity>
        {
            PropertyInfo[] properties = typeof(TEntity).GetProperties();
            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");

            foreach (var filter in Filters)
            {
                var lambda = filter.GetExpression<TEntity>();
                queryable = (TQueryable)queryable.Where(lambda);
            }

            foreach (var order in Orders)
            {
                var lambda = order.GetExpression<TEntity>();

                if (order.SortOrder == SortOrder.Descending)
                {
                    queryable = (TQueryable)queryable.OrderByDescending(lambda);
                }
                else
                {
                    queryable = (TQueryable)queryable.OrderBy(lambda);
                }
            }

            if (Offset.HasValue)
            {
                queryable = (TQueryable)queryable.Skip(Offset.Value);
            }

            if (Limit.HasValue)
            {
                queryable = (TQueryable)queryable.Take(Limit.Value);
            }

            return queryable;
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
                        queryableParameters.Orders.Add(new Order
                        {
                            PropertyName = param.Value,
                            SortOrder = GetSortOrder(@operator)
                        });
                    }
                    else
                    {
                        queryableParameters.Filters.Add(new Filter
                        {
                            PropertyName = key,
                            Operator = GetOperator(@operator, param.Value),
                            Values = param.Value
                        });
                    }
                }
                else
                {
                    if (param.Key == ORDERBY)
                    {
                        queryableParameters.Orders.Add(new Order
                        {
                            PropertyName = param.Value,
                            SortOrder = default
                        });
                    }
                    else
                    {
                        queryableParameters.Filters.Add(new Filter
                        {
                            PropertyName = param.Key,
                            Operator = GetOperator(null, param.Value),
                            Values = param.Value
                        });
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
}