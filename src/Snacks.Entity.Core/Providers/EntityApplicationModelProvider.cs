using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Snacks.Entity.Core.Helpers;

namespace Snacks.Entity.Core.Providers
{
    internal class EntityApplicationModelProvider<TDbContext> : IApplicationModelProvider
        where TDbContext : DbContext
    {
        private readonly IDbContextFactory<TDbContext> _dbContextFactory;
        private readonly IModelMetadataProvider _modelMetadataProvider;
        public EntityApplicationModelProvider(
            IDbContextFactory<TDbContext> dbContextFactory,
            IModelMetadataProvider modelMetadataProvider)
        {
            _dbContextFactory = dbContextFactory;
            _modelMetadataProvider = modelMetadataProvider;
        }

        // Run after default
        public int Order => -999;

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            foreach (var entityType in dbContext.Model.GetEntityTypes())
            {
                var controller = CreateController(context, entityType);

                if (controller != null)
                {
                    context.Result.Controllers.Add(controller);
                }
            }
        }

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            return;
        }

        internal ControllerModel CreateController(ApplicationModelProviderContext context, IEntityType entityType)
        {
            var primaryKey = entityType.FindPrimaryKey();
            var primaryKeyProperty = primaryKey?.Properties.SingleOrDefault();

            Type primaryKeyType = null;

            if (primaryKeyProperty == default || primaryKeyProperty.ClrType == null)
            {
                primaryKeyType = typeof(string);
            }
            else
            {
                primaryKeyType = primaryKeyProperty.ClrType;
            }

            var modelType = entityType.ClrType;
            var property = typeof(TDbContext).GetProperties()
                .SingleOrDefault(x => x.PropertyType == typeof(DbSet<>).MakeGenericType(modelType));

            if (property == default)
            {
                return null;
            }

            var controllerType = typeof(EntityController<,,>).MakeGenericType(modelType, primaryKeyType, typeof(TDbContext));

            if (context.ControllerTypes.Any(t => t.IsSubclassOf(controllerType)))
            {
                return null;
            }

            var controllerModel = ApplicationModelHelper.CreateControllerModel(
                context.Result, controllerType.GetTypeInfo(), _modelMetadataProvider);
            
            controllerModel.ControllerName = property.Name;

            return controllerModel;
        }
    }
}