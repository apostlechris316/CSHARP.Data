/********************************************************************************
 * CSHARP Data Object Library 
 * 
 * LICENSE: Free to use provided details on fixes and/or extensions emailed to 
 *          chris.williams@readwatchcreate.com
 ********************************************************************************/

using System.Collections.Generic;

namespace CSHARP.Data.Common
{
    /// <summary>
    /// Stores Data Related to a Custom Field
    /// </summary>
    public interface ICustomField
    {
        /// <summary>
        /// Name of the field
        /// </summary>
        string FieldName { get; set; }
        /// <summary>
        /// Value of the field
        /// </summary>
        string FieldValue { get; set; }
    }
}
