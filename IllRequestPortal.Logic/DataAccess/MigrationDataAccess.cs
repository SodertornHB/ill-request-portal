
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using IllRequestPortal.Logic.Model;
using IllRequestPortal.Logic.DataAccess;

namespace IllRequestPortal.Logic.DataAccess
{
    public interface IMigrationDataAccess : IDataAccess<Migration>
    {    }

    public class MigrationDataAccess : BaseDataAccess<Migration>, IMigrationDataAccess
    {
        public MigrationDataAccess(ISqlDataAccess db, SqlStringBuilder<Migration> sqlStringBuilder)
            : base(db, sqlStringBuilder)
        { }
     }
} 