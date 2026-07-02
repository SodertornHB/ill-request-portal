
//--------------------------------------------------------------------------------------------------------------------
// Warning! This is an auto generated file. Changes may be overwritten.
// Generator version: 0.0.1.0
//--------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IllRequestPortal.Logic.DataAccess
{
    // Builds INSERT and UPDATE statements with @parameter placeholders. Property values
    // are bound by Dapper when the statement is executed and are never concatenated
    // into the SQL string.
    public class SqlStringBuilder<T>
    {
        public string GetInsertString(T entity, string table)
        {
            return CreateInsertString(entity, true, table);
        }

        // The entity's Id property must be assigned the pre-fetched id before the
        // statement is executed - the Id column is bound from the @Id parameter.
        public string GetInsertString(T entity, int nextId, string table)
        {
            return CreateInsertString(entity, false, table);
        }

        public string GetUpdateString(T entity, string table)
        {
            var setClauses = GetParameterizedColumns(entity)
                .Where(column => column.Key.ToLower() != "id")
                .Select(column => $"[{column.Key}] = {column.Value}");

            return $"update [{table}] set {string.Join(", ", setClauses)} where [Id] = @Id";
        }

        #region private

        private static string CreateInsertString(T entity, bool hasIdentityColumn, string table)
        {
            var columns = GetParameterizedColumns(entity)
                .Where(column => !(column.Key == "Id" && hasIdentityColumn))
                .ToList();
            var columnNames = string.Join(", ", columns.Select(column => $"[{column.Key}]"));
            var values = string.Join(", ", columns.Select(column => column.Value));

            return $"insert into [{table}] ({columnNames}) output inserted.[Id] values ({values})";
        }

        private static Dictionary<string, string> GetParameterizedColumns(T entity)
        {
            var columns = new Dictionary<string, string>();
            foreach (PropertyInfo pi in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (pi.GetCustomAttribute(typeof(SqlInsertIgnoreAttribute)) != null) continue;
                columns.Add(pi.Name, StoreAsNull(entity, pi) ? "NULL" : "@" + pi.Name);
            }

            return columns;
        }

        private static bool StoreAsNull(T entity, PropertyInfo pi)
        {
            var value = pi.GetValue(entity);
            if (value == null) return true;
            // DateTime.MinValue represents "not set" and is outside the range of the sql server datetime type.
            if (value is DateTime dateTime) return dateTime == DateTime.MinValue;
            return false;
        }

        #endregion
    }
}
