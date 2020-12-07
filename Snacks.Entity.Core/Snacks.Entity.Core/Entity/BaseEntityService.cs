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
        where TDbService : IDbService<TDbConnection>
    {
        protected readonly TDbService _dbService;
        protected readonly IEntityCacheService<TModel, TKey> _cacheService;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly ILogger _logger;

        public TableMapping TableMapping { get; private set; }

        public BaseEntityService(
            IServiceProvider serviceProvider,
            ILogger logger)
        {
            _dbService = (TDbService)serviceProvider.GetService(typeof(IDbService<TDbConnection>));
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
            if (!TableMapping.KeyColumn.IsDatabaseGenerated &&
                (TableMapping.KeyColumn.GetValue(model) == null ||
                TableMapping.KeyColumn.GetValue(model).Equals(default(TKey))))
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

            await _dbService.ExecuteSqlAsync(
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

            await _dbService.ExecuteSqlAsync(statement, new { Key = TableMapping.KeyColumn.GetValue(model) });

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
            await _dbService.ExecuteSqlAsync(statement, new { Key = key });

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

            List<TModel> models = (await _dbService.QueryAsync<TModel>(statement, parameters, transaction)).ToList();

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

            TModel model = await _dbService.QuerySingleAsync<TModel>(statement, new { Key = key }, transaction);

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

            await _dbService.ExecuteSqlAsync(
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
            return $@"
                select {string.Join(',', TableMapping.Columns.Select(x => $"{x.Name} `{x.Property.Name}`"))}
                from {TableMapping.Name}
                where {TableMapping.KeyColumn.Name} = @Key";
        }

        private string GetSelectStatement(IQueryCollection queryCollection)
        {
            Type entityType = typeof(TModel);

            Regex filterRegex = new Regex(@"(.*?)\[(.*?)\]", RegexOptions.IgnoreCase);

            List<Tuple<TableColumnMapping, string, object>> filters =
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
                    filterStrings.Add($"lower({filter.Item1.Name}) like lower(@{filter.Item1.Property.Name})");
                }
                else
                {
                    filterStrings.Add($"{filter.Item1.Name} {filter.Item2} @{filter.Item1.Property.Name}");
                }
            }

            List<Tuple<TableColumnMapping, string>> orderByColumns =
                new List<Tuple<TableColumnMapping, string>>();

            foreach (string s in orderByAsc)
            {
                TableColumnMapping orderByColumn =
                    TableMapping.Columns.FirstOrDefault(x =>
                        x.Name.Equals(s, StringComparison.InvariantCultureIgnoreCase) ||
                        x.Property.Name.Equals(s, StringComparison.InvariantCultureIgnoreCase));

                if (orderByColumn != null)
                {
                    orderByColumns.Add(new Tuple<TableColumnMapping, string>(
                        orderByColumn, "asc"));
                }
            }

            foreach (string s in orderByDesc)
            {
                TableColumnMapping orderByColumn =
                    TableMapping.Columns.FirstOrDefault(x =>
                        x.Name.Equals(s, StringComparison.InvariantCultureIgnoreCase) ||
                        x.Property.Name.Equals(s, StringComparison.InvariantCultureIgnoreCase));

                if (orderByColumn != null)
                {
                    orderByColumns.Add(new Tuple<TableColumnMapping, string>(
                        orderByColumn, "desc"));
                }
            }

            return $@"
                select {string.Join(',', TableMapping.Columns.Select(x => $"{x.Name} `{x.Property.Name}`"))}
                from {TableMapping.Name}
                {(filters.Any() ? "where" : "")} {string.Join(" and ", filterStrings)}
                {(orderByColumns.Count > 0 ? $"order by {string.Join(',', orderByColumns.Select(x => $"{x.Item1.Name} {x.Item2}"))}" : "")}
                {(limit != null ? $"limit {offset ?? 0}, {limit}" : "")}";
        }

        private DynamicParameters GetDynamicQueryParameters(IQueryCollection queryCollection)
        {
            List<Tuple<TableColumnMapping, string, object>> filters = GetSelectFilters(queryCollection);

            DynamicParameters parameters = new DynamicParameters();

            foreach (var filter in filters)
            {
                parameters.Add(filter.Item1.Property.Name, filter.Item3);
            }

            return parameters;
        }

        private string GetUpdateStatement()
        {
            IEnumerable<TableColumnMapping> columns =
                TableMapping.Columns.Where(x => !(
                    x.IsDatabaseGenerated && 
                    x.DatabaseGeneratedAttribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed));

            return $@"
                update {TableMapping.Name}
                set {string.Join(",", columns.Select(x => $"{x.Name} = @{x.Property.Name}"))}
                where key = @Key";
        }

        private DynamicParameters GetDynamicUpdateParameters(TModel model)
        {
            DynamicParameters parameters = new DynamicParameters();

            foreach (TableColumnMapping column in TableMapping.Columns)
            {
                parameters.Add(column.Property.Name, column.GetValue(model));
            }

            return parameters;
        }

        private string GetDeleteStatement()
        {
            return $"delete from {TableMapping.Name} where id = @Id";
        }

        private string GetInsertStatement()
        {
            return $@"
                insert into {TableMapping.Name} ({string.Join(",", TableMapping.Columns.Select(x => $"{x.Name}"))})
                values ({string.Join(",", TableMapping.Columns.Select(x => $"@{x.Property.Name}"))})";
        }

        private DynamicParameters GetDynamicInsertParameters(TModel model)
        {
            DynamicParameters parameters = new DynamicParameters();

            foreach (TableColumnMapping column in TableMapping.Columns)
            {
                parameters.Add(column.Property.Name, column.GetValue(model));
            }

            return parameters;
        }

        private List<Tuple<TableColumnMapping, string, object>> GetSelectFilters(IQueryCollection queryCollection)
        {
            List<Tuple<TableColumnMapping, string, object>> filters = 
                new List<Tuple<TableColumnMapping, string, object>>();

            Regex filterRegex = new Regex(@"(.*?)\[(.*?)\]", RegexOptions.IgnoreCase);

            foreach (KeyValuePair<string, StringValues> query in queryCollection)
            {
                if (filterRegex.IsMatch(query.Key))
                {
                    Match filterMatch = filterRegex.Match(query.Key);

                    GroupCollection filterGroups = filterMatch.Groups;

                    TableColumnMapping column =
                        TableMapping.Columns.FirstOrDefault(x =>
                            x.Name.Equals(filterGroups[1].Value, StringComparison.InvariantCultureIgnoreCase) ||
                            x.Property.Name.Equals(filterGroups[1].Value, StringComparison.InvariantCultureIgnoreCase));

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

                    Type propertyType = Nullable.GetUnderlyingType(column.Property.PropertyType) ?? column.Property.PropertyType;

                    if (propertyType == typeof(DateTime))
                    {
                        castedValue = DateTime.Parse(query.Value.ToString());
                    }
                    else if (propertyType == typeof(int))
                    {
                        castedValue = int.Parse(query.Value.ToString());
                    }
                    else
                    {
                        castedValue = Convert.ChangeType(
                            query.Value.ToString(),
                            column.Property.PropertyType);
                    }

                    filters.Add(new Tuple<TableColumnMapping, string, object>(
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
                int? id = await _dbService.QuerySingleAsync<int>(
                    "select last_insert_id() from dual", null, transaction);

                await _dbService.QuerySingleAsync<TModel>(@$"
                    select *
                    from {TableMapping.Name}
                    where {TableMapping.KeyColumn.Name} = @id", new { id }, transaction);
            }
            else if (typeof(TDbConnection) == typeof(SqliteConnection))
            {
                return await _dbService.QuerySingleAsync<TModel>(@$"
                    SELECT *
                    FROM {TableMapping.Name}
                    WHERE ROWID = LAST_INSERT_ROWID()", null, transaction);
            }

            return default;
        }

        public virtual Task InitializeAsync()
        {
            if (TableMapping.KeyColumn == null)
            {
                // TODO: Better exception
                throw new Exception("Key column doesn't exist.");
            }

            return Task.CompletedTask;
        }
    }
}
