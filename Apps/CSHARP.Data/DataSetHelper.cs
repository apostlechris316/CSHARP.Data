/********************************************************************************
 * CSHARP DataSet Library - General Elements used to manipulate .net datasets 
 * 
 * NOTE: Adapted from Clinch.Data
 * 
 * LICENSE: Free to use provided details on fixes and/or extensions emailed to 
 *          chris.williams@readwatchcreate.com
 ********************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using CSHARP.Diagnostics;
using CSHARP.Text;

namespace CSHARP.Data
{
	/// <summary>
	/// DataSet Builder
	/// </summary>
	public static class DataSetHelper
    {
        #region CreateTable 

        /// <summary>
        /// Attempts to create a table on a dataset
        /// </summary>
        /// <param name="dataSetXml">If not passed in one is created</param>
        /// <param name="tableName">Name of table to create</param>
        /// <param name="fields">Delimted List of field names to create</param>
        /// <param name="delimiter">Delimiter</param>
        /// <returns>Xml representation of dataset with additional table added</returns>
        /// <remarks>NEW in 2.0.0.6</remarks>
	    public static string CreateTable    (string dataSetXml, string tableName, string fields, char delimiter)
        {
            var dataSet = (string.IsNullOrEmpty(dataSetXml)) ? new DataSet() : ConvertXmlStringToDataSet(dataSetXml, false);
            CreateTable(ref dataSet, tableName, fields, delimiter);
            return ConvertDataSetToXml(dataSet);

        }

        /// <summary>
		/// Attempts to create a table on a dataset
		/// </summary>
		/// <param name="dataSet">If not passed in one is created</param>
		/// <param name="tableName">Name of table to create</param>
		/// <param name="fields">Delimted List of field names to create</param>
		/// <param name="delimiter">Delimiter</param>
		public static void CreateTable(ref DataSet dataSet, string tableName, string fields, char delimiter)
		{
            if (dataSet == null) dataSet = new DataSet();
            
            var fieldList = new List<string>();
			fieldList.AddRange(fields.Split(delimiter));
			CreateTable(ref dataSet, tableName, fieldList);
		}

        /// <summary>
		/// Attempts to create a table on a dataset.  If it exists it will add fields that do not already exist.
		/// </summary>
		/// <param name="dataSet">If not passed in one is created</param>
		/// <param name="tableName">Name of table to create</param>
		/// <param name="fieldList">Array of field names to create</param>
		public static void CreateTable(ref DataSet dataSet, string tableName, List<string> fieldList)
		{
			if (dataSet == null) dataSet = new DataSet();
            if (!dataSet.Tables.Contains(tableName)) dataSet.Tables.Add(tableName);

            foreach (var field in fieldList) if (!dataSet.Tables[tableName].Columns.Contains(field)) dataSet.Tables[tableName].Columns.Add(field);
		}

        /// <summary>
        /// Attempts to create a table on a dataset
        /// </summary>
        /// <param name="dataSet">If not passed in one is created</param>
        /// <param name="tableName">Name of table to create</param>
        /// <param name="fields">Delimted List of field names to create</param>
        /// <param name="delimiter">Delimiter</param>
        public static void CreateTable(DataSet dataSet, string tableName, string fields, char delimiter)
        {
            var fieldList = new List<string>();
            fieldList.AddRange(fields.Split(delimiter));
            CreateTable(dataSet, tableName, fieldList);
        }
        /// <summary>
        /// Attempts to create a table on a dataset.  If it exists it will add fields that do not already exist.
        /// </summary>
        /// <param name="dataSet">If not passed in one is created</param>
        /// <param name="tableName">Name of table to create</param>
        /// <param name="fieldList">Array of field names to create</param>
        public static void CreateTable(DataSet dataSet, string tableName, List<string> fieldList)
        {
            if (dataSet == null)
                throw new InvalidDataException();

            if (!dataSet.Tables.Contains(tableName)) dataSet.Tables.Add(tableName);

            foreach (var field in fieldList.Where(field => !dataSet.Tables[tableName].Columns.Contains(field)))
            {
                dataSet.Tables[tableName].Columns.Add(field);
            }
        }

        #endregion

        #region Csv String To DataTable

        /// <summary>
        /// Converts a CSV String into a Data Table
        /// </summary>
        /// <param name="csvContent">String containing CSV Content</param>
        /// <param name="tableName">Name of Table</param>
        /// <returns>DataTable object</returns>
        /// <remarks>V2.0.0.3 - Method Name changed to proper case</remarks>
        public static DataTable ConvertCsvStringToDataTable(string csvContent, string tableName)
        {
            //Process output and return
            var split = csvContent.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (split.Length < 2) return null;
            var iteration = 0;
            DataTable table = null;
 
            foreach (var values in split)
            {
                if (iteration == 0)
                {
                    var columnNames = StringHelper.SplitStringIntoLines(values);
                    table = new DataTable(tableName);

                    table.Columns.AddRange(columnNames.Select(columnName => new DataColumn(columnName)).ToArray());
                }
                else
                {
                    var fields = StringHelper.SplitStringIntoLines(values);
                    if (table != null)
                    {
                        if (fields != null) table.Rows.Add(fields);
                    }
                }
 
                iteration++;
            }
 
            return table;
        }

        #endregion

        #region DataTable To CSV

	    /// <summary>
	    /// Takes a DataSet and Table Name and returns a CSV String
	    /// </summary>
	    /// <param name="dataSet">Dataset containing the table</param>
	    /// <param name="tableName">Name of the table</param>
	    /// <returns>csv representation of table</returns>
	    /// <remarks>NEW in v2.0.0.7
	    /// Taken from StackOverflow Article https://stackoverflow.com/questions/4959722/c-sharp-datatable-to-csv </remarks>
	    public static string ConvertDataTableToCsv(DataSet dataSet, string tableName)
	    {
	        return dataSet.Tables.Contains(tableName) == false ? string.Empty : ConvertDataTableToCsv(dataSet.Tables[tableName]);
	    }

	    /// <summary>
        /// Takes a DataTable and returns a CSV String
        /// </summary>
        /// <param name="dataTable">DataTable object to convert to CSV</param>
        /// <returns>csv representation of table</returns>
        /// <remarks>NEW in v2.0.0.7
        /// Taken from StackOverflow Article https://stackoverflow.com/questions/4959722/c-sharp-datatable-to-csv </remarks>
	    public static string ConvertDataTableToCsv(DataTable dataTable)
	    {
	        var sb = new StringBuilder();

	        var columnNames = dataTable.Columns.Cast<DataColumn>().
	            Select(column => column.ColumnName).
	            ToArray();
	        sb.AppendLine(string.Join(",", columnNames));

            foreach (var fields in from DataRow row in dataTable.Rows select row.ItemArray.Select(field =>
                string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\"")))
            {
                sb.AppendLine(string.Join(",", fields));
            }

	        return sb.ToString();
	    }

	    #endregion

        #region XML Export

        /// <summary>
		/// Generates an xml based on a .net datatable object
		/// </summary>
		/// <param name="dataTable">DataTable object to convert to xml</param>
		/// <returns>xml representation of data table</returns>
		public static string ConvertDataTableToXml(DataTable dataTable)
		{
			var returnValue = string.Empty;
			if (dataTable != null) returnValue = dataTable.DataSet.GetXml();
            return returnValue;
		}

        /// <summary>
		/// Genereates an xml based on a .net dataset object
		/// </summary>
		/// <param name="dataSet">DataSet object to convert to xml</param>
		/// <returns>xml representation of dataset</returns>
		public static string ConvertDataSetToXml(DataSet dataSet)
		{
			var returnValue = string.Empty;
			if (dataSet != null) returnValue = dataSet.GetXml();
			return returnValue;
		}

        #endregion

        #region XML Import

        /// <summary>
        /// Gets XML From Url and uses it to create dataset
        /// </summary>
        /// <param name="url">Absolute Url to xml to convert to dataset</param>
        /// <param name="enforceConstraints">If true, will respecet contraints in when converting</param>
        /// <returns>.Net Dataset Object</returns>
        /// <remarks>new in 2.0.0.3</remarks>
        public static DataSet ConvertXmlFromUrlToDataSet(string url, bool enforceConstraints)
        {
            var returnValue = new DataSet { EnforceConstraints = enforceConstraints };
            returnValue.ReadXml(new XmlTextReader(url));
            return returnValue;
        }

        /// <summary>
		/// Generates a .net dataset object based on a supplied xml string
		/// </summary>
		/// <param name="xml">xml string to convert to dataset</param>
		/// <param name="enforceConstraints">if true, enforces contraints when converting</param>
        /// <returns>.Net Dataset Object</returns>
        public static DataSet ConvertXmlStringToDataSet(string xml, bool enforceConstraints)
		{
            var returnValue = new DataSet {EnforceConstraints = enforceConstraints};
            returnValue.ReadXml(new StringReader(xml));
			return returnValue;
		}

        #endregion

        #region Insert/Add Row To Table

	    /// <summary>
	    /// Inserts a datarow into a table in a dataset
	    /// </summary>
        /// <param name="dataSet">DataSet object to insert row in</param>
        /// <param name="tableName">Name of table to insert row in</param>
	    /// <param name="columns">Collection of columns</param>
	    /// <param name="sourceRow">Row to insert in the table</param>
        /// <returns>True if row inserted successfully</returns>
        public static bool InsertRow(DataSet dataSet, string tableName, DataColumnCollection columns,
	        DataRow sourceRow)
	    {
            var fields = (from DataColumn column in columns select column.ColumnName).ToList();
            var values = (from DataColumn column in columns select sourceRow[column.ColumnName].ToString()).ToList();
	        CreateTable(ref dataSet, tableName, fields);
            AddRowToTable(dataSet.Tables[tableName], fields, values);
	        return true;
	    }

        /// <summary>
        /// Inserts A Row Into A Table In A DataSet. 
        /// NOTE: This version will create the dataset, table and fields if they do not exist.
        /// </summary>
        /// <param name="dataSetXml">Dataset to add table to</param>
        /// <param name="tableName">table to add row to</param>
        /// <param name="fields">delimited list of field names</param>
        /// <param name="values">delimited list of field values</param>
        /// <param name="delimiter">delimiter</param>
        /// <returns>The xml with the new data row inserted</returns>
        /// <remarks>NEW in 2.0.0.6</remarks>
        public static string InsertRow(string dataSetXml, string tableName, string fields, string values, char delimiter)
        {
            var dataSet = (string.IsNullOrEmpty(dataSetXml)) ? new DataSet() : ConvertXmlStringToDataSet(dataSetXml, false);

            CreateTable(ref dataSet, tableName, fields, delimiter);
            AddRowToTable(dataSet.Tables[tableName], fields, values, delimiter);
            return ConvertDataSetToXml(dataSet);
        }

        /// <summary>
		/// Inserts A Row Into A Table In A DataSet. 
		/// NOTE: This version will create the dataset, table and fields if they do not exist.
		/// </summary>
		/// <param name="dataSet">Dataset to add table to</param>
		/// <param name="tableName">table to add row to</param>
		/// <param name="fields">delimited list of field names</param>
		/// <param name="values">delimited list of field values</param>
		/// <param name="delimiter">delimiter</param>
		/// <returns>True if row inserted successfully</returns>
		public static bool InsertRow(DataSet dataSet, string tableName, string fields, string values, char delimiter)
		{
			CreateTable(ref dataSet, tableName, fields, delimiter);
			AddRowToTable(dataSet.Tables[tableName], fields, values, delimiter);
			return true;
		}
        /// <summary>
        /// Inserts A Row Into A Table In A DataSet. 
        /// NOTE: This version will create the dataset, table and fields if they do not exist.
        /// </summary>
        /// <param name="dataSet">Dataset to add table to</param>
        /// <param name="tableName">table to add row to</param>
        /// <param name="fieldList">list of field names</param>
        /// <param name="valueList">list of field values</param>
        /// <returns>True if row inserted successfully</returns>
        /// <remarks>v2.0.0.5 supports passing field and values string lists</remarks>
        public static bool InsertRow(DataSet dataSet, string tableName, List<string> fieldList, List<string> valueList)
        {
            CreateTable(ref dataSet, tableName, fieldList);
            AddRowToTable(dataSet.Tables[tableName], fieldList, valueList);
            return true;
        }

		/// <summary>
		/// Adds a row to the table passed in
		/// </summary>
		/// <param name="dataTable">If not passed in one is created</param>
		/// <param name="fields">Delimted List of field names to insert into the new row</param>
		/// <param name="values">Delimited List of field values to insert into the new row</param>
		/// <param name="delimiter">Delimiter</param>
		public static void AddRowToTable(DataTable dataTable, string fields, string values, char delimiter)
		{
			var fieldList = new List<string>();
			fieldList.AddRange(fields.Split(delimiter));
			var valueList = new List<string>();
			valueList.AddRange(values.Split(delimiter));
			AddRowToTable(dataTable, fieldList, valueList);
		}

		/// <summary>
		/// Adds a row to the table passed in
		/// </summary>
		/// <param name="dataTable">If not passed in one is created</param>
		/// <param name="fieldList">Array of field names to insert into the new row</param>
		/// <param name="valueList">Array of field values to insert into the new row</param>
		public static void AddRowToTable(DataTable dataTable, List<string> fieldList, List<string> valueList)
		{
			if (fieldList.Count != valueList.Count) throw new Exception("ERROR - FIELD/VALUE MISMATCH: You must enter the same number of fields and values.");
			
			var drNew = dataTable.NewRow();
			for (var fieldNdx = 0; fieldNdx < fieldList.Count; fieldNdx++)
			{
				drNew[fieldList[fieldNdx]] = valueList[fieldNdx];
			}
			dataTable.Rows.Add(drNew);
		}

        /// <summary>
        /// Adds a row to the table passed in
        /// </summary>
        /// <param name="dataTable">If not passed in one is created</param>
        /// <param name="fields">Delimted List of field names to insert into the new row</param>
        /// <param name="values">Delimited List of field values to insert into the new row</param>
        /// <param name="delimiter">Delimiter</param>
        public static void AddRowToTableShortcutColumns(DataTable dataTable, string fields, string values, char delimiter)
        {
            var fieldList = new List<string>();
            fieldList.AddRange(fields.Split(delimiter));
            var valueList = new List<string>();
            valueList.AddRange(values.Split(delimiter));
            AddRowToTableShortcutColumns(dataTable, fieldList, valueList);
        }

        /// <summary>
        /// Adds a row to the table but ignore extra columns in fieldList
        /// </summary>
        /// <param name="dataTable">If not passed in one is created</param>
        /// <param name="fieldList">Array of field names to insert into the new row</param>
        /// <param name="valueList">Array of field values to insert into the new row</param>
        /// <returns></returns>
        public static void AddRowToTableShortcutColumns(DataTable dataTable, List<string> fieldList, List<string> valueList)
        {
            var drNew = dataTable.NewRow();
            for (var fieldNdx = 0; fieldNdx < fieldList.Count; fieldNdx++)
            {
                if (fieldNdx < valueList.Count) drNew[fieldList[fieldNdx]] = valueList[fieldNdx];
            }
            dataTable.Rows.Add(drNew);
        }

        #endregion

        #region Data Row To String

        /// <summary>
        /// Returns a string representation for the row in the format name=value~name=value
        /// </summary>
        /// <param name="row">DataRow to convert to string</param>
        /// <returns>string in teh format name=value~name=value</returns>
        /// <remarks> (NEW in 2.0.0.1)</remarks>
        public static string DataRowToString(DataRow row)
        {
            var result = new StringBuilder();
            foreach(DataColumn column in row.Table.Columns)
            {
                result.AppendFormat(result.Length == 0 ? "{0}={1}" : "~{0}={1}", column.ColumnName,
                    row[column.ColumnName]);
            }

            return result.ToString();
        }

        #endregion

        #region DataTableColumnToDelimitedString 

	    /// <summary>
	    /// Returns a delimited string of the column values for all rows in a table
	    /// </summary>
	    /// <param name="xml"></param>
	    /// <param name="tableName"></param>
	    /// <param name="enforceConstraints"></param>
	    /// <param name="columnName"></param>
	    /// <param name="token"></param>
	    /// <param name="eventLog"></param>
	    /// <returns>Delimited string containing all rows in DataTable</returns>
        /// <remarks>(NEW in 2.0.0.4)</remarks>
        public static string DataTableColumnToDelimitedString(string xml, string tableName, bool enforceConstraints, string columnName, char token, IEventLog eventLog)
        {
	        var dataSet = ConvertXmlStringToDataSet(xml, enforceConstraints);
            if (dataSet == null)
            {
                if(eventLog != null) eventLog.LogEvent(1, "ERROR Invalid xml");
                return string.Empty;
            }
            if (dataSet.Tables.Contains(tableName) == false)
            {
                if (eventLog != null) eventLog.LogEvent(1, "ERROR Table not part of dataset");
                return string.Empty;
            }

            var dataTable = dataSet.Tables[tableName];
            return DataTableColumnToDelimitedString(dataTable, columnName, token, eventLog);
        }

	    /// <summary>
	    /// Returns a delimited string of the column values for all rows in a table
	    /// </summary>
	    /// <param name="dataTable">DataTbale object</param>
	    /// <param name="columnName">column to convert</param>
	    /// <param name="token">token to use as delimiter between rows</param>
	    /// <param name="eventLog"></param>
	    /// <returns>Delimited string representation of data table</returns>
	    /// <remarks>(NEW in 2.0.0.4)</remarks>
	    public static string DataTableColumnToDelimitedString(DataTable dataTable, string columnName, char token, IEventLog eventLog)
        {
            var result = new StringBuilder();
            foreach (DataRow row in dataTable.Rows)
            {
                if (result.Length > 0) result.Append(token);
                result.Append(row[columnName]);
            }

            return result.ToString();
        }

        #endregion

        /// <summary>
	    /// Copies the contents of a table in a dataset to another table in another dataset. If table exists and overwrite is true will delete all records and copy the new ones.
	    /// Otherwise will append rows.
	    /// </summary>
	    /// <param name="sourceDataSet"></param>
	    /// <param name="sourceTableName"></param>
	    /// <param name="destinationDataSet"></param>
	    /// <param name="destinationTableName"></param>
	    /// <param name="overwrite"></param>
	    /// <returns>True if table was copied</returns>
        /// <remarks>New in 2.0.0.3</remarks>
	    public static bool CopyTable(DataSet sourceDataSet, string sourceTableName, DataSet destinationDataSet,
	        string destinationTableName, bool overwrite)
        {
            bool[] result = {true};
            if (!sourceDataSet.Tables.Contains(sourceTableName)) return false;
            var sourceTable = sourceDataSet.Tables[sourceTableName];

            // If overwriting and the table exists in the destination then clear it
            if (overwrite) if (destinationDataSet.Tables.Contains(destinationTableName)) destinationDataSet.Tables[destinationTableName].Clear();

            foreach (var sourceRow in sourceTable.Rows.Cast<DataRow>().Where(sourceRow => result[0]))
            {
                result[0] = InsertRow(destinationDataSet, destinationTableName, sourceTable.Columns, sourceRow);
            }

            return result[0];
        }

        /// <summary>
        /// Returns true if the value for a column is equal in both rows supplied.
        /// (NEW in 2.0.0.2)
        /// </summary>
        /// <param name="firstRow">DataRow to compare</param>
        /// <param name="secondRow">Another DataRow to compare</param>
        /// <param name="columnName">name of column to compare</param>
        /// <returns>true if the value for a column is equal in both rows supplied</returns>
        public static bool DataRowColumnValueByNameEqual(DataRow firstRow, DataRow secondRow, string columnName)
        {
            if (firstRow.Table.Columns.Contains(columnName) == false || secondRow.Table.Columns.Contains(columnName) == false) return false;
            return (firstRow[columnName].ToString() == secondRow[columnName].ToString());
        }
    }
}
