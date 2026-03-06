
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
    public class SqlStringBuilder<T>
    {
        private BuilderType type;

        private enum BuilderType
        {
            Insert = 1,
            Update = 2
        }

        public string GetInsertString(T entity, string table)
        {
        
                var identifier = default(int);
                    return GetInsertString(entity, true, identifier, table);
        }

        public string GetInsertString(T entity, int nextId, string table)
        {
            return GetInsertString(entity, false, nextId, table);
        }

        private string GetInsertString(T entity, bool hasIdentityColumn, int nextId, string table)
        {
            type = BuilderType.Insert;
            var dictionary = GetDictionary(entity);
            return dictionary.CreateInsertString<T>(hasIdentityColumn, nextId, table);
        }

        public string GetUpdateString(T entity, string table)
        {
            type = BuilderType.Update;
            var dictionary = GetDictionary(entity);
            return dictionary.CreateUpdateString<T>(table);
        }

        #region private

        private Dictionary<string, string> GetDictionary(T entity)
        {
            var dictionary = new Dictionary<string, string>();
            foreach (PropertyInfo pi in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if ((type == BuilderType.Insert || type == BuilderType.Update) && pi.GetCustomAttribute(typeof(SqlInsertIgnoreAttribute)) != null) continue;
                string value;
                if (pi.PropertyType == typeof(DateTime) && (PropertyIsNull(entity, pi) || PropertyIsDateTimeNullRepresentation(entity, pi)))
                {
                    value = null;
                }
                else if (PropertyIsNull(entity, pi))
                {
                    value = null;
                }
                else if (pi.PropertyType == typeof(string))
                {
                    var str = pi.GetValue(entity).ToString();
                    value = str.Replace("'", "''");
                }
                else if (pi.PropertyType == typeof(DateTime))
                {
                    var dateTime = (DateTime) pi.GetValue(entity);
                    var formattedDateTimeString = dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
                    value = formattedDateTimeString.Replace("'", "''");
                }
                else if (pi.PropertyType == typeof(DateTime?))
                {
                    var dateTime = (DateTime?) pi.GetValue(entity);
                   if(!dateTime.HasValue) 
                    {
                        value = null;
                    }
                    else
                    {
                        var formattedDateTimeString = dateTime.Value.ToString("yyyy-MM-ddTHH:mm:ss");
                        value = formattedDateTimeString.Replace("'", "''");
                    }
                }
                else
                {
                    value = pi.GetValue(entity).ToString();
                }
                dictionary.Add(pi.Name, value);
            }

            return dictionary;
        }

        private static bool PropertyIsDateTimeNullRepresentation(T entity, PropertyInfo pi)
        {
            DateTime dateTime;
            var entityAsString = pi.GetValue(entity).ToString();
            if (!DateTime.TryParse(entityAsString, out dateTime))
            {
                return false;
            }
            return dateTime == DateTime.MinValue;
        }

        private static bool PropertyIsNull(T entity, PropertyInfo pi)
        {
            return pi.GetValue(entity) == null;
        }

        #endregion
    }

    public static partial class StringExtentions
    {
        public static string RemoveLast(this string str, int numberOfCharactersToRemove)
        {
            return str.Remove(str.Length - numberOfCharactersToRemove);
        }
    }
    
    public static partial class DictionaryExtentions
    {
        public static string CreateInsertString<T>(this Dictionary<string, string> dictionary, bool hasIdentityColumn, int nextId, string table)
        {
            var insertStringHead = $"insert into [{table}] (";
            var insertStringTail = " output inserted.[Id] values (";
            foreach (var item in dictionary)
            {
                if (item.Key == "Id" && hasIdentityColumn) continue;
                insertStringHead += $"[{item.Key}], ";
                var value = ColumnIsIdAndNextIdIsSet(nextId, item.Key) ? nextId.ToString() : dictionary[item.Key];
                insertStringTail += value == null ? "NULL, " : "'" + value + "', ";
            }
            insertStringHead = insertStringHead.RemoveLast(2);
            insertStringTail = insertStringTail.RemoveLast(2);

            return insertStringHead + ") " + insertStringTail + ")";
        }

        private static bool ColumnIsIdAndNextIdIsSet(int  nextId, string column)
        {
                         return column == "Id" && nextId > 0;
                    }

        public static string CreateUpdateString<T>(this Dictionary<string, string> dictionary, string table)
        {
            var updateString = $"update {table} set ";
            foreach (var item in dictionary.Where(x => x.Key.ToLower() != "id"))
            {
                var value = dictionary[item.Key];

                var valueString = value == null ? "NULL, " : "'" + value + "', ";

                updateString += $"[{item.Key}] = {valueString}";

            }
            updateString = updateString.RemoveLast(2);

            updateString += " where Id = " + dictionary["Id"];

            return updateString;
        }
    }
}
