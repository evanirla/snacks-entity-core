using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Snacks.Entity.Core.Caching;
using Snacks.Entity.Core.Database;
using Snacks.Entity.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
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
    public abstract class BaseEntityService<TModel, TDbService> : 
        IEntityService<TModel, TDbService>
        where TModel : IEntityModel
        where TDbService : IDbService
    {
        protected readonly TDbService _dbService;
        protected readonly IEntityCacheService<TModel> _cacheService;
        protected readonly IServiceProvider _serviceProvider;

        public TableMapping Mapping { get; private set; }

        public BaseEntityService(
            IServiceProvider serviceProvider)
        {
            _dbService = (TDbService)serviceProvider.GetService(typeof(TDbService));
            _cacheService = (IEntityCacheService<TModel>)serviceProvider.GetService(typeof(IEntityCacheService<TModel>));
            _serviceProvider = serviceProvider;

            Mapping = TableMapping.GetMapping<TModel>();
        }

        public virtual Task InitializeAsync()
        {
            if (Mapping.KeyColumn == null)
            {
                // TODO: Better exception
                throw new Exception("Key column doesn't exist.");
            }

            return Task.CompletedTask;
        }

        public virtual async Task<IEnumerable<TModel>> CreateManyAsync(IEnumerable<TModel> models, IDbTransaction transaction = null)
        {
            List<TModel> newModels = new List<TModel>();

            foreach (TModel model in models)
            {
                newModels.Add(await CreateOneAsync(model, transaction));
            }

            if (_cacheService != null)
            {
                await _cacheService.RemoveManyAsync();
            }

            return newModels;
        }

        public virtual async Task<TModel> CreateOneAsync(TModel model, IDbTransaction transaction = null)
        {
            if (!Mapping.KeyColumn.IsDatabaseGenerated &&
                (Mapping.KeyColumn.GetValue(model) == null ||
                Mapping.KeyColumn.GetValue(model).Equals(Mapping.KeyColumn.GetDefaultValue())))
            {
                throw new KeyValueInvalidException();
            }

            if (model.IdempotencyKey != null && _cacheService != null)
            {
                TModel idempotentModel = await _cacheService.GetCustomOneAsync(
                    $"{typeof(TModel).Name}(Idempotency={model.IdempotencyKey})");

                if (idempotentModel != null)
                {
                    throw new IdempotencyKeyUsedException();
                }
            }

            string statement = GetInsertStatement();

            async Task createOne()
            {
                await _dbService.ExecuteSqlAsync(
                    statement,
                    GetDynamicInsertParameters(model),
                    transaction);

                model = await GetLastInsertedAsync(transaction);
            }

            if (transaction != null)
            {
                await createOne();
            }
            else
            {
                using var conn = await _dbService.GetConnectionAsync();
                transaction = conn.BeginTransaction();
                await createOne();
                transaction.Commit();
            }

            if (model.IdempotencyKey != null && _cacheService != null)
            {
                await _cacheService.SetCustomOneAsync(
                    $"{typeof(TModel).Name}(Idempotency={model.IdempotencyKey})", model);
            }

            if (_cacheService != null)
            {
                await _cacheService.SetOneAsync(model);
                await _cacheService.RemoveManyAsync();
            }

            return model;
        }

        public virtual async Task DeleteOneAsync(TModel model, IDbTransaction transaction = null)
        {
            string statement = GetDeleteStatement();

            await _dbService.ExecuteSqlAsync(statement, new { Key = Mapping.KeyColumn.GetValue(model) });

            if (_cacheService != null)
            {
                await _cacheService.RemoveOneAsync(model);
                await _cacheService.RemoveManyAsync();
            }
        }

        public virtual async Task DeleteOneAsync(object key, IDbTransaction transaction = null)
        {
            if (key == null || key.Equals(Mapping.KeyColumn.GetDefaultValue()))
            {
                throw new KeyValueInvalidException();
            }

            string statement = GetDeleteStatement();
            await _dbService.ExecuteSqlAsync(statement, new { Key = key });

            if (_cacheService != null)
            {
                await _cacheService.RemoveOneAsync(key);
                await _cacheService.RemoveManyAsync();
            }
        }

        public virtual async Task<IEnumerable<TModel>> GetManyAsync(IQueryCollection queryCollection, IDbTransaction transaction = null)
        {
            if (_cacheService != null)
            {
                IList<TModel> cachedModels = await _cacheService.GetManyAsync(queryCollection);

                if (cachedModels != null)
                {
                    return cachedModels;
                }
            }

            string statement = GetSelectStatement(queryCollection);
            DynamicParameters parameters = GetDynamicQueryParameters(queryCollection);

            IList<TModel> models = (await _dbService.QueryAsync<TModel>(statement, parameters, transaction)).ToList();

            if (_cacheService != null)
            {
                await _cacheService.SetManyAsync(queryCollection, models);
            }

            return models;
        }

        public virtual async Task<TModel> GetOneAsync(object key, IDbTransaction transaction = null)
        {
            if (_cacheService != null)
            {
                TModel cachedModel = await _cacheService.GetOneAsync(key);

                if (cachedModel != null)
                {
                    return cachedModel;
                }
            }

            string statement = GetSelectByKeyStatement();

            TModel model = await _dbService.QuerySingleAsync<TModel>(statement, new { Key = key }, transaction);

            if (model != null)
            {
                if (_cacheService != null)
                {
                    await _cacheService.SetOneAsync(model);
                }
            }

            return model;
        }

        public async Task<IEnumerable<TModel>> GetManyAsync(string sql, object parameters, IDbTransaction transaction = null)
        {
            string cacheKey = parameters != null ? sql :
                $"{sql}({parameters.GetType().GetProperties().Select(x => $"{x.Name}={x.GetValue(parameters)}")})";

            if (_cacheService != null)
            {
                IList<TModel> cachedModels = await _cacheService.GetCustomManyAsync(cacheKey);

                if (cachedModels != default)
                {
                    return cachedModels;
                }
            }

            IList<TModel> models = (await _dbService.QueryAsync<TModel>(sql, parameters, transaction)).ToList();

            if (_cacheService != null)
            {
                await _cacheService.SetCustomManyAsync(cacheKey, models);
            }

            return models;
        }

        public async Task<IEntityModel> CreateOneAsync(IEntityModel model, IDbTransaction transaction = null)
        {
            return await CreateOneAsync((TModel)model, transaction);
        }

        public async Task<IEnumerable<IEntityModel>> CreateManyAsync(IEnumerable<IEntityModel> models, IDbTransaction transaction = null)
        {
            return (await CreateManyAsync(models.Select(x => (TModel)x).ToList(), transaction)).Select(x => (IEntityModel)x);
        }

        public async Task UpdateOneAsync(IEntityModel model, IDbTransaction transaction = null)
        {
            await UpdateOneAsync((TModel)model, transaction);
        }

        public virtual async Task UpdateOneAsync(TModel model, IDbTransaction transaction = null)
        {
            if (Mapping.KeyColumn.GetValue(model) == null)
            {
                throw new ArgumentException("Model must contain a value for Id.");
            }

            string statement = GetUpdateStatement();
            DynamicParameters parameters = GetDynamicUpdateParameters(model);

            await _dbService.ExecuteSqlAsync(
                statement,
                parameters,
                transaction);

            if (_cacheService != null)
            {
                await _cacheService.RemoveOneAsync(model);
                await _cacheService.RemoveManyAsync();
            }

            model = await GetOneAsync(Mapping.KeyColumn.GetValue(model), transaction);

            if (_cacheService != null)
            {
                await _cacheService.SetOneAsync(model);
            }

            if (_cacheService != null)
            {
                await _cacheService.RemoveManyAsync();
            }
        }

        public async Task DeleteOneAsync(IEntityModel model, IDbTransaction transaction = null)
        {
            await DeleteOneAsync((TModel)model, transaction);
        }

        protected virtual async Task<TModel> GetLastInsertedAsync(IDbTransaction transaction)
        {
            Type dbServiceType = typeof(TDbService);
            Type dbConnectionType = dbServiceType.GetGenericArguments().First();

            if (dbConnectionType.Name == "MySqlConnection")
            {
                int key = await _dbService.QuerySingleAsync<int>(
                    "select last_insert_id() from dual", null, transaction);

                return await GetOneAsync(key, transaction);
            }
            else if (dbConnectionType.Name == "SqliteConnection")
            {
                dynamic key = await _dbService.QuerySingleAsync<dynamic>(@$"
                    SELECT {Mapping.KeyColumn.Name}
                    FROM {Mapping.Name}
                    WHERE ROWID = LAST_INSERT_ROWID()", null, transaction);

                return await GetOneAsync(key, transaction);
            }

            return default;
        }

        protected IEntityService<TOtherModel> GetOtherEntityService<TOtherModel>() where TOtherModel : IEntityModel
        {
            return (IEntityService<TOtherModel>)_serviceProvider.GetService(typeof(IEntityService<TOtherModel>));
        }

        private string GetSelectByKeyStatement()
        {
            return $@"
                select {string.Join(',', Mapping.Columns.Select(x => $"{x.Name} `{x.Property.Name}`"))}
                from {Mapping.Name}
                where {Mapping.KeyColumn.Name} = @Key";
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
                    Mapping.Columns.FirstOrDefault(x =>
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
                    Mapping.Columns.FirstOrDefault(x =>
                        x.Name.Equals(s, StringComparison.InvariantCultureIgnoreCase) ||
                        x.Property.Name.Equals(s, StringComparison.InvariantCultureIgnoreCase));

                if (orderByColumn != null)
                {
                    orderByColumns.Add(new Tuple<TableColumnMapping, string>(
                        orderByColumn, "desc"));
                }
            }

            return $@"
                select {string.Join(',', Mapping.Columns.Select(x => $"{x.Name} `{x.Property.Name}`"))}
                from {Mapping.Name}
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
            IEnumerable<TableColumnMapping> columns = Mapping.Columns
                .Where(x => x.IsKey)
                .Where(x => 
                    !x.IsDatabaseGenerated ||
                    x.DatabaseGeneratedAttribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed);

            return $@"
                update {Mapping.Name}
                set {string.Join(",", columns.Select(x => $"{x.Name} = @{x.Property.Name}"))}
                where key = @Key";
        }

        private DynamicParameters GetDynamicUpdateParameters(TModel model)
        {
            DynamicParameters parameters = new DynamicParameters();

            foreach (TableColumnMapping column in Mapping.Columns)
            {
                parameters.Add(column.Property.Name, column.GetValue(model));
            }

            return parameters;
        }

        private string GetDeleteStatement()
        {
            return $"delete from {Mapping.Name} where id = @Id";
        }

        private IEnumerable<TableColumnMapping> GetInsertColumns()
        {
            return Mapping.Columns
                .Where(x => !x.IsKey || !x.IsDatabaseGenerated);
        }

        private string GetInsertStatement()
        {
            var insertColumns = GetInsertColumns();

            return $@"
                insert into {Mapping.Name} ({string.Join(",", insertColumns.Select(x => $"{x.Name}"))})
                values ({string.Join(",", insertColumns.Select(x => $"@{x.Property.Name}"))})";
        }

        private DynamicParameters GetDynamicInsertParameters(TModel model)
        {
            DynamicParameters parameters = new DynamicParameters();

            var insertColumns = GetInsertColumns();

            foreach (TableColumnMapping column in insertColumns)
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
                        Mapping.Columns.FirstOrDefault(x =>
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
                else
                {
                    TableColumnMapping column =
                        Mapping.Columns.FirstOrDefault(x =>
                            x.Name.Equals(query.Key, StringComparison.InvariantCultureIgnoreCase) ||
                            x.Property.Name.Equals(query.Key, StringComparison.InvariantCultureIgnoreCase));
                }
            }

            return filters;
        }

        Task<IEntityModel> IEntityService.GetOneAsync(object key, IDbTransaction transaction)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<IEntityModel>> IEntityService.GetManyAsync(IQueryCollection queryCollection, IDbTransaction transaction)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<IEntityModel>> IEntityService.GetManyAsync(string sql, object parameters, IDbTransaction transaction)
        {
            throw new NotImplementedException();
        }
    }
}
