/********************************************************************************
 * CSHARP SQL Library - General Elements used to work with SQL Server
 * 
 * NOTE: Adapted from Clinch.Data
 * 
 * LICENSE: Free to use provided details on fixes and/or extensions emailed to 
 *          chris.williams@readwatchcreate.com
 ********************************************************************************/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

using CSHARP.Diagnostics;

namespace CSHARP.Data.SQL.Standard
{
    /// <summary>
    /// Static methods to help work with the Standard .NET SQL Client classes
    /// V1.0.0.2 - Classname case Changed
    /// </summary>
    public static class SqlServerHelper
    {
        #region ReadDataTableFromSqlServerViaQuery - WARNING: In Production be aware of SQL Injection 

        /// <summary>
        /// Reads data into a .NET DataTable based on the results of a SQL Server Stored Procedure
        /// </summary>
        /// <param name="connectionStringName">Name of Connection String entry in Web.Config</param>
        /// <param name="query">SQL Query</param>
        /// <param name="eventLog">StringBuilder used to store any log messages</param>
        /// <returns></returns>
        /// <remarks>WARNING: In production environment be aware of possible SQL Injection<br/>
        /// NEW in v1.0.0.6</remarks>
        public static DataTable ReadDataTableFromSqlServerViaQuery(string connectionStringName,
            string query, IEventLog eventLog)
        { 
            var settings = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (settings == null)
                throw new Exception(string.Format("Error Connecting to Source Database: {0}", connectionStringName));

            DataTable results = new DataTable();

            using (SqlConnection conn = new SqlConnection(settings.ConnectionString))
            using (SqlCommand command = new SqlCommand(query, conn))
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
                dataAdapter.Fill(results);

            return results;
        }

        #endregion

        #region ReadDataTableFromSqlServerViaStoredProcedure 

        /// <summary>
        /// Reads data into a .NET DataTable based on the results of a SQL Server Stored Procedure
        /// </summary>
        /// <param name="connectionStringName">Name of connection string in config file</param>
        /// <param name="storedProcedureName">Name of stored procedure to execute</param>
        /// <param name="eventLog">StringBuilder used to store any log messages</param>
        /// <returns></returns>
        /// <remarks>NEW in 1.0.0.3</remarks>
        public static DataTable ReadDataTableFromSqlServerViaStoredProcedure(string connectionStringName,
            string storedProcedureName, IEventLog eventLog)
        {
            var settings = ConfigurationManager.ConnectionStrings[connectionStringName];


            if (settings == null)
                throw new Exception(string.Format("Error Connecting to Source Database: {0}", connectionStringName));

            var results = new DataTable();

            using (var conn = new SqlConnection(settings.ConnectionString))
            {
                using (var command = new SqlCommand(storedProcedureName, conn)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                    conn.Open();
                    var reader = command.ExecuteReader();
                    results.Load(reader);
                    conn.Close();
                }
            }

            return results;
        }

        /// <summary>
        /// Reads data into a .NET DataTable based on the results of a SQL Server Stored Procedure
        /// </summary>
        /// <param name="connectionString">Actual Connection String</param>
        /// <param name="storedProcedureName">Name of stored procedure to execute</param>
        /// <param name="eventLog">StringBuilder used to store any log messages</param>
        /// <returns></returns>
        /// <remarks>NEW in 1.0.0.3</remarks>
        public static DataTable ReadDataTableFromSqlServerViaStoredProcedureAndRawConnectionString(string connectionString,
            string storedProcedureName, IEventLog eventLog)
        {
            var results = new DataTable();

            using (var conn = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand(storedProcedureName, conn)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                    conn.Open();
                    var reader = command.ExecuteReader();
                    results.Load(reader);
                    conn.Close();
                }
            }

            return results;
        }

