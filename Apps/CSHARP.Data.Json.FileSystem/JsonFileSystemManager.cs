/********************************************************************************
 * CSHARP JSON File System Library - General Elements used to manipulate JSON data stored on the file system
 * 
 * LICENSE: Free to use provided details on fixes and/or extensions emailed to 
 *      chris.williams@readwatchcreate.com
********************************************************************************/

using Cms.Data.Json;
using CSHARP.Diagnostics;
using CSHARP.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CSHARP.Data.Json.FileSystem
{
    /// <summary>
    /// Data Access Layer for reading and writing Json Files to the File System
    /// </summary>
    public class JsonFileSystemManager
    {

        /// <summary>
        /// This is the full path to the folder containing all Json Item Files
        /// </summary>
        public string DefaultJsonRootFolder { get; set; }

        /// <summary>
        /// If cache enabled we need to assign type of item we are caching
        /// </summary>
        public Type ItemType { get; set; }

        #region Json Item File Cache Related

        /// <summary>
        /// If true, reads will come from the cache
        /// </summary>
        public bool EnableReadCache
        {
            get { return _readCachEnabled; }
            set
            {
                // Disable cache so clear it
                if (value == false) _cachedJsonItems = null;

                _readCachEnabled = (value == true) ? LoadJsonItemFilesToCache(ItemType, null) : false;
            }
        }
        private bool _readCachEnabled = false;

        /// <summary>
        /// Cache containing Json Items
        /// </summary>
        protected List<JsonItemFile> _cachedJsonItems = null;

        /// <summary>
        /// Ceched Json Items Dictionary or null if not cached
        /// </summary>
        public List<JsonItemFile> CachedJsonItems {  get { return _cachedJsonItems; } }

        /// <summary>
        /// Last time the Json Item cache was loaded
        /// </summary>
        protected DateTime _cacheLastLoadedDateTime { get; set; }

        /// <summary>
        /// Flushes the cache, forceing a reload
        /// </summary>
        /// <returns></returns>
        public bool FlushCache(Type itemType, IEventLog eventLog)
        {
            return LoadJsonItemFilesToCache(itemType, eventLog);
        }

        /// <summary>
        /// Gets a JsonItemFile object for the item passed in
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemType"></param>
        /// <param name="itemId"></param>
        /// <param name="itemName"></param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public JsonItemFile GetJsonItemFileForItem(object item, string itemId, Type itemType, IEventLog eventLog)
        {
            string jsonFileName = itemId.Replace("{", "").Replace("-", "").Replace("}", "");
            return new JsonItemFile()
            {
                ItemType = itemType.Name,
                JsonFileName = jsonFileName,
                UniqueFileName = jsonFileName,
                JsonContent = JsonHelper.SerializeJsonObject(item, itemType, eventLog),
                RelativePathBeneathTable = jsonFileName               
            };
        }

        /// <summary>
        /// Loads Json Items into Cache using default Json Root Folder and Item Type Name
        /// </summary>
        /// <returns></returns>
        public bool LoadJsonItemFilesToCache(Type itemType, IEventLog eventLog)
        {
            if (string.IsNullOrEmpty(DefaultJsonRootFolder))
            {
                if(eventLog != null) eventLog.LogEvent(0, "JsonFileSystemManager.EnableReadCache - DefaultJsonRootFolder is required to enable caching");
                return false;
            }

            _cachedJsonItems = LoadAllJsonFileItems(DefaultJsonRootFolder, itemType.Name, eventLog);
            _cacheLastLoadedDateTime = DateTime.Now;
            return true;
        }

        #endregion

        #region Get/Insert Item Related

        /// <summary>
        /// Inserts an item to the Json File System
        /// </summary>
        /// <param name="jsonRootFolder">Full path to the folder containing all Json Item Files</param>
        /// <param name="item">Item to write to file system</param>
        /// <param name="itemType">Type of item to write. Pass using typeof()</param>
        /// <param name="itemId">Unique id for item</param>
        /// <param name="itemName">Name of item</param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public bool InsertItem(string jsonRootFolder, object item, Type itemType, string itemId, string itemName, IEventLog eventLog)
        {
            try
            {
                bool priorEnableReadCache = EnableReadCache;
                EnableReadCache = false;

                // Load the assets and then add the asset and then save it.
                var jsonFileSystemManager = new JsonFileSystemManager();
                var items = jsonFileSystemManager.LoadAllJsonFileItems(jsonRootFolder, itemType.Name, eventLog);

                var jsonFileItem = GetJsonItemFileForItem(item, itemId, itemType, eventLog);
                items.Add(jsonFileItem);

                bool returnValue = jsonFileSystemManager.SaveAllJsonFileItems(jsonRootFolder, itemType.Name, items, eventLog);
                EnableReadCache = priorEnableReadCache;
                return returnValue;
            }
            catch (Exception exception)
            {
                if (eventLog != null) eventLog.LogEvent(0, "ERROR Inserting " + itemType.Name + ": (" + itemId + "," + itemName + ") - " + exception);
                return false;
            }
        }

        /// <summary>
        /// Writes some items to the file system. Skipping existing items
        /// </summary>
        /// <param name="jsonRootFolder"></param>
        /// <param name="itemsToAdd"></param>
        /// <param name="itemType"></param>
        /// <param name="itemId"></param>
        /// <param name="itemName"></param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public bool InsertItems(string jsonRootFolder, List<JsonItemFile> itemsToAdd, Type itemType, bool overwrite, IEventLog eventLog)
        {
            // if we are not overwriting items
            if (overwrite == false) return InsertItems(jsonRootFolder, itemsToAdd, itemType, eventLog);

            // if we are overwriting items
            try
            {
                bool priorEnableReadCache = EnableReadCache;
                EnableReadCache = false;

                // Load the assets
                var jsonFileSystemManager = new JsonFileSystemManager();
                List<JsonItemFile> items = jsonFileSystemManager.LoadAllJsonFileItems(jsonRootFolder, itemType.Name, eventLog);
                List<JsonItemFile> itemsToSave = new List<JsonItemFile>();

                // Ensure all the ones we are adding are in the save list.
                // Only include existing items that are not replaced by our inserts
                foreach (var item in itemsToAdd)
                {
                    // Ensure item filename ends in .json
                    if (item.JsonFileName.EndsWith(".json") == false) item.JsonFileName = item.JsonFileName + ".json";

                    itemsToSave.Add(item);
                }

                // Only include existing items that are not replaced by our inserts
                foreach (var item in items)
                {
                    // Ensure item filename ends in .json
                    if (item.JsonFileName.EndsWith(".json") == false) item.JsonFileName = item.JsonFileName + ".json";

                    if (itemsToSave.FirstOrDefault(x => x.JsonFileName == item.JsonFileName) == null)
                        itemsToSave.Add(item);
                }

                // save everything back to file system.
                bool returnValue = jsonFileSystemManager.SaveAllJsonFileItems(jsonRootFolder, itemType.Name, itemsToSave, eventLog);
                EnableReadCache = priorEnableReadCache;
                return returnValue;
            }
            catch (Exception exception)
            {
                if (eventLog != null)
                {
                    eventLog.LogEvent(0, "ERROR Inserting " + itemType.Name + "s : " + exception);
                }

                return false;
            }
        }
        /// <summary>
        /// Writes some items to the file system. Skipping existing items
        /// </summary>
        /// <param name="jsonRootFolder"></param>
        /// <param name="itemsToAdd"></param>
        /// <param name="itemType"></param>
        /// <param name="itemId"></param>
        /// <param name="itemName"></param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public bool InsertItems(string jsonRootFolder, List<JsonItemFile> itemsToAdd, Type itemType, IEventLog eventLog)
        {
            try
            {
                bool priorEnableReadCache = EnableReadCache;
                EnableReadCache = false;

                // Load the assets and then add the asset and then save it.
                var jsonFileSystemManager = new JsonFileSystemManager();
                List<JsonItemFile> items = jsonFileSystemManager.LoadAllJsonFileItems(jsonRootFolder, itemType.Name, eventLog);
                foreach(var item in itemsToAdd)
                {
                    if(items.FirstOrDefault(x => x.JsonFileName == item.JsonFileName) == null)
                        items.Add(item);
                }

                bool returnValue = jsonFileSystemManager.SaveAllJsonFileItems(jsonRootFolder, itemType.Name, items, eventLog);
                EnableReadCache = priorEnableReadCache;
                return returnValue;
            }
            catch (Exception exception)
            {
                if (eventLog != null)
                {
                    eventLog.LogEvent(0, "ERROR Inserting " + itemType.Name + "s : " + exception);
                }

                return false;
            }
        }

        #endregion

        #region Serialization Related

        /// <summary>
        /// Loads all the Json Items stored on file system to object list
        /// </summary>
        /// <param name="jsonRootFolder"></param>
        /// <param name="iteTypeName"></param>
        /// <param name="itemType"></param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        /// <remarks>Loading with this method makes it not possible to re-save to file system. Use LoadAllJsonFileItems instead of you wish to resave to file system.</remarks>
        public List<object> LoadAllJsonFileItemsToObjects(string jsonRootFolder, string itemTypeName, Type itemType, IEventLog eventLog)
        {
            List<object> objects = new List<object>();

            // Use directory to get all files beneath a folder and subfolders
            FileInfo[] filePaths = FileHelper.GetFilteredFileListForDirectory(jsonRootFolder + "/" + itemTypeName, "*.json", true);

            foreach (var filePath in filePaths)
            {
               objects.Add(JsonHelper.DeserializeJsonObject(TextFileHelper.ReadContents(filePath.ToString()), itemType, eventLog));
            }

            return objects;
        }

        /// <summary>
        /// Loads all the Json Items into a dictionary containing item name and JSON Content 
        /// </summary>
        /// <param name="jsonRootFolder"></param>
        /// <param name="itemType"></param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public List<JsonItemFile> LoadAllJsonFileItems(string jsonRootFolder, string itemTypeName, IEventLog eventLog)
        {
            List<JsonItemFile> jsonItems = new List<JsonItemFile>();

            // Make sure the folder exists. If it does not then create it.
            string folderToLoadFrom = FileHelper.EnsureTrailingDirectorySeparator(jsonRootFolder) + itemTypeName;
            if (Directory.Exists(folderToLoadFrom) == false) Directory.CreateDirectory(folderToLoadFrom);

            // Use directory to get all files beneath a folder and subfolders
            FileInfo[] filePaths = FileHelper.GetFilteredFileListForDirectory(FileHelper.EnsureTrailingDirectorySeparator(jsonRootFolder) + itemTypeName, "*.json", true);

            foreach(var filePath in filePaths)
            {
                JsonItemFile jsonItemFile = new JsonItemFile()
                {
                    UniqueFileName = FileHelper.GetFileNameFromFilePath(jsonRootFolder) + Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").Replace("-", ""),
                    JsonFileName = FileHelper.GetFileNameFromFilePath(filePath.FullName),
                    RelativePathBeneathTable = FileHelper.GetFileNameFromFilePath(filePath.FullName),
                    JsonContent = TextFileHelper.ReadContents(filePath.FullName.ToString())  
                };

                jsonItems.Add(jsonItemFile);
            }

            return jsonItems;
        }

        /// <summary>
        /// Savaes all the Json Items in a dictionary to the File System 
        /// </summary>
        /// <param name="jsonRootFolder"></param>
        /// <param name="itemType"></param>
        /// <param name="jsonItems"></param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        /// <remarks>Note: The JSON files end in the file extension .json</remarks>
        public bool SaveAllJsonFileItems(string jsonRootFolder,string itemTypeName, List<JsonItemFile> jsonItems, IEventLog eventLog)
        {
            if(jsonItems == null)
            {
                if (eventLog != null) eventLog.LogEvent(0, "SaveAllJsonFileItems - " + itemTypeName + " - jsonItems is null");
                return false;
            }

            bool returnValue = true;
            foreach (var item in jsonItems)
            {
                try
                {
                    // Note: JSON Files need to end in .json
                    string fullPath = FileHelper.EnsureTrailingDirectorySeparator(jsonRootFolder) + FileHelper.EnsureTrailingDirectorySeparator(itemTypeName) + (item.RelativePathBeneathTable.StartsWith("\\") ? item.RelativePathBeneathTable.Substring(1) : item.RelativePathBeneathTable) + (item.RelativePathBeneathTable.EndsWith(".json") ? "" : ".json");
                    TextFileHelper.WriteContents(fullPath, item.JsonContent, true);
                }
                catch(Exception exception)
                {
                    if (eventLog != null) eventLog.LogEvent(0, "Error SaveAllJsonFileItems: (" + item.JsonFileName + ") " + exception.ToString());
                    returnValue = false;
                }
            }

            return returnValue;
        }

        #endregion
    }
}
