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
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace CSHARP.Data.Json
{
    /// <summary>
    /// Helps serialze to JSON and Deserialize to XML
    /// V1.0.0.1 - Namspace and class name changed to proper case
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Sserialize object to a JSON stream.  
        /// </summary>
        /// <param name="objectToSerialize"></param>
        /// <param name="objectType"></param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        /// <remarks>NEW in v1.0.0.2<br/>
        /// Adapted from https://msdn.microsoft.com/en-us/library/bb412179(v=vs.110).aspx </remarks>
        public static string SerializeJsonObject(object objectToSerialize, System.Type objectType, IEventLog eventLog)
        {
            //Create a stream to serialize the object to.  
            MemoryStream ms = new MemoryStream();

            // Serializer the User object to the stream.  
            DataContractJsonSerializer ser = new DataContractJsonSerializer(objectType);
            ser.WriteObject(ms, objectToSerialize);
            byte[] json = ms.ToArray();
            ms.Close();
            return Encoding.UTF8.GetString(json, 0, json.Length);
        }

        /// <summary>
        /// Deserialize a JSON COntent to anobject.  
        /// </summary>
        /// <param name="json"></param>
        /// <param name="objectType"></param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        /// <remarks>NEW in v1.0.0.2<br/>
        /// Adapted from https://msdn.microsoft.com/en-us/library/bb412179(v=vs.110).aspx </remarks>
        public static object DeserializeJsonObject(string json, System.Type objectType, IEventLog eventLog)
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            DataContractJsonSerializer ser = new DataContractJsonSerializer(objectType);
            object deserializedUser = ser.ReadObject(ms);
            ms.Close();
            return deserializedUser;
        }

        /// <summary>
        /// Converts a JSON string to XML
        /// </summary>
        /// <param name="json">String containing valid JSON</param>
        /// <param name="deserializeRootElementName"></param>
        /// <param name="eventLog"></param>
        /// <returns>string containing xml</returns>
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
        /// <returns>string containing JSON</returns>
        public static string XmlToJson(string xml, IEventLog eventLog)
        {
            // To convert an XML node contained in string xml into a JSON string   
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return JsonConvert.SerializeXmlNode(doc);
        }
    }
}
