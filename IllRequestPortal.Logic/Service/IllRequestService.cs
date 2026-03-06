
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using IllRequestPortal.Logic.DataAccess;
using IllRequestPortal.Logic.Model;
using Microsoft.Extensions.Logging;

namespace IllRequestPortal.Logic.Services
{
    public partial interface IIllRequestService : IService<IllRequest>
    {
    }

    public partial class IllRequestService : Service<IllRequest>, IIllRequestService
    {
        public IllRequestService(ILogger<IllRequestService> logger,
           IIllRequestDataAccess dataAccess)
           : base(logger, dataAccess)
        { }
    }
}
