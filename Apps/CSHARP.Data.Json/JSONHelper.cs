/********************************************************************************
 * CSHARP JSON Library - General Elements used to manipulate JSON data 
 * 
 * NOTE: Adapted from Clinch.NET.JSON
 * LICENSE: Free to use provided details on fixes and/or extensions emailed to 
 *      chris.williams@readwatchcreate.com
********************************************************************************/

using System.Xml;
using Newtonsoft.Json;
using CSHARP.Diagnostics;

namespace CSHARP.Data.Json
{
    /// <summary>
    /// Helps serialze to JSON and Deserialize to XML
    /// V1.0.0.1 - Namspace and class name changed to proper case
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Converts a JSON string to XML
        /// </summary>
        /// <param name="json">String containing valid JSON</param>
        /// <param name="deserializeRootElementName"></param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public static string JsonToXml(string json, string deserializeRootElementName, IEventLog eventLog)
        {
            string returnValue;

            // To convert JSON text contained in string json into an XML node
            if (string.IsNullOrEmpty(deserializeRootElementName))
            {
                var doc = JsonConvert.DeserializeXmlNode(json);
                returnValue = doc.InnerXml;
            }
            else
            {
                var doc = JsonConvert.DeserializeXmlNode(json, deserializeRootElementName);
                returnValue = doc.InnerXml;
            }

            return returnValue;
        }

        /// <summary>
        /// Converts an XML string to JSON
        /// </summary>
        /// <param name="xml">String containing valid XML</param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public static string XmlToJson(string xml, IEventLog eventLog)
        {
            // To convert an XML node contained in string xml into a JSON string   
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return JsonConvert.SerializeXmlNode(doc);
        }
    }
}
