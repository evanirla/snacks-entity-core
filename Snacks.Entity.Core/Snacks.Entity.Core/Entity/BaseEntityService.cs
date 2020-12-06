using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using MySql.Data.MySqlClient;
using Snacks.Entity.Core.Caching;
using Snacks.Entity.Core.Database;
using Snacks.Entity.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Entity
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TDbService"></typeparam>
    /// <typeparam name="TDbConnection"></typeparam>
    public abstract class BaseEntityService<TModel, TKey, TDbService, TDbConnection> : 
        IEntityService<TModel, TKey, TDbService, TDbConnection>
        where TModel : IEntityModel<TKey>
        where TDbConnection : IDbConnection
        where TDbService : IDbService<IDbConnection>
    {
        protected readonly TDbService _databaseService;
        protected readonly IEntityCacheService<TModel, TKey> _cacheService;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly ILogger _logger;

        public TableMapping TableMapping { get; private set; }

        public BaseEntityService(
            IServiceProvider serviceProvider,
            ILogger logger)
        {
            _databaseService = (TDbService)serviceProvider.GetService(typeof(IDbService<IDbConnection>));
            _cacheService = (IEntityCacheService<TModel, TKey>)serviceProvider.GetService(typeof(IEntityCacheService<TModel, TKey>));
            _serviceProvider = serviceProvider;
            _logger = logger;

            TableMapping = TableMapping.GetMapping<TModel>();
        }

        public virtual async Task<IList<TModel>> CreateManyAsync(IList<TModel> models, IDbTransaction transaction = null)
        {
            _logger.LogInformation($"Inserting {models.Count} {typeof(TModel).Name}s");

            List<TModel> newModels = new List<TModel>();

            foreach (TModel model in models)
            {
                newModels.Add(await CreateOneAsync(model, transaction));
            }

            _logger.LogInformation($"{newModels.Count} {typeof(TModel).Name}s inserted");

            if (_cacheService != null)
            {
                await _cacheService.RemoveManyAsync();
            }

            return newModels;
        }

        public virtual async Task<TModel> CreateOneAsync(TModel model, IDbTransaction transaction = null)
        {
            if (!TableMapping.KeyColumn.IsDatabaseGenerated || 
                TableMapping.KeyColumn.GetValue(model) == null ||
                TableMapping.KeyColumn.GetValue(model).Equals(default(TKey)))
            {
                throw new KeyValueInvalidException();
            }

            _logger.LogInformation($"Inserting one {typeof(TModel).Name}");

            if (model.IdempotencyKey != null)
            {
                TModel idempotentModel = await _cacheService.GetCustomOneAsync(
                    $"{typeof(TModel).Name}(Idempotency={model.IdempotencyKey})");

                if (idempotentModel != null)
                {
                    throw new IdempotencyKeyUsedException();
                }
            }

            string statement = GetInsertStatement();

            await _databaseService.ExecuteSqlAsync(
                statement,
                GetDynamicInsertParameters(model),
                transaction);

            model = await GetLastInsertedAsync(transaction);

            _logger.LogInformation($"{typeof(TModel).Name} ({TableMapping.KeyColumn.GetValue(model)}) inserted");

            if (model.IdempotencyKey != null)
            {
                await _cacheService.SetCustomOneAsync(
                    $"{typeof(TModel).Name}(Idempotency={model.IdempotencyKey})", model);
            }

            await _cacheService.SetOneAsync(model);
            await _cacheService.RemoveManyAsync();

            return model;
        }

        public virtual async Task DeleteOneAsync(TModel model, IDbTransaction transaction = null)
        {
            string statement = GetDeleteStatement();

            await _databaseService.ExecuteSqlAsync(statement, new { Key = TableMapping.KeyColumn.GetValue(model) });

            _logger.LogInformation($"{typeof(TModel).Name} ({TableMapping.KeyColumn.GetValue(model)}) deleted");
            await _cacheService.RemoveOneAsync(model);
            await _cacheService.RemoveManyAsync();
        }

        public virtual async Task DeleteOneAsync(TKey key, IDbTransaction transaction = null)
        {
            if (key == null || key.Equals(default(TKey)))
            {
                throw new KeyValueInvalidException();
            }

            _logger.LogInformation($"Deleting {typeof(TModel).Name} ({key})");

            string statement = GetDeleteStatement();
            await _databaseService.ExecuteSqlAsync(statement, new { Key = key });

            _logger.LogInformation($"{typeof(TModel).Name} ({key}) deleted");
            await _cacheService.RemoveOneAsync(key);
            await _cacheService.RemoveManyAsync();
        }

        public virtual async Task<IList<TModel>> GetManyAsync(IQueryCollection queryCollection, IDbTransaction transaction = null)
        {
            _logger.LogInformation($"Retrieving multiple {typeof(TModel).Name}s");

            IList<TModel> cachedModels = await _cacheService.GetManyAsync(queryCollection);

            if (cachedModels != null)
            {
                _logger.LogInformation(
                    $"Retrieved {cachedModels.Count} cached {typeof(TModel).Name}s");
                return cachedModels;
            }

            string statement = GetSelectStatement(queryCollection);
            DynamicParameters parameters = GetDynamicQueryParameters(queryCollection);

            List<TModel> models = (await _databaseService.QueryAsync<TModel>(statement, parameters, transaction)).ToList();

            _logger.LogInformation($"Retrieved {models.Count} {typeof(TModel).Name}s");

            await _cacheService.SetManyAsync(queryCollection, models);

            return models;
        }

        public virtual async Task<IList<TModel>> GetManyAsync(Dictionary<string, StringValues> queryCollection, IDbTransaction transaction = null)
        {
            return await GetManyAsync(
                new QueryCollection(queryCollection), transaction);
        }

        public virtual async Task<TModel> GetOneAsync(TKey key, IDbTransaction transaction = null)
        {
            _logger.LogInformation($"Retrieving one {typeof(TModel).Name} with key '{key}'");

            TModel cachedModel = await _cacheService.GetOneAsync(key);

            if (cachedModel != null)
            {
                _logger.LogInformation($"{typeof(TModel).Name} retrieved from cache.");
                return cachedModel;
            }

            string statement = GetSelectByKeyStatement();

            TModel model = await _databaseService.QuerySingleAsync<TModel>(statement, new { Key = key }, transaction);

            if (model != null)
            {
                _logger.LogInformation($"{typeof(TModel).Name} ({key}) retrieved");
                await _cacheService.SetOneAsync(model);
            }
            else
            {
                _logger.LogInformation($"{typeof(TModel).Name} ({key}) not found");
            }

            return model;
        }

        public virtual async Task UpdateOneAsync(TModel model, IDbTransaction transaction = null)
        {
            if (TableMapping.KeyColumn.GetValue(model) == null)
            {
                throw new ArgumentException("Model must contain a value for Id.");
            }

            _logger.LogInformation($"Updating {typeof(TModel).Name} ({TableMapping.KeyColumn.GetValue(model)})");

            string statement = GetUpdateStatement();
            DynamicParameters parameters = GetDynamicUpdateParameters(model);

            await _databaseService.ExecuteSqlAsync(
                statement,
                parameters,
                transaction);

            await _cacheService.RemoveOneAsync(model);
            await _cacheService.RemoveManyAsync();

            model = await GetOneAsync((TKey)TableMapping.KeyColumn.GetValue(model), transaction);

            await _cacheService.SetOneAsync(model);

            _logger.LogInformation($"Updated {typeof(TModel).Name} ({TableMapping.KeyColumn.GetValue(model)})");
            await _cacheService.RemoveManyAsync();
        }

        private string GetSelectByKeyStatement()
        {
            List<Tuple<PropertyInfo, ColumnAttribute>> columns = GetColumns();

            return $@"
                select {string.Join(',', columns.Select(x => $"{x.Item2.Name} `{x.Item1.Name}`"))}
                from {TableMapping.Name}
                where {TableMapping.KeyColumn.Name} = @Key";
        }

        private string GetSelectStatement(IQueryCollection queryCollection)
        {
            Type entityType = typeof(TModel);

            List<Tuple<PropertyInfo, ColumnAttribute>> columns = GetColumns();

            Regex filterRegex = new Regex(@"(.*?)\[(.*?)\]", RegexOptions.IgnoreCase);

            List<Tuple<Tuple<PropertyInfo, ColumnAttribute>, string, object>> filters =
                GetSelectFilters(queryCollection);

            int? limit = null;
            int? offset = null;
            StringValues orderByAsc;
            StringValues orderByDesc;

            if (queryCollection.ContainsKey("limit"))
            {
                limit = Convert.ToInt32(queryCollection["limit"]);
            }

            if (queryCollection.ContainsKey("offset"))
            {
                offset = Convert.ToInt32(queryCollection["offset"]);
            }

            if (queryCollection.ContainsKey("orderby[asc]"))
            {
                orderByAsc = queryCollection["orderby[asc]"];
            }

            if (queryCollection.ContainsKey("orderby[desc]"))
            {
                orderByDesc = queryCollection["orderby[desc]"];
            }

            List<string> filterStrings = new List<string>();

            foreach (var filter in filters)
            {
                if (filter.Item2 == "like")
                {
                    filterStrings.Add($"lower({filter.Item1.Item2.Name}) like lower(@{filter.Item1.Item1.Name})");
                }
                else
                {
                    filterStrings.Add($"{filter.Item1.Item2.Name} {filter.Item2} @{filter.Item1.Item1.Name}");
                }
            }

            List<Tuple<ColumnAttribute, string>> orderByColumns =
                new List<Tuple<ColumnAttribute, string>>();

            foreach (string s in orderByAsc)
            {
                Tuple<PropertyInfo, ColumnAttribute> orderByColumn =
                    columns.FirstOrDefault(x =>
                        x.Item1.Name.Equals(s, StringComparison.InvariantCultureIgnoreCase) ||
                        x.Item2.Name.Equals(s, StringComparison.InvariantCultureIgnoreCase));

                if (orderByColumn != null)
                {
                    orderByColumns.Add(new Tuple<ColumnAttribute, string>(
                        orderByColumn.Item2, "asc"));
                }
            }

            foreach (string s in orderByDesc)
            {
                Tuple<PropertyInfo, ColumnAttribute> orderByColumn =
                    columns.FirstOrDefault(x =>
                        x.Item1.Name.Equals(s, StringComparison.InvariantCultureIgnoreCase) ||
                        x.Item2.Name.Equals(s, StringComparison.InvariantCultureIgnoreCase));

                if (orderByColumn != null)
                {
                    orderByColumns.Add(new Tuple<ColumnAttribute, string>(
                        orderByColumn.Item2, "desc"));
                }
            }

            return $@"
                select {string.Join(',', columns.Select(x => $"{x.Item2.Name} `{x.Item1.Name}`"))}
                from {TableMapping.Name}
                {(filters.Any() ? "where" : "")} {string.Join(" and ", filterStrings)}
                {(orderByColumns.Count > 0 ? $"order by {string.Join(',', orderByColumns.Select(x => $"{x.Item1.Name} {x.Item2}"))}" : "")}
                {(limit != null ? $"limit {offset ?? 0}, {limit}" : "")}";
        }

        private DynamicParameters GetDynamicQueryParameters(IQueryCollection queryCollection)
        {
            List<Tuple<Tuple<PropertyInfo, ColumnAttribute>, string, object>> filters = GetSelectFilters(queryCollection);

            DynamicParameters parameters = new DynamicParameters();

            foreach (var filter in filters)
            {
                parameters.Add(filter.Item1.Item1.Name, filter.Item3);
            }

            return parameters;
        }

        private string GetUpdateStatement()
        {
            IEnumerable<Tuple<PropertyInfo, ColumnAttribute>> columns =
                GetColumns().Where(x => !x.Item1.IsDefined(typeof(ReadOnlyAttribute)));

            return $@"
                update {TableMapping.Name}
                set {string.Join(",", columns.Select(x => $"{x.Item2.Name} = @{x.Item1.Name}"))}
                where key = @Key";
        }

        private DynamicParameters GetDynamicUpdateParameters(TModel model)
        {
            DynamicParameters parameters = new DynamicParameters();

            foreach (Tuple<PropertyInfo, ColumnAttribute> column in GetColumns())
            {
                parameters.Add(column.Item1.Name, column.Item1.GetValue(model));
            }

            return parameters;
        }

        private string GetDeleteStatement()
        {
            return $"delete from {TableMapping.Name} where id = @Id";
        }

        private string GetInsertStatement()
        {
            Type entityType = typeof(TModel);

            if (!entityType.IsDefined(typeof(TableAttribute)))
            {
                throw new Exception($"{entityType.Name} must have a table attribute specified.");
            }

            IEnumerable<Tuple<PropertyInfo, ColumnAttribute>> columns =
                GetColumns().Where(x => x.Item1.GetCustomAttribute<ReadOnlyAttribute>() == null);

            return $@"
                insert into {TableMapping.Name} ({string.Join(",", columns.Select(x => $"{x.Item2.Name}"))})
                values ({string.Join(",", columns.Select(x => $"@{x.Item1.Name}"))})";
        }

        private DynamicParameters GetDynamicInsertParameters(TModel model)
        {
            DynamicParameters parameters = new DynamicParameters();

            foreach (Tuple<PropertyInfo, ColumnAttribute> column in GetColumns())
            {
                if (column.Item1.GetCustomAttribute<ReadOnlyAttribute>() == null)
                {
                    parameters.Add(column.Item1.Name, column.Item1.GetValue(model));
                }
            }

            return parameters;
        }

        private List<Tuple<PropertyInfo, ColumnAttribute>> GetColumns()
        {
            Type entityType = typeof(TModel);

            List<PropertyInfo> properties = entityType.GetProperties().ToList();

            List<Tuple<PropertyInfo, ColumnAttribute>> columns = new List<Tuple<PropertyInfo, ColumnAttribute>>();

            foreach (PropertyInfo property in properties)
            {
                ColumnAttribute columnAttribute = property.GetCustomAttribute<ColumnAttribute>();

                if (columnAttribute != null)
                {
                    columns.Add(new Tuple<PropertyInfo, ColumnAttribute>(property, columnAttribute));
                }
            }

            return columns;
        }

        private List<Tuple<Tuple<PropertyInfo, ColumnAttribute>, string, object>> GetSelectFilters(IQueryCollection queryCollection)
        {
            List<Tuple<PropertyInfo, ColumnAttribute>> columns = GetColumns();
            List<Tuple<Tuple<PropertyInfo, ColumnAttribute>, string, object>> filters =
                new List<Tuple<Tuple<PropertyInfo, ColumnAttribute>, string, object>>();

            Regex filterRegex = new Regex(@"(.*?)\[(.*?)\]", RegexOptions.IgnoreCase);

            foreach (KeyValuePair<string, StringValues> query in queryCollection)
            {
                if (filterRegex.IsMatch(query.Key))
                {
                    Match filterMatch = filterRegex.Match(query.Key);

                    GroupCollection filterGroups = filterMatch.Groups;

                    Tuple<PropertyInfo, ColumnAttribute> column =
                        columns.FirstOrDefault(x =>
                            x.Item1.Name.Equals(filterGroups[1].Value, StringComparison.InvariantCultureIgnoreCase) ||
                            x.Item2.Name.Equals(filterGroups[1].Value, StringComparison.InvariantCultureIgnoreCase));

                    if (column == null)
                    {
                        continue;
                    }

                    string @operator = "";

                    @operator = filterGroups[2].Value switch
                    {
                        "eq" => "=",
                        "lt" => "<",
                        "lte" => "<=",
                        "gt" => ">",
                        "gte" => ">=",
                        "like" => "like",
                        _ => throw new Exception("Operator not valid."),
                    };
                    object castedValue = null;

                    if (column.Item1.PropertyType == typeof(DateTime) || column.Item1.PropertyType == typeof(DateTime?))
                    {
                        castedValue = DateTime.Parse(query.Value.ToString());
                    }
                    else if (column.Item1.PropertyType == typeof(int) || column.Item1.PropertyType == typeof(int?))
                    {
                        castedValue = int.Parse(query.Value.ToString());
                    }
                    else
                    {
                        castedValue = Convert.ChangeType(
                            query.Value.ToString(),
                            column.Item1.PropertyType);
                    }

                    filters.Add(new Tuple<Tuple<PropertyInfo, ColumnAttribute>, string, object>(
                        column,
                        @operator,
                        castedValue));
                }
            }

            return filters;
        }

        protected virtual async Task<TModel> GetLastInsertedAsync(IDbTransaction transaction)
        {
            if (typeof(TDbConnection) == typeof(MySqlConnection))
            {
                int? id = await _databaseService.QuerySingleAsync<int>(
                    "select last_insert_id() from dual", null, transaction);

                await _databaseService.QuerySingleAsync<TModel>(@$"
                    select *
                    from {TableMapping.Name}
                    where {TableMapping.KeyColumn.Name} = @id", new { id }, transaction);
            }
            else if (typeof(TDbConnection) == typeof(SqliteConnection))
            {
                return await _databaseService.QuerySingleAsync<TModel>(@$"
                    SELECT *
                    FROM {TableMapping.Name}
                    WHERE ROWID = LAST_INSERT_ROWID()", null, transaction);
            }

            return default;
        }

        public virtual Task InitializeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
