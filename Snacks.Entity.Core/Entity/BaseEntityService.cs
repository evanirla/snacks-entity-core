using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Snacks.Entity.Core.Caching;
using Snacks.Entity.Core.Database;
using Snacks.Entity.Core.Exceptions;
using System;
using System.Collections.Generic;
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
    /// <typeparam name="TDbService"></typeparam>
    public abstract class BaseEntityService<TModel, TDbService> : 
        IEntityService<TModel, TDbService>
        where TModel : IEntityModel
        where TDbService : IDbService
    {
        private TDbService _dbService;
        protected TDbService DbService
        {
            get
            {
                if (_dbService != null)
                {
                    return _dbService;
                }

                Type connectionType = typeof(TDbService).BaseType.GetGenericArguments().First();

                _dbService = (TDbService)_serviceProvider.GetRequiredService(typeof(IDbService<>).MakeGenericType(connectionType));

                return _dbService;
            }
        }

        private IEntityCacheService<TModel> _cacheService;
        protected IEntityCacheService<TModel> CacheService
        {
            get
            {
                if (_cacheService != null)
                {
                    return _cacheService;
                }

                _cacheService = _serviceProvider.GetRequiredService<IEntityCacheService<TModel>>();

                return _cacheService;
            }
        }
        protected readonly IServiceProvider _serviceProvider;

        public TableMapping Mapping { get; private set; }

        protected readonly string _insertStatement;

        public BaseEntityService(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Mapping = TableMapping.GetMapping<TModel>();

            _insertStatement = GetInsertStatement();
        }

        public virtual Task InitializeAsync()
        {
            if (Mapping.KeyColumn == null)
            {
                throw new NoKeyColumnException($"No key column for {typeof(TModel).Name}");
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

            if (CacheService != null)
            {
                await CacheService.RemoveManyAsync();
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

            if (model.IdempotencyKey != null && CacheService != null)
            {
                TModel idempotentModel = await CacheService.GetCustomOneAsync(
                    $"{typeof(TModel).Name}(Idempotency={model.IdempotencyKey})");

                if (idempotentModel != null)
                {
                    throw new IdempotencyKeyUsedException();
                }
            }

            async Task createOne()
            {
                await DbService.ExecuteSqlAsync(
                    _insertStatement,
                    GetInsertParameters(model),
                    transaction);

                if (Mapping.KeyColumn.GetValue(model).Equals(Mapping.KeyColumn.GetDefaultValue()))
                {
                    model = await GetLastInsertedAsync(transaction);
                }
                else
                {
                    model = await GetOneAsync(model.Key, transaction);
                }
            }

            if (transaction != null)
            {
                await createOne();
            }
            else
            {
                using var conn = await DbService.GetConnectionAsync();
                transaction = conn.BeginTransaction();
                await createOne();
                transaction.Commit();
            }

            if (model.IdempotencyKey != null && CacheService != null)
            {
                await CacheService.SetCustomOneAsync(
                    $"{typeof(TModel).Name}(Idempotency={model.IdempotencyKey})", model);
            }

            if (CacheService != null)
            {
                await CacheService.SetOneAsync(model);
                await CacheService.RemoveManyAsync();
            }

            return model;
        }

        public async Task DeleteOneAsync(TModel model, IDbTransaction transaction = null)
        {
            await DeleteOneAsync(model.Key, transaction);
        }

        public virtual async Task DeleteOneAsync(object key, IDbTransaction transaction = null)
        {
            if (key == null || key.Equals(Mapping.KeyColumn.GetDefaultValue()))
            {
                throw new KeyValueInvalidException();
            }

            string statement = GetDeleteStatement();
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add(Mapping.KeyColumn.Name, key);
            await DbService.ExecuteSqlAsync(statement, parameters);

            if (CacheService != null)
            {
                await CacheService.RemoveOneAsync(key);
                await CacheService.RemoveManyAsync();
            }
        }

        public virtual async Task<IEnumerable<TModel>> GetManyAsync(IQueryCollection queryCollection = null, IDbTransaction transaction = null)
        {
            if (CacheService != null)
            {
                IList<TModel> cachedModels = await CacheService.GetManyAsync(queryCollection);

                if (cachedModels != null)
                {
                    return cachedModels;
                }
            }

            string statement = GetSelectStatement(queryCollection);
            DynamicParameters parameters = GetDynamicQueryParameters(queryCollection);

            IList<TModel> models = (await DbService.QueryAsync<TModel>(statement, parameters, transaction)).ToList();

            if (CacheService != null)
            {
                await CacheService.SetManyAsync(queryCollection, models);
            }

            return models;
        }

        public virtual async Task<TModel> GetOneAsync(object key, IDbTransaction transaction = null)
        {
            if (CacheService != null)
            {
                TModel cachedModel = await CacheService.GetOneAsync(key);

                if (cachedModel != null)
                {
                    return cachedModel;
                }
            }

            string statement = GetSelectByKeyStatement();

            TModel model = await DbService.QuerySingleAsync<TModel>(statement, new { Key = key }, transaction);

            if (model != null)
            {
                if (CacheService != null)
                {
                    await CacheService.SetOneAsync(model);
                }
            }

            return model;
        }

        public async Task<IEnumerable<TModel>> GetManyAsync(string sql, object parameters, IDbTransaction transaction = null)
        {
            return await DbService.QueryAsync<TModel>(sql, parameters, transaction);
        }

        public async Task<IEntityModel> CreateOneAsync(IEntityModel model, IDbTransaction transaction = null)
        {
            return await CreateOneAsync((TModel)model, transaction);
        }

        public async Task<IEnumerable<IEntityModel>> CreateManyAsync(IEnumerable<IEntityModel> models, IDbTransaction transaction = null)
        {
            return (await CreateManyAsync(models.Select(x => (TModel)x).ToList(), transaction)).Select(x => (IEntityModel)x);
        }

        public async Task<IEntityModel> UpdateOneAsync(IEntityModel model, object data, IDbTransaction transaction = null)
        {
            return await UpdateOneAsync((TModel)model, data, transaction);
        }

        public virtual async Task<TModel> UpdateOneAsync(TModel model, object data, IDbTransaction transaction = null)
        {
            if (Mapping.KeyColumn.GetValue(model) == null)
            {
                throw new ArgumentException("Model must contain a value for Id.");
            }

            string statement = GetUpdateStatement(data);
            DynamicParameters parameters = GetDynamicUpdateParameters(model, data);

            await DbService.ExecuteSqlAsync(
                statement,
                parameters,
                transaction);

            if (CacheService != null)
            {
                await CacheService.RemoveOneAsync(model);
                await CacheService.RemoveManyAsync();
            }

            model = await GetOneAsync(Mapping.KeyColumn.GetValue(model), transaction);

            if (CacheService != null)
            {
                await CacheService.SetOneAsync(model);
            }

            if (CacheService != null)
            {
                await CacheService.RemoveManyAsync();
            }

            return model;
        }

        public async Task DeleteOneAsync(IEntityModel model, IDbTransaction transaction = null)
        {
            await DeleteOneAsync((TModel)model, transaction);
        }

        protected virtual async Task<TModel> GetLastInsertedAsync(IDbTransaction transaction)
        {
            int key = await DbService.GetLastInsertId(transaction);
            return await GetOneAsync(key, transaction);
        }

        protected IEntityService<TOtherModel> GetOtherEntityService<TOtherModel>() where TOtherModel : IEntityModel
        {
            return _serviceProvider.GetRequiredService<IEntityService<TOtherModel>>();
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

            if (queryCollection != null)
            {
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

        private string GetUpdateStatement(object data)
        {
            List<TableColumnMapping> columns = new List<TableColumnMapping>();

            foreach (PropertyInfo property in data.GetType().GetProperties())
            {
                TableColumnMapping column = Mapping.Columns
                    .FirstOrDefault(x => x.Property.Name.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase));

                if (column == null)
                {
                    // TODO: Custom exception?
                    throw new Exception($"{typeof(TModel).Name} does not contain property {property.Name}");
                }

                if (column.IsKey)
                {
                    // TODO: Custom exception?
                    throw new Exception($"Cannot update {typeof(TModel).Name} key.");
                }

                if (column.IsDatabaseGenerated && column.DatabaseGeneratedAttribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity)
                {
                    // TODO: Custom exception?
                    throw new Exception($"Cannot update {column.Property.Name} as it is read-only.");
                }

                columns.Add(column);
            }

            return $@"
                update {Mapping.Name}
                set {string.Join(",", columns.Select(x => $"{x.Name} = @{x.Property.Name}"))}
                where {Mapping.KeyColumn.Name} = @{Mapping.KeyColumn.Property.Name}";
        }

        private DynamicParameters GetDynamicUpdateParameters(TModel model, object data)
        {
            DynamicParameters parameters = new DynamicParameters();

            foreach (PropertyInfo property in data.GetType().GetProperties())
            {
                TableColumnMapping column = Mapping.Columns
                    .FirstOrDefault(x => x.Property.Name.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase));

                if (column != null)
                {
                    parameters.Add(column.Property.Name, property.GetValue(data));
                }
            }

            parameters.Add(Mapping.KeyColumn.Property.Name, model.Key);

            return parameters;
        }

        private string GetDeleteStatement()
        {
            return $"delete from {Mapping.Name} where {Mapping.KeyColumn.Name} = @{Mapping.KeyColumn.Property.Name}";
        }

        private IEnumerable<TableColumnMapping> GetInsertColumns()
        {
            // We want to get the default values of "DatabaseGenerated" columns.
            TModel referenceModel = Activator.CreateInstance<TModel>();

            List<TableColumnMapping> insertableColumns = new List<TableColumnMapping>();

            foreach (TableColumnMapping column in Mapping.Columns)
            {
                if (column.IsDatabaseGenerated)
                {
                    if (column.IsKey || column.GetValue(referenceModel).Equals(column.GetDefaultValue()))
                    {
                        continue;
                    }
                }

                insertableColumns.Add(column);
            }

            return insertableColumns;
        }

        private string GetInsertStatement()
        {
            var insertColumns = GetInsertColumns();

            return $@"
                insert into {Mapping.Name} ({string.Join(",", insertColumns.Select(x => $"{x.Name}"))})
                values ({string.Join(",", insertColumns.Select(x => $"@{x.Property.Name}"))})";
        }

        private DynamicParameters GetInsertParameters(TModel model)
        {
            // We want to get the default values of "DatabaseGenerated" columns.
            TModel referenceModel = Activator.CreateInstance<TModel>();

            DynamicParameters parameters = new DynamicParameters();

            var insertColumns = GetInsertColumns();

            foreach (TableColumnMapping column in insertColumns)
            {
                if (column.IsDatabaseGenerated)
                {
                    parameters.Add(column.Property.Name, column.GetValue(referenceModel));
                }
                else
                {
                    parameters.Add(column.Property.Name, column.GetValue(model));
                }
            }

            return parameters;
        }

        private List<Tuple<TableColumnMapping, string, object>> GetSelectFilters(IQueryCollection queryCollection)
        {
            List<Tuple<TableColumnMapping, string, object>> filters = 
                new List<Tuple<TableColumnMapping, string, object>>();

            if (queryCollection == null)
            {
                return filters;
            }

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

        async Task<IEntityModel> IEntityService.GetOneAsync(object key, IDbTransaction transaction)
        {
            return await GetOneAsync(key, transaction);
        }

        async Task<IEnumerable<IEntityModel>> IEntityService.GetManyAsync(IQueryCollection queryCollection, IDbTransaction transaction)
        {
            return (await GetManyAsync(queryCollection, transaction)).Select(x => (IEntityModel)x);
        }

        async Task<IEnumerable<IEntityModel>> IEntityService.GetManyAsync(string sql, object parameters, IDbTransaction transaction)
        {
            return (await GetManyAsync(sql, parameters, transaction)).Select(x => (IEntityModel)x);
        }
    }
}
