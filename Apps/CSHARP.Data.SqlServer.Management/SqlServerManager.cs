/********************************************************************************
 * CSHARP SQL Server Manager Library - General Elements used to work manipulate the SQL Server instance
 * 
 * NOTE: Adapted from Clinch.Data
 * 
 * LICENSE: Free to use provided details on fixes and/or extensions emailed to 
 *          chris.williams@readwatchcreate.com
 ********************************************************************************/

using System.Collections.Generic;
using System.Data.SqlClient;

namespace CSHARP.SqlServer.Management
{
    /// <summary>
    /// This class assists with managing databases on SQL Server
    /// </summary>
    /// <remarks>Based on code from http://www.codeproject.com/Articles/72465/Programmatically-Enumerating-Attaching-and-Detachi </remarks>
    public static class SqlServerManager
    {
        /// <summary>
        /// Attaches a database to the server
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="databaseName"></param>
        /// <param name="physicalPath">including .mdf</param>
        public static bool AttachDatabase(string connectionString, string databaseName, string physicalPath)
        {
            using (var conn = new SqlConnection(connectionString))
            {

                using (var cmd = new SqlCommand("", conn))
                {
                    cmd.CommandText = string.Format(
                        "CREATE DATABASE '{0}' ON PRIMARY ( FILENAME =  '{1}' ) FOR ATTACH", databaseName, physicalPath);

                    conn.Open();

                    cmd.ExecuteNonQuery();

                }
            }

            return true;
        }

        /// <summary>
        /// Gets the list of databases
        /// </summary>
        /// <param name="connectionString">Connection String used to connect to database server</param>
        /// <param name="preSqlServer2000">If true, will use the alternative method used for SQL Server 2000 or older</param>
        /// <returns></returns>
        /// <remarks>Note that it will may ONLY return database you have permission to</remarks>
        public static List<string> GetDatabaseList(string connectionString, bool preSqlServer2000)
        {
            var databaseList = new List<string>();

            using (var conn = new SqlConnection(connectionString))
            {
                using (var cmd = new SqlCommand("", conn)
                {
                    CommandText =
                        (preSqlServer2000)
                            ? "SELECT DISTINCT CATALOG_NAME FROM INFORMATION_SCHEMA.SCHEMATA"
                            : "SELECT [name] AS CATALOG_NAME FROM sys.sysdatabases"
                })
                {

                    conn.Open();

                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            databaseList.Add(rdr.GetString(0));
                        }

                    }
                }
            }

            return databaseList;
        }

    }
}
