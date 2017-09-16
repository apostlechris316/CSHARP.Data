/********************************************************************************
 * CSHARP Data Object Library 
 * 
 * LICENSE: Free to use provided details on fixes and/or extensions emailed to 
 *          chris.williams@readwatchcreate.com
 ********************************************************************************/

namespace CSHARP.Data.Common
{
    /// <summary>
    /// Interface implemented by object storing Id of object in external system
    /// </summary>
    public class IExternalId
    {
        /// <summary>
        /// Unique Name Object Type from External Source
        /// </summary>
        string SourceObjectType { get; set; }

        /// <summary>
        /// Unique Id Representing the object in the external source
        /// </summary>
        string Id { get; set; }
    }
}
