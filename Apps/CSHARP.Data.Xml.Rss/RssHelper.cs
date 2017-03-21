/********************************************************************************
 * CSHARP RSS Data Library - General Elements used to work with RSS Feed data
 * 
 * NOTE: Adapted from Clinch.Xml
 *  
 * LICENSE: Free to use provided details on fixes and/or extensions emailed to 
 *          chris.williams@readwatchcreate.com
 ********************************************************************************/

using CSHARP.Diagnostics;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace CSHARP.Data.Xml.Rss
{
    /// <summary>
    /// RSS Feed Helper Functions
    /// </summary>
    /// <remarks>UPGRADE WARNING: This class uses HttpUtility which requires System.Web. Thus affecting upgrade to .NET Core</remarks>
    public static class RssHelper
    {
        /// <summary>
        /// Returns .net dataset friendly xml based on an rss feed
        /// </summary>
        /// <param name="rssXmlData"></param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        /// <remarks>Adapted from this forum post: http://social.msdn.microsoft.com/Forums/eu/xmlandnetfx/thread/2aad018f-7af1-41c3-bfa1-8691eed0eb38 </remarks>
        public static String RssToXml(string rssXmlData, IEventLog eventLog)
        {
            using (var stringReader = new StringReader(rssXmlData))
            {
                using (var reader = new XmlTextReader(stringReader) { WhitespaceHandling = WhitespaceHandling.None })
                {
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
                                        builder.Append(" " + sName + "=\"" + reader.Value + "\"");
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

                    reader.Close();

                    var sXmlResult = builder.ToString();

                    var dataSet = new DataSet();
                    using (var oStringReader = new StringReader(sXmlResult))
                    {
                        try
                        {
                            dataSet.ReadXml(oStringReader, XmlReadMode.Auto);
                        }
                        catch (Exception exception)
                        {
                            if (eventLog != null) eventLog.LogEvent(0, "RssHelper.RssToXml - ERROR: " + exception.ToString());
                        }

                        oStringReader.Close();
                    }

                    return dataSet.Tables.Count == 0 ? null : dataSet.GetXml();
                }
            }
        }

        /// <summary>
        /// Writes an rss feed to a stream passed in based on a .net friendly dataset passed in
        /// </summary>
        /// <param name="title"></param>
        /// <param name="link"></param>
        /// <param name="description"></param>
        /// <param name="copyright"></param>
        /// <param name="postTableName"></param>
        /// <param name="postItemTitleFieldName"></param>
        /// <param name="postItemDescriptionFieldName"></param>
        /// <param name="postItemUrlFieldName"></param>
        /// <param name="postItemDatePostedFieldName"></param>
        /// <param name="xmlData"></param>
        /// <param name="maxItemsToReturn"></param>
        /// <param name="outputStream"></param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public static bool XmlToRss(String title, String link, String description, String copyright, String postTableName, String postItemTitleFieldName, String postItemDescriptionFieldName, String postItemUrlFieldName, String postItemDatePostedFieldName, String xmlData, Int32 maxItemsToReturn, Stream outputStream, IEventLog eventLog)
        {
            var settings = new XmlWriterSettings {Encoding = Encoding.UTF8, Indent = true};
            var feedWriter = XmlWriter.Create(outputStream, settings);

            feedWriter.WriteStartDocument();

            // These are RSS Tags
            feedWriter.WriteStartElement("rss");
            feedWriter.WriteAttributeString("version", "2.0");

            feedWriter.WriteStartElement("channel");
            feedWriter.WriteElementString("title", title);
            feedWriter.WriteElementString("link", link);
            feedWriter.WriteElementString("description", description);
            feedWriter.WriteElementString("copyright", copyright);

            var posts = DataSetHelper.ConvertXmlStringToDataSet(xmlData, false);

            // Write all Posts in the rss feed
            int[] itemCount = {0};
            foreach (var post in posts.Tables[postTableName].Rows.Cast<DataRow>().Where(post => itemCount[0] <= maxItemsToReturn || maxItemsToReturn == 0))
            {
                feedWriter.WriteStartElement("item");
                feedWriter.WriteElementString("title", post[postItemTitleFieldName].ToString());
                feedWriter.WriteElementString("description", post[postItemDescriptionFieldName].ToString());
                feedWriter.WriteElementString("link", post[postItemUrlFieldName].ToString());
                feedWriter.WriteElementString("pubDate", post[postItemDatePostedFieldName].ToString());
                feedWriter.WriteEndElement();
                itemCount[0]++;
            }

            // Close all open tags tags
            feedWriter.WriteEndElement();
            feedWriter.WriteEndElement();
            feedWriter.WriteEndDocument();
            feedWriter.Flush();
            feedWriter.Close();

            return true;
        }

        /// <summary>
        /// Converts an xml list of items into an RSS feed
        /// </summary>
        /// <param name="title"></param>
        /// <param name="link"></param>
        /// <param name="description"></param>
        /// <param name="copyright"></param>
        /// <param name="postTableName"></param>
        /// <param name="postItemTitleFieldName"></param>
        /// <param name="postItemDescriptionFieldName"></param>
        /// <param name="postItemUrlFieldName"></param>
        /// <param name="postItemDatePostedFieldName"></param>
        /// <param name="xmlData"></param>
        /// <param name="maxItemsToReturn"></param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public static String XmlToRss(String title, String link, String description, String copyright, String postTableName, String postItemTitleFieldName, String postItemDescriptionFieldName, String postItemUrlFieldName, String postItemDatePostedFieldName, String xmlData, Int32 maxItemsToReturn, IEventLog eventLog)
        {
            var builder = new StringBuilder();
            var settings = new XmlWriterSettings {Encoding = Encoding.UTF8, Indent = true};
            var feedWriter = XmlWriter.Create(builder, settings);

            feedWriter.WriteStartDocument();

            // These are RSS Tags
            feedWriter.WriteStartElement("rss");
            feedWriter.WriteAttributeString("version", "2.0");

            feedWriter.WriteStartElement("channel");
            feedWriter.WriteElementString("title", title);
            feedWriter.WriteElementString("link", link);
            feedWriter.WriteElementString("description", description);
            feedWriter.WriteElementString("copyright", copyright);

            var posts = DataSetHelper.ConvertXmlStringToDataSet(xmlData, false);

            // Write all Posts in the rss feed
            int[] itemCount = {0};
            foreach (var post in posts.Tables[postTableName].Rows.Cast<DataRow>().Where(post => itemCount[0] <= maxItemsToReturn || maxItemsToReturn == 0))
            {
                feedWriter.WriteStartElement("item");
                feedWriter.WriteElementString("title", post[postItemTitleFieldName].ToString());
                feedWriter.WriteElementString("description", post[postItemDescriptionFieldName].ToString());
                feedWriter.WriteElementString("link", post[postItemUrlFieldName].ToString());
                feedWriter.WriteElementString("pubDate", post[postItemDatePostedFieldName].ToString());
                feedWriter.WriteEndElement();
                itemCount[0]++;
            }

            // Close all open tags tags
            feedWriter.WriteEndElement();
            feedWriter.WriteEndElement();
            feedWriter.WriteEndDocument();
            feedWriter.Flush();
            feedWriter.Close();

            return builder.ToString();
        }
    }
}