/********************************************************************************
 * CSHARP Common Data Object Library 
 * 
 * LICENSE: Free to use provided details on fixes and/or extensions emailed to 
 *          chris.williams@readwatchcreate.com
 ********************************************************************************/

using System.Collections.Generic;

namespace CSHARP.Data.Common
{
    /// <summary>
    /// Data Transfer Object for a custom field
    /// </summary>
    public class CustomField
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        public CustomField(string fieldName, string fieldValue) { FieldName = fieldName; FieldValue = fieldValue; }

        /// <summary>
        /// Name of the field
        /// </summary>
        public string FieldName { get; set; }
        /// <summary>
        /// Value of the field
        /// </summary>
        public string FieldValue { get; set; }

        public override string ToString()
        {
            return string.Format("<CustomField><FieldName>{0}</FieldName><FieldValue>{1}</FieldValue></CustomField>");
        }
    }
}
