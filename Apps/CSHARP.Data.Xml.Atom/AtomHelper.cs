/********************************************************************************
 * CSHARP Atom Data Library - General Elements used to work with atom feed data
 * 
 * NOTE: Adapted from Clinch.Xml
 * 
 * LICENSE: Free to use provided details on fixes and/or extensions emailed to 
 *          chris.williams@readwatchcreate.com
 ********************************************************************************/

using System;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using CSHARP.Diagnostics;

namespace CSHARP.Data.Xml.Atom
{
    /// <summary>
    /// ATOM Feed Helper Functions
    /// </summary>
    /// <remarks>NEW in v1.0.0.1</remarks>
    public static class AtomHelper
    {
        /// <summary>
        /// Returns .net dataset friendly xml based on an Atom feed
        /// </summary>
        /// <param name="atomXmlData"></param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        /// <remarks>Adapted from this forum post: http://social.msdn.microsoft.com/Forums/eu/xmlandnetfx/thread/2aad018f-7af1-41c3-bfa1-8691eed0eb38 </remarks>
        public static String AtomToXml(string atomXmlData, IEventLog eventLog)
        {
            var stringReader = new StringReader(atomXmlData);
            var reader = new XmlTextReader(stringReader) {WhitespaceHandling = WhitespaceHandling.None};
            var builder = new StringBuilder();

            var fContinue = reader.Read();
            while (fContinue)
            {
                var sName = reader.Name.Replace(":", "_");

                var isClosed = reader.IsEmptyElement;
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        builder.Append("<" + sName);
                        if (reader.HasAttributes)
                        {
                            while (reader.MoveToNextAttribute())
                            {
                                sName = reader.Name.Replace(":", "_");
                                builder.Append(" " + sName + "=\"" + reader.Value.Replace("&", "&amp;") + "\"");
                            }
                        }
                        if (isClosed)
                            builder.Append("/");
                        builder.Append(">");
                        break;
                    case XmlNodeType.Text:
                        builder.Append(HttpUtility.HtmlEncode(reader.Value));
                        break;
                    case XmlNodeType.EndElement:
                        builder.Append("</" + sName + ">");
                        break;
                }


                fContinue = reader.Read();
            }

            stringReader.Close();
            stringReader.Dispose();

            reader.Close();

            var sXmlResult = "<atom>" + builder + "</atom>";

            var dataSet = new DataSet();
            var oStringReader = new StringReader(sXmlResult);
            try
            {
                dataSet.ReadXml(oStringReader, XmlReadMode.Auto);
            }
            catch (Exception exception)
            {
                if (eventLog != null) eventLog.LogEvent(0, "ERROR Converting Atom To Xml - " + exception);
            }

            oStringReader.Close();
            oStringReader.Dispose();
            reader.Close();
            return dataSet.Tables.Count == 0 ? null : dataSet.GetXml();
        }
   }
}