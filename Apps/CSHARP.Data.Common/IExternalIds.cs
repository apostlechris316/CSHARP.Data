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
    /// List of Ids of object in external system
    /// </summary>
    public interface IExternalIds
    {
        /// <summary>
        /// Stores a list of the external Ids related to this entity
        /// </summary>
        Dictionary<string, IExternalId> ExternalIds { get; set; }
    }
}
