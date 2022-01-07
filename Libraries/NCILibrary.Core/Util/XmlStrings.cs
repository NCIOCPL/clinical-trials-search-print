using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace NCI.Util
{
    public static class XmlStrings
    {
        /// <summary>
        /// Returns a Trimmed String passed in as an XmlNode, null if the node is null
        /// </summary>
        /// <param name="node">The node to be converted into a string</param>
        /// <returns>The trimmed string if it != null or empty, otherwise returns null</returns>
        public static string Clean(XmlNode node)
        {
            //if node is xmlelement do what u need to do

            //xmlelement innertext
            if (node != null)
            {
                return XmlStrings.Clean(node, null, false);
            }
            else
            {
                return null;
            }
            //xmlattribute  value
        }

        /// <summary>
        /// Trims the contents of an XmlNode and returns a string
        /// </summary>
        /// <param name="node">The node to be converted into a string</param>
        /// <param name="defValue">The string to be returned if the contents of the node is empty</param>
        /// <returns>The trimmed string if it != null or empty, otherwise returns defValue</returns>
        public static string Clean(XmlNode node, string defValue)
        {
            //xmlelement innertext
            if (node != null)
            {
                return XmlStrings.Clean(node, defValue, false);
            }
            else
            {
                return defValue;
            }
        }

        /// <summary>
        /// Trims the contents of an XmlNode and returns a string
        /// </summary>
        /// <param name="node">The node to be converted into a string</param>
        /// <param name="defValue">The string to be returned if the contents of the node is empty</param>
        /// <returns></returns>
        public static string Clean(XmlNode node, bool preserveEmptyStrings)
        {
            //xmlelement innertext
            if (node != null)
            {
                return XmlStrings.Clean(node, null, preserveEmptyStrings);
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// Trims the contents of an XmlNode and returns a string
        /// </summary>
        /// <param name="node">The node to be converted into a string</param>
        /// <param name="defValue">the default value to be returned incase trim fails</param>
        /// <param name="preserveEmptyStrings">determines if EmptryStrings shold be preserved</param>
        /// <returns>The trimmed string if it != null or empty, otherwise returns default value</returns>
        public static string Clean(XmlNode node, string defValue,bool preserveEmptyStrings)
        {
            //if node is null or empty retrun defValue
            string text = string.Empty;
            //xmlelement innertext
            if (node != null)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Element:
                        {
                            if (!ContainsChildXMLElements(node))
                            {
                                text = node.InnerText;
                            }
                            else
                                throw new NCIStringConversionFailedException("Cannot convert an XmlElement that contains another XMLElement");

                        }
                        break;
                    case XmlNodeType.Attribute:
                        {
                            text = node.Value;
                        }
                        break;
                    case XmlNodeType.CDATA:
                        {
                            text = node.Value;
                        }
                        break;
                    default:
                        {
                            throw new NCIStringConversionFailedException("NodeType cannot be converted");
                        }

                }

                return Strings.Clean(text, defValue, preserveEmptyStrings);
            }
            else
            {
                return defValue;
            }
        }

        /// <summary>
        /// Determines if the node contains Child Elements
        /// </summary>
        /// <param name="node">The node to be converted</param>
        /// <returns>true if it contains child elements, false otherwise</returns>
        private static bool ContainsChildXMLElements(System.Xml.XmlNode node)
        {
            if (node.HasChildNodes)
            {
                foreach (XmlNode childnode in node.ChildNodes)
                {
                    if (childnode.NodeType.Equals(XmlNodeType.Element))
                        return true;
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Converts an XmlNode to Int
        /// </summary>
        /// <param name="node">The XmlNode to be converted</param>
        /// <returns>The integer if XmlNode is != null</returns>
        public static int ToInt(XmlNode node)
        {
            return ToInt(node, -1, false);
        }

        /// <summary>
        /// Converts an XmlNode to Int
        /// </summary>
        /// <param name="node">The XmlNode to be converted, throwError signifies if error should be thrown incase of invalid object</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The integer if XmlNode is != null, otherwise throws an exception</returns>
        public static int ToInt(XmlNode node, bool throwError)
        {
            return ToInt(node, -1, throwError);
        }

        /// <summary>
        /// Converts an XmlNode to Int
        /// </summary>
        /// <param name="node">The XmlNode to be converted</param>
        /// <param name="defValue">defValue siginfies the default value for the int returned</param>
        /// <returns>The integer if XmlNode is != null,  otherwise returns the default value</returns>
        public static int ToInt(XmlNode node, int defValue)
        {
            return ToInt(node, defValue, false);
        }

        /// <summary>
        /// Converts an XmlNode to Int
        /// </summary>
        /// <param name="obj">The XmlNode to be converted, defValue and throwError</param>
        /// <param name="defValue">defValue siginfies the default value for the int returned</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The integer if XmlNode is != null,error if throwError is true,default value if throwError is false</returns>

        private static int ToInt(XmlNode node, int defValue, bool throwError)
        {

            string s = null;

            try
            {
                //s = Clean(node);
                s = XmlStrings.Clean(node);

                if (s == null)
                    throw new NCIStringConversionFailedException("Node could not be converted to Int. Please check input Data.");
                else
                    return Convert.ToInt32(s);
            }
            catch (NCIStringConversionFailedException ex)
            {
                if (throwError)
                    throw new NCIStringConversionFailedException("Could not convert NodeType", ex);
                else
                    return defValue;
            }
            catch (FormatException ex)
            {
                if (throwError)
                    throw new NCIStringConversionFailedException("Could not convert NodeType cannot be converted", ex);
                else
                    return defValue;
            }
        }

        /// <summary>
        /// Converts an Object to Int
        /// </summary>
        /// <param name="node">The object to be converted</param>
        /// <returns>The integer if object is != null, otherwise throws an exception</returns>
        public static uint ToUInt(XmlNode node)
        {
            return ToUInt(node, 0, false);
        }

        /// <summary>
        /// Converts an Object to Int
        /// </summary>
        /// <param name="node">The object to be converted</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The integer if object is != null, otherwise throws an exception</returns>
        public static uint ToUInt(XmlNode node, bool throwError)
        {
            return ToUInt(node, 0, throwError);
        }

        /// <summary>
        /// Converts an Object to Int
        /// </summary>
        /// <param name="node">The object to be converted</param>
        /// <param name="defValue">defValue siginfies the default value for the int returned</param>
        /// <returns>The integer if object is != null, otherwise returns the default value</returns>
        public static uint ToUInt(XmlNode node, uint defValue)
        {
            return ToUInt(node, defValue, false);
        }

        /// <summary>
        /// Converts an Object to Int
        /// </summary>
        /// <param name="node">The object to be converted, defValue and throwError</param>
        /// <param name="defValue">defValue siginfies the default value for the int returned</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The integer if object is != null, error if throwError is true,default value if throwError is false</returns>
        public static uint ToUInt(XmlNode node, uint defValue, bool throwError)
        {
            string s = null;

            try
            {
                //s = Clean(node);
                s = XmlStrings.Clean(node);

                if (s == null)
                    throw new NCIStringConversionFailedException("Node could not be converted to UInt. Please check input Data.");
                else
                    return Convert.ToUInt32(s);//converts to 32 bit unsigned int
            }
            catch (NCIStringConversionFailedException ex)
            {
                if (throwError)
                    throw new NCIStringConversionFailedException("Could not convert NodeType", ex);
                else
                    return defValue;
            }
            catch (FormatException ex)
            {
                if (throwError)
                    throw new NCIStringConversionFailedException("Could not convert NodeType cannot be converted", ex);
                else
                    return defValue;
            }
        }

        /// <summary>
        /// Converts an XmlNode to long
        /// </summary>
        /// <param name="node">The XmlNode to be converted</param>
        /// <returns>The long if XmlNode is != null</returns>
        public static long ToLong(XmlNode node)
        {
            return ToLong(node, -1, false);
        }
        /// <summary>
        /// Converts an XmlNode to long
        /// </summary>
        /// <param name="node">The XmlNode to be converted, throwError signifies if error should be thrown incase of invalid object</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The Int64 value if XmlNode is != null, otherwise throws an exception</returns>
        public static long ToLong(XmlNode node, bool throwError)
        {
            return ToLong(node, -1, throwError);
        }
        /// <summary>
        /// Converts an XmlNode to long
        /// </summary>
        /// <param name="node">The XmlNode to be converted, defValue siginfies the default value for the int returned</param>
        /// <param name="defValue">defValue siginfies the default value for the Int64 returned</param>

        /// <returns>The Int64 value if XmlNode is != null,  otherwise returns the default value</returns>
        public static long ToLong(XmlNode node, long defValue)
        {

            return ToLong(node, defValue, false);
        }
        /// <summary>
        /// Converts an XmlNode to long
        /// </summary>
        /// <param name="node">The XmlNode to be converted, defValue and throwError</param>
        /// <param name="defValue">defValue siginfies the default value for the Int64 returned</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The Int64 value if XmlNode is != null,error if throwError is true,default value if throwError is false</returns>
        public static long ToLong(XmlNode node, long defValue, bool throwError)
        {

            string s = null;
            try
            {
                s = XmlStrings.Clean(node);

                if (s == null)
                    throw new NCIStringConversionFailedException("Node could not be converted to Int. Please check input Data.");
                else
                    return Convert.ToInt64(s);
            }
            catch (NCIStringConversionFailedException ex)
            {
                if (throwError)
                    throw new NCIStringConversionFailedException("Could not convert NodeType", ex);
                else
                    return defValue;
            }
            catch (FormatException ex)
            {
                if (throwError)
                    throw new NCIStringConversionFailedException("Could not convert NodeType cannot be converted", ex);
                else
                    return defValue;
            }
        }
        /// <summary>
        /// Converts an XmlNode to a Guid
        /// </summary>
        /// <param name="node">The node to be converted</param>
        /// <returns>The Guid value if obj is != null</returns>
        public static Guid ToGuid(XmlNode node)
        {
            return ToGuid(node, Guid.Empty, false);
        }
        /// <summary>
        /// Converts an XmlNode to a Guid
        /// </summary>
        /// <param name="node">The node to be converted</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The Guid value if obj is != null</returns>
        public static Guid ToGuid(XmlNode node, bool throwError)
        {
            return ToGuid(node, Guid.Empty, throwError);
        }
        /// <summary>
        /// Converts an XmlNode to a Guid
        /// </summary>
        /// <param name="node">The node to be converted</param>
        /// <param name="defValue">defValue siginfies the default value for the Guid returned</param>
        /// <returns>The Guid value if obj is != null</returns>
        public static Guid ToGuid(XmlNode node, Guid defValue)
        {

            return ToGuid(node, defValue, false);
        }
        /// <summary>
        /// Converts an XmlNode to a Guid
        /// </summary>
        /// <param name="node">The object to be converted</param>
        /// <param name="defValue">defValue siginfies the default value for the Guid returned</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The Guid value if obj is != null</returns>
        public static Guid ToGuid(XmlNode node, Guid defValue, bool throwError)
        {

            string s = null;
            try
            {
                s = XmlStrings.Clean(node);


                if (s == null)
                    throw new NCIStringConversionFailedException("Node could not be converted to Guid. Please check input Data.");
                else
                    return new Guid(s);
            }
            catch (NCIStringConversionFailedException ex)
            {
                if (throwError)
                    throw new NCIStringConversionFailedException("Could not convert NodeType ", ex);
                else
                    return defValue;
            }
            catch (FormatException ex)
            {
                if (throwError)
                    throw new NCIStringConversionFailedException("Could not convert NodeType cannot be converted", ex);
                else
                    return defValue;
            }
        }
        /// <summary>
        /// Converts an XmlNode to a DateTime
        /// </summary>
        /// <param name="node"></param>
        /// <returns>The DateTime value is object != null</returns>
        public static DateTime ToDateTime(XmlNode node)
        {
            return ToDateTime(node, new DateTime(), false);
        }
        /// <summary>
        /// Converts an XmlNode to a DateTime
        /// </summary>
        /// <param name="node"></param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The DateTime value if node != null</returns>
        public static DateTime ToDateTime(XmlNode node, bool throwError)
        {
            return ToDateTime(node, new DateTime(), throwError);
        }
        /// <summary>
        /// Converts an XmlNode to a DateTime
        /// </summary>
        /// <param name="node"></param>
        /// <param name="defValue">defValue siginfies the default value for the DateTime returned</param>
        /// <returns>The DateTime value if node != null</returns>
        public static DateTime ToDateTime(XmlNode node, DateTime defValue)
        {

            return ToDateTime(node, defValue, false);
        }
        /// <summary>
        /// Converts an XmlNode to a DateTime
        /// </summary>
        /// <param name="node"></param>
        /// <param name="defValue">defValue siginfies the default value for the DateTime returned</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The DateTime value if node != null</returns>
        public static DateTime ToDateTime(XmlNode node, DateTime defValue, bool throwError)
        {

            string s = null;
            try
            {

                s = XmlStrings.Clean(node);

                if (s == null)
                    throw new NCIStringConversionFailedException("Node could not be converted to DateTime. Please check input Data.");
                else
                    return Convert.ToDateTime(s);
            }
            catch (NCIStringConversionFailedException ex)
            {
                if (throwError)
                    throw new NCIStringConversionFailedException("Could not convert NodeType", ex);
                else
                    return defValue;
            }
            catch (FormatException ex)
            {
                if (throwError)
                    throw new NCIStringConversionFailedException("Could not convert NodeType cannot be converted", ex);
                else
                    return defValue;
            }

        }


    }
}
