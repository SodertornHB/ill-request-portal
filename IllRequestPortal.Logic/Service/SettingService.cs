using IllRequestPortal.Logic.DataAccess;
using IllRequestPortal.Logic.Model;
using Logic.Model;
using Microsoft.Extensions.Logging;

namespace IllRequestPortal.Logic.Services
{
    public partial interface ISettingService : IService<Setting>
    {
    }

    public partial class SettingService : Service<Setting>, ISettingService
    {
        public SettingService(ILogger<SettingService> logger,
           ISettingDataAccess dataAccess)
           : base(logger, dataAccess)
        { }
    }
}
