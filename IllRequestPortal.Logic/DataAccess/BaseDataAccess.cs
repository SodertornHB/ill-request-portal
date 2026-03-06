
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using IllRequestPortal.Logic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IllRequestPortal.Logic.DataAccess
{
    public partial interface IDataAccess<T>
    {
        Task<T> Get(int id);
        Task<IEnumerable<T>> GetSince(int id);
        Task<IEnumerable<T>> GetAll();
        Task<T> Insert(T model);
        Task Update(T model);
        Task Delete(int id);
        Task DeleteAll();
    }

    public partial class BaseDataAccess<T> : IDataAccess<T> where T : Entity
    {
        protected readonly ISqlDataAccess db;
        private readonly SqlStringBuilder<T> sqlStringBuilder;

        public BaseDataAccess(ISqlDataAccess db,
            SqlStringBuilder<T> sqlStringBuilder)
        {
            this.db = db;
            this.sqlStringBuilder = sqlStringBuilder;
            Table = typeof(T).Name;
        }

        protected string Table { get; }
        
        public async virtual Task<T> Insert(T model)
        {            
            var sql = await HasIdentityColumn() ? sqlStringBuilder.GetInsertString(model,Table) : sqlStringBuilder.GetInsertString(model, await GetNextId(), Table);

            model.Id = await db.InsertData(sql, model);

            return model;
        }

        public async virtual Task Update(T model)
        {
            string sql = sqlStringBuilder.GetUpdateString(model, Table);

            await db.UpdateData(sql, model);
        }

        public virtual async Task<IEnumerable<T>> GetAll()
        {
            string sql = $"SELECT * FROM [{Table}] "; 
            return await ExecuteSelectMany(sql);
        }
        
        public virtual async Task<T> Get(int id)
        {
            string sql = $"SELECT * FROM [{Table}] Where Id = @id";
            return await db.LoadSingularData<T, dynamic>(sql, new { Id = id });
        }
        
        public virtual async Task<IEnumerable<T>> GetSince(int id)
        {
            string sql = $"SELECT * FROM [{Table}] Where Id > @id";
            return await db.LoadData<T, dynamic>(sql, new { Id = id });
        }

        public async Task DeleteAll()
        {
            string sql = $"DELETE FROM [{Table}] ";
            await db.UpdateData(sql, new { });
        }

        public async Task Delete(int id)
        {
            string sql = $"DELETE FROM [{Table}] WHERE Id = @id";
            await db.UpdateData(sql, new { Id = id });
        }

        protected async Task<T> ExecuteSelectSingle(string sql, int id)
        {
            return await db.LoadSingularData<T, dynamic>(sql, new { Id = id });
        }

        protected async Task<T> ExecuteSelectSingle(string sql)
        {
            return await db.LoadSingularData<T, dynamic>(sql, new { });
        }

        protected async Task<List<T>> ExecuteSelectMany(string sql)
        {
            return await db.LoadData<T, dynamic>(sql, new { });
        }

        #region private

        private async Task<bool> HasIdentityColumn()
        {
            var sql = @$"select b.name as IdentityColumn 
                        from sysobjects a inner join syscolumns b on a.id = b.id 
                       where a.name = '{Table}' and    
                            columnproperty(a.id, b.name, 'isIdentity') = 1 and 
                            objectproperty(a.id, 'isTable') = 1";
            var result = await ExecuteSelectMany(sql);
            return result.Any();
        }

        private async Task<int> GetNextId()
        {
            var sql = @$"select top 1 id from [{Table}] order by 1 desc";
            var currentId = await db.LoadSingularData<int, dynamic>(sql, new { });
            return currentId + 1;
        }

        #endregion
    }
}
