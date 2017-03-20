/********************************************************************************
 * CSHARP SQL Server Library - General Elements used to work manipulate the SQL Server instance using MSPractices Library
 * 
 * NOTE: Adapted from Clinch.Data
 * 
 * LICENSE: Free to use provided details on fixes and/or extensions emailed to 
 *          chris.williams@readwatchcreate.com
 ********************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

using Microsoft.Practices.EnterpriseLibrary.Data;

namespace CSHARP.Data.SQL.MSPractices
{
    /// <summary>
    /// Utility class to assist when working with Micrsooft Practices Library to access Sql Server
    /// </summary>
    public class MsPracticesUtils
    {
        /// <summary>
        /// Check if Parameter is clean and valid data
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="parameterType"></param>
        /// <returns></returns>
        public static bool ValidateParameter(string parameter, string parameterType)
        {
            // TO DO: Needs to be more complex
            bool returnValue = true;
            parameterType.ToUpperInvariant();
            if (parameter.IndexOf("<script") > -1) returnValue = false;
            return returnValue;
        }

        /// <summary>
        /// In database workd bit fields are 1 or 0 so if the field is 1 its true.
        /// </summary>
        /// <param name="databaseBoolean"></param>
        /// <returns></returns>
        public static bool ConvertToBoolean(string databaseBoolean) { return (string.IsNullOrEmpty(databaseBoolean) == false) ? ((databaseBoolean == "1") ? true : false) : false; }

        /// <summary>
        /// Executes a SQL Server Stored Procedure
        /// </summary>
        /// <param name="procedureName">Procedure Name To Call</param>
        /// <param name="resultTableName">Name to use for result table</param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public static DataTable ExecuteProcedure(string procedureName, string resultTableName)
        {
            return ExecuteProcedure(procedureName, resultTableName, null, null, null);
        }

        /// <summary>
        /// Executes a SQL Server Stored Procedure
        /// </summary>
        /// <param name="procedureName">Procedure Name To Call</param>
        /// <param name="resultTableName">Name to use for result table</param>
        /// <param name="parameterNames">List of parameter names</param>
        /// <param name="parameterTypes">List of parameter types</param>
        /// <param name="parameterValues">List of parameter values</param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public static DataTable ExecuteProcedure(string procedureName, string resultTableName, List<string> parameterNames, List<string> parameterTypes, List<string> parameterValues)
        {
            return ExecuteProcedure(string.Empty, procedureName, resultTableName, parameterNames, parameterTypes, parameterValues);
        }

        /// <summary>
        /// Executes a SQL Server Stored Procedure
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="procedureName">Procedure Name To Call</param>
        /// <param name="resultTableName">Name to use for result table</param>
        /// <param name="parameterNames">List of parameter names</param>
        /// <param name="parameterTypes">List of parameter types</param>
        /// <param name="parameterValues">List of parameter values</param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public static DataTable ExecuteProcedure(string connectionString, string procedureName, string resultTableName, List<string> parameterNames, List<string> parameterTypes, List<string> parameterValues)
        {

            DataTable returnValue = null;
            Database db = null;
            if (string.IsNullOrEmpty(connectionString) == true)
            {
                db = DatabaseFactory.CreateDatabase();
            }
            else
            {
                db = DatabaseFactory.CreateDatabase(connectionString);
            }
            DbCommand dbCommandWrapper = db.GetStoredProcCommand(procedureName);
            try
            {
                if (parameterNames != null && parameterTypes != null && parameterValues != null)
                {
                    if (parameterNames.Count == parameterTypes.Count && parameterTypes.Count == parameterValues.Count)
                    {
                        for (int ndx = 0; ndx < parameterNames.Count<string>(); ndx++)
                        {
                            string text = parameterTypes[ndx].ToString().ToUpperInvariant();
                            switch (text)
                            {
                                case "BOOLEAN":
                                    {
                                        db.AddInParameter(dbCommandWrapper, "@" + parameterNames[ndx], System.Data.DbType.Boolean, Convert.ToBoolean(parameterValues[ndx]));
                                        break;
                                    }
                                case "DATETIME":
                                    {
                                        db.AddInParameter(dbCommandWrapper, "@" + parameterNames[ndx], System.Data.DbType.DateTime, Convert.ToDateTime(parameterValues[ndx]));
                                        break;
                                    }
                                case "DECIMAL":
                                    {
                                        db.AddInParameter(dbCommandWrapper, "@" + parameterNames[ndx], System.Data.DbType.Decimal, Convert.ToDecimal(parameterValues[ndx]));
                                        break;
                                    }
                                case "DOUBLE":
                                    {
                                        db.AddInParameter(dbCommandWrapper, "@" + parameterNames[ndx], System.Data.DbType.Double, Convert.ToDouble(parameterValues[ndx]));
                                        break;
                                    }
                                case "GUID":
                                    {
                                        db.AddInParameter(dbCommandWrapper, "@" + parameterNames[ndx], System.Data.DbType.Guid, new Guid(parameterValues[ndx]));
                                        break;
                                    }
                                case "INTEGER":
                                    {
                                        db.AddInParameter(dbCommandWrapper, "@" + parameterNames[ndx], System.Data.DbType.Int32, Convert.ToInt32(parameterValues[ndx]));
                                        break;
                                    }
                                case "STRING":
                                    {
                                        db.AddInParameter(dbCommandWrapper, "@" + parameterNames[ndx], System.Data.DbType.AnsiString, parameterValues[ndx]);
                                        break;
                                    }
                            }
                        }
                    }
                }
                db.AddParameter(dbCommandWrapper, "RETURN_VALUE", System.Data.DbType.Int32, System.Data.ParameterDirection.ReturnValue, string.Empty, System.Data.DataRowVersion.Default, DBNull.Value);
                System.Data.DataSet ds = db.ExecuteDataSet(dbCommandWrapper);
                int spReturnValue = 0;
                if (int.TryParse(db.GetParameterValue(dbCommandWrapper, "RETURN_VALUE").ToString(), out spReturnValue))
                {
                    if (spReturnValue == 0)
                    {
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                ds.Tables[0].TableName = resultTableName;
                                returnValue = ds.Tables[0];
                            }
                        }
                    }
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                throw new Exception(ex.Message + "\nStored Procedure Line Number:" + ex.LineNumber);
            }
            catch (Exception ex2)
            {
                throw new Exception(ex2.Message + "\n");
            }
            finally
            {
            }
            return returnValue;
        }

        /// <summary>
        /// Executes a SQL Server Store Procedure and Returns the First Row (if one exists)
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="procedureName">Procedure Name To Call</param>
        /// <param name="resultTableName">Name to use for result table</param>
        /// <param name="parameterNames">List of parameter names</param>
        /// <param name="parameterTypes">List of parameter types</param>
        /// <param name="parameterValues">List of parameter values</param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public static DataRow ExecuteProcedureReturnFirstRecord(string connectionString, string procedureName, string resultTableName, List<string> parameterNames, List<string> parameterTypes, List<string> parameterValues)
        {
            DataRow returnValue = null;
            DataTable table = ExecuteProcedure(connectionString, procedureName, resultTableName, parameterNames, parameterTypes, parameterValues);
            if (table != null) if (table.Rows.Count > 0) returnValue = table.Rows[0];
            return returnValue;
        }

        /// <summary>
        /// Converts a .NET DataTable to JSON
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static string JSON_DataTable(System.Data.DataTable dataTable)
        {
            StringBuilder JsonString = new StringBuilder();
            JsonString.Append("{ ");
            JsonString.Append("\"TABLE\":[{ ");
            JsonString.Append("\"ROW\":[ ");
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                JsonString.Append("{ ");
                JsonString.Append("\"COL\":[ ");
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    if (j < dataTable.Columns.Count - 1)
                    {
                        JsonString.Append("{\"DATA\":\"" + dataTable.Rows[i][j].ToString() + "\"},");
                    }
                    else
                    {
                        if (j == dataTable.Columns.Count - 1)
                        {
                            JsonString.Append("{\"DATA\":\"" + dataTable.Rows[i][j].ToString() + "\"}");
                        }
                    }
                }
                if (i == dataTable.Rows.Count - 1)
                {
                    JsonString.Append("]} ");
                }
                else
                {
                    JsonString.Append("]}, ");
                }
            }
            JsonString.Append("]}]}");
            return JsonString.ToString();
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static string CreateJsonParameters(System.Data.DataTable dataTable)
        {
            StringBuilder JsonString = new StringBuilder();
            string result;
            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                JsonString.Append("{ ");
                JsonString.Append("\"Head\":[ ");
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    JsonString.Append("{ ");
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        if (j < dataTable.Columns.Count - 1)
                        {
                            JsonString.Append(string.Concat(new string[]
							{
								"\"", 
								dataTable.Columns[j].ColumnName.ToString(), 
								"\":\"", 
								dataTable.Rows[i][j].ToString(), 
								"\","
							}));
                        }
                        else
                        {
                            if (j == dataTable.Columns.Count - 1)
                            {
                                JsonString.Append(string.Concat(new string[]
								{
									"\"", 
									dataTable.Columns[j].ColumnName.ToString(), 
									"\":\"", 
									dataTable.Rows[i][j].ToString(), 
									"\""
								}));
                            }
                        }
                    }
                    if (i == dataTable.Rows.Count - 1)
                    {
                        JsonString.Append("} ");
                    }
                    else
                    {
                        JsonString.Append("}, ");
                    }
                }
                JsonString.Append("]}");
                result = JsonString.ToString();
            }
            else
            {
                result = null;
            }
            return result;
        }
    }
}