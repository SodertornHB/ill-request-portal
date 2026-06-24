using IllRequestPortal.Logic.Model;
using IllRequestPortal.Logic.DataAccess;
using Logic.Model;

namespace IllRequestPortal.Logic.DataAccess
{
    public interface ISettingDataAccess : IDataAccess<Setting>
    {    }

    public class SettingDataAccess : BaseDataAccess<Setting>, ISettingDataAccess
    {
        public SettingDataAccess(ISqlDataAccess db, SqlStringBuilder<Setting> sqlStringBuilder)
            : base(db, sqlStringBuilder)
        { }
     }
} 