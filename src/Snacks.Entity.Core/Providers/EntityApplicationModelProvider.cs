using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Reflection;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Snacks.Entity.Core.Helpers;


namespace Snacks.Entity.Core.Providers
{
    internal class EntityApplicationModelProvider<TDbContext> : IApplicationModelProvider
        where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext; 
        private readonly IModelMetadataProvider _modelMetadataProvider;
        public EntityApplicationModelProvider(
            TDbContext dbContext,
            IModelMetadataProvider modelMetadataProvider)
        {
            _dbContext = dbContext;
            _modelMetadataProvider = modelMetadataProvider;
        }

        // Run after default
        public int Order => -999;

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            foreach (var entityType in _dbContext.Model.GetEntityTypes())
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
            var primaryKeyType = primaryKey?.Properties.Single().ClrType ?? typeof(string);
            var modelType = entityType.ClrType;
            var propertyName = typeof(TDbContext).GetProperties().First(x => x.PropertyType == typeof(DbSet<>).MakeGenericType(modelType)).Name;
            var controllerType = typeof(EntityController<,,>).MakeGenericType(modelType, primaryKeyType, typeof(TDbContext));

            if (context.ControllerTypes.Any(t => t.IsSubclassOf(controllerType)))
            {
                return null;
            }

            var controllerModel = ApplicationModelHelper.CreateControllerModel(
                context.Result, controllerType.GetTypeInfo(), _modelMetadataProvider);
            controllerModel.ControllerName = propertyName;

            return controllerModel;
        }
    }
}