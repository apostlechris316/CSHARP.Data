/********************************************************************************
 * CSHARP JSON File System Library - General Elements used to manipulate JSON data stored on the file system
 * 
 * LICENSE: Free to use provided details on fixes and/or extensions emailed to 
 *      chris.williams@readwatchcreate.com
********************************************************************************/

namespace Cms.Data.Json
{
    /// <summary>
    /// Information about a Json Item stored on the file system
    /// </summary>
    public class JsonItemFile
    {
        /// <summary>
        /// This is generated on load in case there are Filenames in other folders with the same name.
        /// </summary>
        public string UniqueFileName { get; set;  }
        
        /// <summary>
        /// Type of Json Data being stored (used to determine subfolder beneath JsonRoot to store item
        /// </summary>
        public string ItemType { get; set; }

        /// <summary>
        /// Relative path under Json Root Folder + ItemType
        /// </summary>
        public string RelativePathBeneathTable { get; set; }

        /// <summary>
        /// Name of Json File
        /// </summary>
        public string JsonFileName { get; set; }

        /// <summary>
        /// Json encoded content
        /// </summary>
        public string JsonContent { get; set; }
    }
}
