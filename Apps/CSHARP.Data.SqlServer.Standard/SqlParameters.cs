/********************************************************************************
 * CSHARP SQL Library - General Elements used to work with SQL Server
 * 
 * NOTE: Adapted from Clinch.Data
 * 
 * LICENSE: Free to use provided details on fixes and/or extensions emailed to 
 *          chris.williams@readwatchcreate.com
 ********************************************************************************/

 using System.Collections.Generic;

namespace CSHARP.Data.SQL.Standard
{
    /// <summary>
    /// Encapsulates set of parameters to pass to stored procedure
    /// </summary>
    public class SqlParameters
    {
        /// <summary>
        /// List of all the parameter names
        /// </summary>
        public List<string> ParameterNames = new List<string>();

        /// <summary>
        /// List of all the parameter values
        /// </summary>
        public List<string> ParameterValues = new List<string>();

        /// <summary>
        /// List of all the parameter types
        /// </summary>
        public List<string> ParameterTypes = new List<string>();

        /// <summary>
        /// Adds a parameter to the lists
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Value for the parameter</param>
        /// <param name="type">Data type of the parameter</param>
        /// <returns></returns>
        public void AppendParameter(string name, string value, string type)
        {
            ParameterNames.Add(name);
            ParameterValues.Add(value);
            ParameterTypes.Add(type);
        }
    }
}