        /// <summary>
        /// Reads data into a .NET DataTable based on the results of a SQL Server Stored Procedure
        /// </summary>
        /// <param name="connectionStringName">Name of Connection String entry in Web.Config</param>
        /// <param name="storedProcedureName">Name of stored procedure to execute</param>
        /// <param name="parameterNames">List of parameter names</param>
        /// <param name="parameterTypes">List of parameter types</param>
        /// <param name="parameterValues">List of parameter values</param>
        /// <param name="eventLog">StringBuilder used to store any log messages</param>
        /// <returns></returns>
        /// <remarks>New in 1.0.0.1</remarks>
        public static DataTable ReadDataTableFromSqlServerViaStoredProcedure(string connectionStringName,
            string storedProcedureName, List<string> parameterNames, List<string> parameterTypes,
            List<string> parameterValues, IEventLog eventLog)
        {
            var settings = ConfigurationManager.ConnectionStrings[connectionStringName];


            if (settings == null)
                throw new Exception(string.Format("Error Connecting to Source Database: {0}", connectionStringName));

            return ReadDataTableFromSqlServerViaStoredProcedureAndRawConnectionString(settings.ConnectionString,
                storedProcedureName, parameterNames, parameterTypes, parameterValues, eventLog);
        }

        /// <summary>
        /// Reads data into a .NET DataTable based on the results of a SQL Server Stored Procedure
        /// </summary>
        /// <param name="connectionString">Actual Connection String</param>
        /// <param name="storedProcedureName">Name of stored procedure to execute</param>
        /// <param name="parameterNames">List of parameter names</param>
        /// <param name="parameterTypes">List of parameter types</param>
        /// <param name="parameterValues">List of parameter values</param>
        /// <param name="eventLog">StringBuilder used to store any log messages</param>
        /// <returns></returns>
        /// <remarks>NEW in 1.0.0.3<br/>
        /// FIXED in v1.0.0.4 - If empty string in parameter value of type guid we automatically pass Guid.Empty.
        /// FIXED in v1.0.0.5 - If parameterNames is null then assume call to procedure with no parameters.
        /// </remarks>
        public static DataTable ReadDataTableFromSqlServerViaStoredProcedureAndRawConnectionString(string connectionString,
            string storedProcedureName, List<string> parameterNames, List<string> parameterTypes,
            List<string> parameterValues, IEventLog eventLog)
        {
            var results = new DataTable();

            using (var conn = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand(storedProcedureName, conn)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                    conn.Open();

                    // if parameters passed then ensure they are added as SQL Parameters
                    if (parameterNames != null)
                    {
                        // TO DO: Handle parameters here
                        for (var parameterIndex = 0; parameterIndex < parameterNames.Count; parameterIndex++)
                        {
                            try
                            {
                                DbType dbType = DbType.String;
                                switch (parameterTypes[parameterIndex].ToUpperInvariant())
                                {
                                    case "GUID":
                                        dbType = DbType.Guid;
                                        break;
                                    case "BOOLEAN":
                                        dbType = DbType.Boolean;
                                        break;

                                    case "DATETIME":
                                        dbType = DbType.DateTime;
                                        break;

                                    //                              SqlDbType.BigInt;
                                    //                              SqlDbType.Binary;
                                    //                              SqlDbType.Char
                                    //                              SqlDbType.Decimal 
                                    //                              SqlDbType.Float
                                    //                              SqlDbType.Int;
                                    case "STRING":
                                        dbType = DbType.String;
                                        break;
                                }

                                // FIX: v1.0.0.4 - If empty string then ignore parameter.
                                if (string.IsNullOrEmpty(parameterValues[parameterIndex])) continue;

                                var parameter = new SqlParameter(parameterNames[parameterIndex],
                                    parameterValues[parameterIndex])
                                { DbType = dbType };
                                // TO DO: Direction
                                command.Parameters.Add(parameter);
                            }
                            catch(Exception exception)
                            {
                                // Log some more details to help debug
                                if (eventLog != null) eventLog.LogEvent(0, "ReadDataTableFromSqlServerViaStoredProcedureAndRawConnectionString: " + parameterIndex + " - " + parameterNames);
                                throw new Exception("ReadDataTableFromSqlServerViaStoredProcedureAndRawConnectionString: " + parameterIndex + " - " + parameterNames, exception);
                            }
                        }
                    }

			        var adapter = new SqlDataAdapter(command);
                    adapter.Fill(results);
                    conn.Close();
                }
            }

            return results;
        }

        #endregion
    }
}