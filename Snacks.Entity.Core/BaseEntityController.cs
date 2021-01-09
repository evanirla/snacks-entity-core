using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Snacks.Entity.Core
{
    public abstract class BaseEntityController<TEntity> : ControllerBase, IEntityController<TEntity>
        where TEntity : class
    {
        private static readonly PropertyInfo[] _entityProperties = typeof(TEntity).GetProperties();
        
        private readonly Regex _filterRegex = new Regex(@"(.*?)\[(.*?)\]", RegexOptions.IgnoreCase);
        protected readonly IEntityService<TEntity> _entityService;

        public BaseEntityController(IEntityService<TEntity> entityService)
        {
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<TEntity>> GetAsync([FromRoute] object id)
        {
            TEntity model = await _entityService.FindAsync(id).ConfigureAwait(false);

            if (model == null)
            {
                return NotFound();
            }

            return model;
        }

        [HttpGet]
        public virtual async Task<ActionResult<IList<TEntity>>> GetAsync()
        {
            if (Request.Query.Count == 0)
            {
                return await _entityService.Entities.ToListAsync();
            }
            else
            {
                int limit = int.MaxValue;
                int offset = 0;

                if (Request.Query.ContainsKey("limit"))
                {
                    int.TryParse(Request.Query["limit"], out limit);
                }

                if (Request.Query.ContainsKey("offset"))
                {
                    int.TryParse(Request.Query["offset"], out offset);
                }

                IQueryable<TEntity> models = _entityService.Entities;

                foreach (Expression<Func<TEntity, bool>> filter in GetFilters())
                {
                    models = models.Where(filter);
                }

                return models.Skip(offset).Take(limit).ToList();
            }
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> DeleteAsync([FromRoute] object id)
        {
            TEntity model = await _entityService.FindAsync(id).ConfigureAwait(false);

            if (model == null)
            {
                return NotFound();
            }

            await _entityService.DeleteAsync(model).ConfigureAwait(false);

            return Ok();
        }

        

        [HttpPost]
        public virtual async Task<ActionResult<TEntity>> PostAsync([FromBody] TEntity model)
        {
            using TDbConnection connection = await DbConnectionFactory.CreateConnectionAsync().ConfigureAwait(false);

            int rowId = await connection.InsertAsync(model).ConfigureAwait(false);

            if (rowId != 0)
            {
                return await connection.GetAsync<TEntity>(rowId).ConfigureAwait(false);
            }

            return default(TEntity);
        }

        [HttpPatch("{id}")]
        public virtual async Task<IActionResult> PatchAsync([FromRoute] object id, [FromBody] object data)
        {
            using TDbConnection connection = await DbConnectionFactory.CreateConnectionAsync().ConfigureAwait(false);

            TEntity existingModel = await connection.GetAsync<TEntity>(id).ConfigureAwait(false);

            if (existingModel == null)
            {
                return NotFound();
            }

            foreach (PropertyInfo property in data.GetType().GetProperties())
            {
                PropertyInfo modelProperty = ModelProperties.FirstOrDefault(
                    x => x.Name.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase));

                if (modelProperty != null)
                {
                    modelProperty.SetValue(existingModel, property.GetValue(data));
                }
            }

            if (await connection.UpdateAsync(existingModel))
            {
                return Ok();
            }

            return BadRequest();
        }

        private List<Expression<Func<TEntity, bool>>> GetFilters()
        {
            List<Expression<Func<TEntity, bool>>> expressions = new List<Expression<Func<TEntity, bool>>>();

            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");

            foreach (var param in Request.Query)
            {
                PropertyInfo property = null;
                string @operator = "=";

                if (_filterRegex.IsMatch(param.Key))
                {
                    Match filterMatch = _filterRegex.Match(param.Key);
                    GroupCollection filterGroups = filterMatch.Groups;

                    property = _entityProperties.FirstOrDefault(x => x.Name.Equals(filterGroups[1].Value, StringComparison.InvariantCultureIgnoreCase));
                }
                else
                {
                    property = _entityProperties.FirstOrDefault(x => x.Name.Equals(param.Key, StringComparison.InvariantCultureIgnoreCase));
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

                    expressions.Add(Expression.Lambda<Func<TEntity, bool>>(equality, new[] { parameter }));
                }
            }
        }
    }
}
