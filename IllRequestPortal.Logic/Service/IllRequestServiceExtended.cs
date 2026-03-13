using IllRequestPortal.Logic.DataAccess;
using IllRequestPortal.Logic.Model;
using Logic.Model;
using Microsoft.Extensions.Logging;
using System;
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
