using IllRequestPortal.Logic.DataAccess;
using IllRequestPortal.Logic.Model;
using Logic.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IllRequestPortal.Logic.Services
{
    public interface IIllRequestServiceExtended : IIllRequestService
    {
        Task<IllRequest> UpdateStatus(int id, string status);
    }

    public partial class IllRequestServiceExtended : IllRequestService, IIllRequestServiceExtended
    {
        public IllRequestServiceExtended(ILogger<IllRequestService> logger,
           IIllRequestDataAccess dataAccess)
           : base(logger, dataAccess)
        { }

        public override async Task<IllRequest> Insert(IllRequest model)
        {
            model.Status = "Created";
            model.CreatedOn = System.DateTime.UtcNow;
            return await base.Insert(model);
        }

        public override async Task Update(IllRequest model)
        {
            model.UpdatedOn = DateTime.UtcNow;
            await base.Update(model);
        }

        public override async Task Delete(int id)
        {
            var item = await Get(id);
            item.DeletedOn = DateTime.UtcNow;
            await Update(item);
        }

        public override async Task<IEnumerable<IllRequest>> GetAll()
        {
            var all = await base.GetAll();
            return all.Where(x => x.DeletedOn == null);
        }

        public async Task<IllRequest> UpdateStatus(int id, string status)
        {
            var item = await Get(id);

            item.Status = status;

            item.UpdatedOn = DateTime.UtcNow;

            await Update(item);
            
            return item;
        }
    }
}
