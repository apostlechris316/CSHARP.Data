/********************************************************************************
 * CSHARP Xslt Library - General Elements used to work with xslt
 * 
 * NOTE: Adapted from Clinch.Xml
 *  
 * LICENSE: Free to use provided details on fixes and/or extensions emailed to 
 *          chris.williams@readwatchcreate.com
 ********************************************************************************/

using CSHARP.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Xsl;

namespace CSHARP.Data.Xml.Xsl
{
    /// <summary>
    /// XSL Transform Helper Functions
    /// </summary>
    public static class XslTransformHelper
    {
        /// <summary>
        /// Combines an xsl template and xml text and returns the results.
        /// </summary>
        /// <param name="xslTemplateContent">contents of the xslt template</param>
        /// <param name="xmlContent">xml text content</param>
        /// <param name="xslArguments">collection of arguments to pass to xsl template</param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public static string XslTransform(string xslTemplateContent, string xmlContent, XsltArgumentList xslArguments, IEventLog eventLog)
        {
            string result = string.Empty;

            //Create a XML Document of the Results
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlContent.Replace(" & ", " &amp; ").Replace("&nbsp;", "&#160;"));

            // load xslt to do the transformation
            XslCompiledTransform xsl = new XslCompiledTransform();
            using (StringReader stringReader = new StringReader(xslTemplateContent))
            {
                XmlTextReader textReader = new XmlTextReader(stringReader);
                xsl.Load(textReader);

                // do the transform and return the string
                using (StringWriter results = new StringWriter())
                {
                    xsl.Transform(xmlDocument, xslArguments, results);

                    result = results.ToString().Replace("xmlns:asp=\"remove\"", string.Empty).Replace("xmlns:ComponentArt=\"remove\"", string.Empty).Replace("&lt;", "<")
                                .Replace("&gt;", ">").Replace("&amp;amp;", "&amp;").Replace("&amp;#160;", "&#160;").Replace("&amp;nbsp;", "&nbsp;");
                }
            }

            return result;
        }

        /// <summary>
        /// Combines an xsl template and xml text and returns the results.
        /// </summary>
        /// <param name="xslTemplateFileName">full path to xslt template</param>
        /// <param name="xmlContent">xml text content</param>
        /// <param name="xslArguments">collection of arguments to pass to xsl template</param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public static string XslTransformUsingXslTemplateFile(string xslTemplateFileName, string xmlContent, XsltArgumentList xslArguments, IEventLog eventLog)
        {
            string result = string.Empty;

            //Create a XML Document of the Results
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlContent.Replace(" & ", " &amp; ").Replace("&nbsp;", "&#160;"));

            // load xslt to do the transformation
            XslCompiledTransform xsl = new XslCompiledTransform();
            xsl.Load(xslTemplateFileName);

            // do the transform and return the string
            using (StringWriter results = new StringWriter())
            {
                xsl.Transform(xmlDocument, xslArguments, results);
                result = results.ToString().Replace("xmlns:asp=\"remove\"", string.Empty).Replace("xmlns:ComponentArt=\"remove\"", string.Empty).Replace("&lt;", "<")
                            .Replace("&gt;", ">").Replace("&amp;amp;", "&amp;").Replace("&amp;#160;", "&#160;").Replace("&amp;nbsp;", "&nbsp;");
                results.Close();
            }

            return result;
        }
    }
}
