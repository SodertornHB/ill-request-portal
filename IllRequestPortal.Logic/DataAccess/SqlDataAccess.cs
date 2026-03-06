
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten. 
// Generator version: 0.0.1.0
//-------------------------------------------------------------------------------------------------------------------- 

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;

namespace IllRequestPortal.Logic.DataAccess
{

    public interface ISqlDataAccess
    {
        Task<T> LoadSingularData<T, U>(string sql, U parameters);
        Task<List<T>> LoadData<T, U>(string sql, U parameters);
        Task<int> InsertData<T>(string sql, T parameters);
        Task UpdateData<T>(string sql, T parameters);
        T LoadSingularDataSynchronous<T, U>(string sql, U parameters);        
        string GetConnectionString();
    }

    public class SqlDataAccess : ISqlDataAccess
    {
        private string connectionString;
        private readonly ILogger logger;

        public SqlDataAccess(IConfiguration iconfiguration,
            ILogger<SqlDataAccess> logger)
        {
            connectionString = iconfiguration.GetConnectionString("Default");
            this.logger = logger;
        }
        public string GetConnectionString() => connectionString;

        public async Task<T> LoadSingularData<T, U>(string sql, U parameters)
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
                }
                catch (Exception e)
                {
                    LogException(e);
                    connection.Close();
                    throw;
                }
            }
        }

        public async Task<List<T>> LoadData<T, U>(string sql, U parameters)
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    var data = await connection.QueryAsync<T>(sql, parameters);
                    return data.AsList();
                }
                catch (Exception e)
                {
                    LogException(e);
                    connection.Close();
                    throw;
                }
            }
        }

        public T LoadSingularDataSynchronous<T, U>(string sql, U parameters)
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    return connection.QueryFirstOrDefault<T>(sql, parameters);
                }
                catch (Exception e)
                {
                    LogException(e);
                    connection.Close();
                    throw;
                }
            }
        }

        public async Task<int> InsertData<T>(string sql, T parameters)
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                           
                int newId = default;
                        
                try
                {
                    newId = await connection.QuerySingleAsync<int>(sql, parameters);
                    return newId;
                }
                catch (Exception e)
                {
                    LogException(e);
                    connection.Close();
                    throw;
                }
                return newId;
            }
        }


        public async Task UpdateData<T>(string sql, T parameters)
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.ExecuteAsync(sql, parameters);
                }
                catch (Exception e)
                {
                    LogException(e);
                    connection.Close();
                    throw;
                }
            }
        }

        #region private 

        private void LogException(Exception e)
        {
            logger.LogError(e, "An error occured when quering data from data source");
        }

        #endregion

    }
}