
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using IllRequestPortal.Logic.DataAccess;
using IllRequestPortal.Logic.Model;
using Microsoft.Extensions.Logging;

namespace IllRequestPortal.Logic.Services
{
    public partial interface IMigrationService : IService<Migration>
    {
    }

    public partial class MigrationService : Service<Migration>, IMigrationService
    {
        public MigrationService(ILogger<MigrationService> logger,
           IMigrationDataAccess dataAccess)
           : base(logger, dataAccess)
        { }
    }
}
