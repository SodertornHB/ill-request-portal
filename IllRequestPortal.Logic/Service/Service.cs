
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using IllRequestPortal.Logic.Model;
using IllRequestPortal.Logic.DataAccess;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using System;

namespace IllRequestPortal.Logic.Services
{
    public partial interface IService<TModel>
    {  
        Task<TModel> Get(int id);
        Task<IEnumerable<TModel>> Get(Dictionary<string, string> filters);
        Task<IEnumerable<TModel>> GetSince(int id);
        Task<bool> Exists(int id);
        Task<IEnumerable<TModel>> GetAll();
        Task<TModel> Insert(TModel model);
        Task Update(TModel model);
        Task Delete(int id);
    }

    public partial class Service<TModel> : IService<TModel> where TModel : Entity
    {
        protected readonly ILogger<Service<TModel>> logger;
        protected readonly IDataAccess<TModel> dataAccess;

        public Service(ILogger<Service<TModel>> logger,
            IDataAccess<TModel> dataAccess)
        {
            this.logger = logger;
            this.dataAccess = dataAccess;
        }        

        public virtual async Task<TModel> Get(int id)
        {
            logger.LogInformation($"Fetching entity with id {id} from data source.");
            return await dataAccess.Get(id);
        }

        public async Task<IEnumerable<TModel>> Get(Dictionary<string, string> filters)
        {
            var all = await GetAll();
            var filtered = all.AsQueryable();

            foreach (var filter in filters)
            {
                var propertyInfo = typeof(TModel).GetProperty(filter.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo != null)
                {
                    filtered = filtered.Where(x =>
                        (propertyInfo.GetValue(x) == null && filter.Value == null) ||
                        (propertyInfo.GetValue(x) != null && propertyInfo.GetValue(x).ToString().ToLowerInvariant() == filter.Value.ToLowerInvariant())
                    );
                }
                else
                {
                    throw new ArgumentException($"Invalid filter parameter: {filter.Key}");
                }
            }

            return filtered;
        }

        public async Task<IEnumerable<TModel>> GetSince(int id) => await dataAccess.GetSince(id);

        public virtual async Task<bool> Exists(int id)
        {
            var item = await Get(id);
            return item != null;
        }

        public virtual async Task<IEnumerable<TModel>> GetAll()
        {
            logger.LogInformation($"Fetching all entities from data source.");
            return await dataAccess.GetAll();
        }
        
        public virtual async Task<TModel> Insert(TModel model)
        {
            logger.LogInformation($"Saving entity with id {model.Id} to data source.");
            return await dataAccess.Insert(model);
        }

        public virtual async Task Update(TModel model)
        {
            logger.LogInformation($"Update entity with id {model.Id} in data source.");
            await dataAccess.Update(model);
        }

        public virtual async Task Delete(int id)
        {
            logger.LogInformation($"Deleting entity with id {id} from data source.");
            await dataAccess.Delete(id);
        }
    }
}