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
        Task<IllRequest> MarkAsRegisteredInLibris(int id);
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
            model.Status = IllRequestConstants.Statuses.Created;
            model.CreatedOn = System.DateTime.UtcNow;
            return await base.Insert(model);
        }

        public async Task<IllRequest> MarkAsRegisteredInLibris(int id)
        {
            var request = await base.Get(id);

            if (request == null) return null;

            request.Status = IllRequestConstants.Statuses.RegisteredInLibris;
            request.UpdatedOn = DateTime.UtcNow;

            await Update(request);

            return request;
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
