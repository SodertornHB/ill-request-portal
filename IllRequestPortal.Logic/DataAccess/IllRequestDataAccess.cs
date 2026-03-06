
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using IllRequestPortal.Logic.Model;
using IllRequestPortal.Logic.DataAccess;

namespace IllRequestPortal.Logic.DataAccess
{
    public interface IIllRequestDataAccess : IDataAccess<IllRequest>
    {    }

    public class IllRequestDataAccess : BaseDataAccess<IllRequest>, IIllRequestDataAccess
    {
        public IllRequestDataAccess(ISqlDataAccess db, SqlStringBuilder<IllRequest> sqlStringBuilder)
            : base(db, sqlStringBuilder)
        { }
     }
} 