using IllRequestPortal.Logic.DataAccess;
using IllRequestPortal.Logic.Model;
using Logic.Model;
using Logic.Util;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace IllRequestPortal.Logic.Services
{

    public partial class IllRequestServiceExtended : IllRequestService
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
    }
}
