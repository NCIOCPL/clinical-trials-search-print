using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NCI.Util
{
    public static class Strings
    {


        #region String Manipulation Methods


        /// <summary>
        /// Returns a Trimmed String passed in as an object, null if object is null
        /// </summary>
        /// <param name="obj">The object to be trimmed</param>
        /// <returns>The trimmed string if it != null or empty, otherwise returns null</returns>
        public static string Clean(object obj)
        {
            return Strings.Clean(obj, false);
            
        }

        /// <summary>
        /// Returns a Trimmed String passed in as an object, null if object is null
        /// </summary>
        /// <param name="obj">The object to be trimmed</param>
        /// <param name="preserveEmptyStrings">determines if EmptryStrings shold be preserved</param>
        /// <returns>The trimmed string if it != null or empty, otherwise returns null</returns>
        public static string Clean(object obj, bool preserveEmptyStrings)
        {
            return Strings.Clean(obj, null, preserveEmptyStrings);
            
        }
        /// <summary>
        /// Returns a Trimmed String passed in as an object, null if object is null
        /// </summary>
        /// <param name="obj">The string to be trimmed</param>
        /// <param name="defValue">the default value to be returned incase trim fails</param>
        /// <returns>The trimmed string if it != null or empty, otherwise returns defValue</returns>
        public static string Clean(object obj, string defValue)
        {
            return Strings.Clean(obj, defValue, false);
            
        }

        /// <summary>
        /// Returns a Trimmed String passed in as an object, null if object is null
        /// </summary>
        /// <param name="obj">The string to be trimmed</param>
        /// <param name="defValue">the default value to be returned incase trim fails</param>
        /// <param name="preserveEmptyStrings">determines if EmptryStrings shold be preserved</param>
        /// <returns>The trimmed string if it != null or empty, otherwise returns defValue</returns>
        public static string Clean(object obj, string defValue,bool preserveEmptyStrings)
        {
            string s = defValue;

            if (obj != null)
            {
                s = obj.ToString().Trim();

                if (!preserveEmptyStrings && (s == string.Empty))
                    s = defValue;

            }
            return s;
            
            
            
        }

        /// <summary>
        /// Wraps the object and returns the string
        /// </summary>
        /// <param name="obj">object,charwidth and wrapchar</param>
        /// <param name="charWidth">charwidth</param>
        /// <param name="wrapChar">wrapchar</param>
        /// <returns>the wrapped string</returns>
        public static string Wrap(object obj, int charWidth, string wrapChar)
        {
            string source = string.Empty;
            try
            {
                source = obj.ToString();
                int index = charWidth;

                while (index < source.Length)
                {
                    source = source.Insert(index, wrapChar);
                    index += charWidth + wrapChar.Length;
                }
            }
            catch (System.Exception ex)
            {
                if (ex is NullReferenceException || ex is ArgumentNullException)
                   throw new NCIStringConversionFailedException("Either object or wrapChar is null, please check input", ex);
            }


            return source;
            
        }
        /// <summary>
        /// Converts the object to an array of strings assuming the original object is a string representing a comma-separated list of items.
        /// </summary>
        /// <param name="obj">object needed to be converted to an array of strings</param>
        /// <returns>the array of strings or null if null argument</returns>
        public static string[] ToStringArray(object obj)
        {
            return ToStringArray(obj, ",", false);
        }

        /// <summary>
        /// Converts the object to an array of strings
        /// </summary>
        /// <param name="obj">object needed to be converted to an array of strings</param>
        /// <param name="separator">the separator</param>
        /// <returns>the array of strings or null if null argument</returns>
        public static string[] ToStringArray(object obj, string separator)
        {
            return ToStringArray(obj, separator, false);

        }
        /// <summary>
        /// Converts the object to an array of strings
        /// </summary>
        /// <param name="obj">object needed to be converted to an array of strings, along with the throwerror flag, for invalid and null objects</param>
        /// <param name="throwError">the error to be thrown in case of an invalid object</param>
        /// <returns>the array of strings or null if null argument</returns>
        public static string[] ToStringArray(object obj, bool throwError)
        {
            return ToStringArray(obj, ",", throwError);

        }
        /// <summary>
        /// Converts the object to an array of strings
        /// </summary>
        /// <param name="obj">object needed to be converted to an array of strings</param>
        /// <param name="throwError">the error to be thrown in case of an invalid object</param>
        /// <param name="separator">the separator</param>
        /// <returns>the array of strings or null if null argument</returns>
        public static string[] ToStringArray(object obj, string separator, bool throwError)
        {
            
            string[] resultstarr = new string[] { };

            //Check obj for null, clean it, then do the other stuff

            if (obj == null)
            {
                if (throwError)
                    throw new NCIStringConversionFailedException("Null Object passed into method");
                else
                    return null;//why does this not return an empty array like "" or ",,,"?
            }

            try
            {
                string s = obj.ToString();

           
                resultstarr = s.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < resultstarr.Length; i++)
                {
                    resultstarr[i] = resultstarr[i].Trim();
                }
                return resultstarr;
            }
            catch (Exception ex)
            {
                if (throwError)
                    throw new NCIStringConversionFailedException("Object contains null Elements", ex);
                else
                    return null;//
               
            }
        }

        public static ArrayList ToArrayListOfInts(string val, char separator)
        {
            ArrayList alList = new ArrayList();

            if (val != null)
            {
                foreach (string strItem in val.Split(separator))
                {
                    alList.Add(Strings.ToInt(strItem));
                }
            }

            return alList;
        }

        /// <summary>
        /// Convert a delimited list of values into an array of strings, removing any empty values.
        /// </summary>
        /// <param name="val">String containing the values to be separated.</param>
        /// <param name="separator">The character separating individual values</param>
        /// <returns>An array of strings.</returns>
        /// <remarks>ToListOfTrimmedStrings is guaranteed to return non-null.  If val is null, an empty array
        /// is returned.  All empty strings are removed from the final array.</remarks>
        public static String[] ToListOfTrimmedStrings(String val, char separator)
        {
            string[] returnList;

            // Coallesce val into an array of strings.
            String[] typeArr = string.IsNullOrEmpty(val) ? new String[] { } : val.Split(separator);

            // Trim the individual strings, removing any empty entries along the way.
            if (typeArr.Length > 0)
            {
                List<string> convert = new List<string>(typeArr.Length);
                Array.ForEach(typeArr, item =>
                {
                    item = item.Trim();
                    if (!string.IsNullOrEmpty(item))
                        convert.Add(item);
                });
                returnList = convert.ToArray();
            }
            else
                returnList = new String[] { };

            return returnList;
        }
       
        #endregion

        #region Converter Functions

        /*
         * 
         * Below is a list of the types we convert, 
         * the name of the methods,
         * and thier default values
         * int : ToInt -1
         * long : ToLong -1
         * bool : ToBoolean false
         * UInt16: ToUInt16 0
         * DateTime: ToDateTime new DateTime(0)
         * Guid: ToGuid Guid.Empty
         * 
         * All converters should have 4 overrides, as show below
         */

        #region Integer Converter Functions
        /// <summary>
        /// Converts an Object to Int
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <returns>The integer if object is != null, otherwise throws an exception</returns>
        public static int ToInt(object obj)
        {
            return ToInt(obj, -1, false);
        }

        /// <summary>
        /// Converts an Object to Int
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The integer if object is != null, otherwise throws an exception</returns>
        public static int ToInt(object obj, bool throwError)
        {
            return ToInt(obj, -1, throwError);
        }

        /// <summary>
        /// Converts an Object to Int
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="defValue">defValue siginfies the default value for the int returned</param>
        /// <returns>The integer if object is != null, otherwise returns the default value</returns>
        public static int ToInt(object obj, int defValue)
        {
            return ToInt(obj, defValue, false);
        }

        /// <summary>
        /// Converts an Object to Int
        /// </summary>
        /// <param name="obj">The object to be converted, defValue and throwError</param>
        /// <param name="defValue">defValue siginfies the default value for the int returned</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The integer if object is != null, error if throwError is true,default value if throwError is false</returns>
        private static int ToInt(object obj, int defValue, bool throwError)
        {
            int i = defValue;

            if (obj != null)
            {
                try
                {
                    i = Convert.ToInt32(obj);
                }
                catch (Exception e)
                {
                    if (throwError)
                    {
                        throw new NCIStringConversionFailedException("The object obj is not a valid int", e);
                    }
                }
            }
            else
            {
                if (throwError)
                    throw new NCIStringConversionFailedException("The object obj is null");
            }

            return i;
           
        }
        /// <summary>
        /// Converts an Object to array of Ints
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <returns>The array of integers if object is != null</returns>
        public static int[] ToIntArray(Object obj)
        {
            return ToIntArray(obj, ",", false);
        }
        /// <summary>
        /// Converts an Object to array of Ints
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="separator">the separator</param>
        /// <returns>The array of integers if object is != null</returns>
        public static int[] ToIntArray(Object obj, string separator)
        {
            return ToIntArray(obj, separator, false);
        }
        /// <summary>
        /// Converts an Object to array of Ints
        /// </summary>
        /// <param name="obj">The object to be converted, throwerror determines if error to be thrown for invalid or null object</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The array of integers if object is != null</returns>
        public static int[] ToIntArray(Object obj, bool throwError)
        {
            return ToIntArray(obj, ",", throwError);
        }
        /// <summary>
        /// Converts an Object to array of Ints
        /// </summary>
        /// <param name="obj">The object to be converted,along with the separator,throwerror determines if error to be thrown for invalid or null object</param>
        /// <param name="separator">the separator</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The array of integers if object is != null</returns>
        public static int[] ToIntArray(Object obj, string separator, bool throwError)
        {
            string[] strArr = ToStringArray(obj, separator, throwError);
            return ToIntArray(strArr, throwError);
        }
        /// <summary>
        /// Converts an array of strings to array of Ints
        /// </summary>
        /// <param name="starr">The array of strings to be converted</param>
        /// <returns>The array of integers if object is != null</returns>
        public static int[] ToIntArray(string[] starr)
        {
            return ToIntArray(starr, false);
        }
        /// <summary>
        /// Converts an array of strings to array of Ints
        /// </summary>
        /// <param name="starr">The array of strings to be converted</param>
        /// <param name="separator">the separator</param>
        /// <returns>The array of integers if object is != null</returns>
        //public static int[] ToIntArray(string[] starr, string separator)
        //{
        //    return ToIntArray(starr, false);
        //}

        /// <summary>
        /// Converts an Array of Strings to an Array of Integers
        /// </summary>
        /// <param name="starr">The array of strings to be converted</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The array of integers if starr is != null,error if throwError is true</returns>
        public static int[] ToIntArray(string[] starr, bool throwError)
        {
            int[] intarr;
            try
            {
                intarr = Array.ConvertAll(starr, new Converter<string, int>(delegate(string s) { return ToInt(s, throwError); }));
            }
            catch (Exception ex)
            {
                if (throwError)
                {
                    if (ex is NCIStringConversionFailedException)
                        throw;
                    else
                        throw new NCIStringConversionFailedException("Failed to convert a null Array", ex);
             
                }
                return null;
            }
            return intarr;
           
        }

        #endregion

        #region Integer Converter Functions
        /// <summary>
        /// Converts an Object to Int
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <returns>The integer if object is != null, otherwise throws an exception</returns>
        public static uint ToUInt(object obj)
        {
            return ToUInt(obj, 0, false);
        }

        /// <summary>
        /// Converts an Object to Int
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The integer if object is != null, otherwise throws an exception</returns>
        public static uint ToUInt(object obj, bool throwError)
        {
            return ToUInt(obj, 0, throwError);
        }

        /// <summary>
        /// Converts an Object to Int
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="defValue">defValue siginfies the default value for the int returned</param>
        /// <returns>The integer if object is != null, otherwise returns the default value</returns>
        public static uint ToUInt(object obj, uint defValue)
        {
            return ToUInt(obj, defValue, false);
        }

        /// <summary>
        /// Converts an Object to Int
        /// </summary>
        /// <param name="obj">The object to be converted, defValue and throwError</param>
        /// <param name="defValue">defValue siginfies the default value for the int returned</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The integer if object is != null, error if throwError is true,default value if throwError is false</returns>
        private static uint ToUInt(object obj, uint defValue, bool throwError)
        {
            uint i = defValue;

            if (obj != null)
            {
                try
                {
                    i = Convert.ToUInt32(obj);
                }
                catch (Exception e)
                {
                    if (throwError)
                    {
                      
                            throw new NCIStringConversionFailedException("The object obj is not a valid uint", e);
                    }
                }
            }
            else
            {
                if (throwError)
                    throw new NCIStringConversionFailedException("The object obj is null");
            }

            return i;

        }
        /// <summary>
        /// Converts an Object to array of Ints
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <returns>The array of integers if object is != null</returns>
        public static uint[] ToUIntArray(Object obj)
        {
            return ToUIntArray(obj, ",", false);
        }
        /// <summary>
        /// Converts an Object to array of Ints
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="separator">the separator</param>
        /// <returns>The array of integers if object is != null</returns>
        public static uint[] ToUIntArray(Object obj, string separator)
        {
            return ToUIntArray(obj, separator, false);
        }
        /// <summary>
        /// Converts an Object to array of Ints
        /// </summary>
        /// <param name="obj">The object to be converted, throwerror determines if error to be thrown for invalid or null object</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The array of integers if object is != null</returns>
        public static uint[] ToUIntArray(Object obj, bool throwError)
        {
            return ToUIntArray(obj, ",", throwError);
        }
        /// <summary>
        /// Converts an Object to array of Ints
        /// </summary>
        /// <param name="obj">The object to be converted,along with the separator,throwerror determines if error to be thrown for invalid or null object</param>
        /// <param name="separator">the separator</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The array of integers if object is != null</returns>
        public static uint[] ToUIntArray(Object obj, string separator, bool throwError)
        {
            string[] strArr = ToStringArray(obj, separator, throwError);
            return ToUIntArray(strArr, throwError);
        }
        /// <summary>
        /// Converts an array of strings to array of Ints
        /// </summary>
        /// <param name="starr">The array of strings to be converted</param>
        /// <returns>The array of integers if object is != null</returns>
        public static uint[] ToUIntArray(string[] starr)
        {
            return ToUIntArray(starr, false);
        }
        /// <summary>
        /// Converts an array of strings to array of Ints
        /// </summary>
        /// <param name="starr">The array of strings to be converted</param>
        /// <param name="separator">the separator</param>
        /// <returns>The array of integers if object is != null</returns>
        //public static uint[] ToUIntArray(string[] starr, string separator)
        //{
        //    return ToUIntArray(starr, false);
        //}

        /// <summary>
        /// Converts an Array of Strings to an Array of Integers
        /// </summary>
        /// <param name="starr">The array of strings to be converted</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The array of integers if starr is != null,error if throwError is true</returns>
        public static uint[] ToUIntArray(string[] starr, bool throwError)
        {
            uint[] intarr;
            try
            {
                intarr = Array.ConvertAll(starr, new Converter<string, uint>(delegate(string s) { return ToUInt(s, throwError); }));
            }
            catch (Exception ex)
            {
                if (throwError)
                {
                    if (ex is NCIStringConversionFailedException)
                        throw;
                    else
                        throw new NCIStringConversionFailedException("Failed to convert to Array of uints due to invalid input", ex);
                }
                return null;
            }
            return intarr;

        }

        #endregion

        #region Long Converter Functions
        /// <summary>
        /// Converts an Object to long
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <returns>The long value if object is != null, otherwise throws an exception</returns>
        public static long ToLong(object obj)
        {
            return ToLong(obj, -1, false);
            
        }
        /// <summary>
        /// Converts an Object to long
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The long value if object is != null, otherwise throws an exception</returns>
        public static long ToLong(object obj, bool throwError)
        {
            return ToLong(obj, -1, throwError);
            
        }
        /// <summary>
        /// Converts an Object to long
        /// </summary>
        /// <param name="obj">The object to be converted, defValue siginfies the default value for the int returned</param>
        /// <param name="defValue">defValue siginfies the default value for the int returned</param>
        /// <returns>The long if object is != null, otherwise returns the default value</returns>
        public static long ToLong(object obj, long defValue)
        {
            return ToLong(obj, defValue, false);
            
        }
        /// <summary>
        /// Converts an Object to long
        /// </summary>
        /// <param name="obj">The object to be converted, defValue and throwError</param>
        /// <param name="defValue">defValue siginfies the default value for the int returned</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The long value if object is != null, error if throwError is true,default value if throwError is false</returns>
        private static long ToLong(object obj, long defValue, bool throwError)
        {
            long i = defValue;

            if (obj != null)
            {
                try
                {
                    i = Convert.ToInt64(obj);
                }
                catch (Exception e)
                {
                    if (throwError)
                    {
                      
                            throw new NCIStringConversionFailedException("The object obj is not a valid int", e);
                    }
                }
            }
            else
            {
                if (throwError)
                    throw new NCIStringConversionFailedException("The object obj is null");
            }

            return i;
        }

        /// <summary>
        /// Converts an array of strings to an array of longs
        /// </summary>
        /// <param name="starr">The array of strings to be converted</param>
        /// <returns>The array of Longs if starr is != null</returns>
        public static long[] ToLongArray(string[] starr)
        {
            return ToLongArray(starr, false);
        }

        /// <summary>
        /// Converts an Array of Strings to an Array of Integers
        /// </summary>
        /// <param name="starr">The array of strings to be converted</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The array of Longs if starr is != null,error if throwError is true</returns>
        public static long[] ToLongArray(string[] starr, bool throwError)
        {
            long[] longarr;

            try
            {
                longarr = Array.ConvertAll(starr, new Converter<string, long>(delegate(string s) { return ToLong(s, throwError); }));
            }
            catch (Exception ex)
            {
                if (throwError)
                {
                    if (ex is NCIStringConversionFailedException)
                        throw;
                    else
                        throw new NCIStringConversionFailedException("Failed to convert to Array of Longs due to invalid input", ex);
                }
                return null;
            }
            return longarr;
        }

        /// <summary>
        /// Converts an object to an Array of longs
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <returns>The array of Longs</returns>
        public static long[] ToLongArray(Object obj)
        {
            return ToLongArray(obj, ",", false);
        }

        /// <summary>
        /// Converts an object to an Array of longs
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="separator">the separator</param>
        /// <returns>The Array of Longs if obj is != null</returns>
        public static long[] ToLongArray(Object obj, string separator)
        {
            return ToLongArray(obj, separator, false);
        }

        /// <summary>
        /// Converts an object to an Array of longs
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The Array of Longs if obj is != null</returns>
        public static long[] ToLongArray(Object obj, bool throwError)
        {
            return ToLongArray(obj, ",", throwError);
        }

        /// <summary>
        /// Converts an object to an Array of longs
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The Array of Long if obj is != null</returns>
        public static long[] ToLongArray(Object obj, string separator, bool throwError)
        {

            string[] strArr = ToStringArray(obj, separator, throwError);
            return ToLongArray(strArr, throwError);

        }

        #endregion

        #region Float Converter Functions
        /// <summary>
        /// Converts an Object to float
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <returns>The float value if object is != null and is a float, otherwise returns float.NaN.</returns>
        public static float ToFloat(object obj)
        {            
            return ToFloat(obj, float.NaN, false);

        }
        /// <summary>
        /// Converts an Object to float
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The float value if object is != null and is a float. Otherwise if <em>throwError</em> is <c>true</c> 
        /// throws a NCIStringConversionFailedException exception, if <c>false</c> it returns float.NaN. </returns>
        public static float ToFloat(object obj, bool throwError)
        {
            return ToFloat(obj, float.NaN, throwError);

        }
        /// <summary>
        /// Converts an Object to float
        /// </summary>
        /// <param name="obj">The object to be converted, defValue siginfies the default value for the int returned</param>
        /// <param name="defValue">defValue siginfies the default value for the int returned</param>
        /// <returns>The float if object is != null, otherwise returns the default value</returns>
        public static float ToFloat(object obj, float defValue)
        {
            return ToFloat(obj, defValue, false);

        }
        /// <summary>
        /// Converts an Object to float
        /// </summary>
        /// <param name="obj">The object to be converted, defValue and throwError</param>
        /// <param name="defValue">defValue siginfies the default value for the int returned</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The float value if object is != null, error if throwError is true,default value if throwError is false</returns>
        private static float ToFloat(object obj, float defValue, bool throwError)
        {
            float i = defValue;

            if (obj != null)
            {
                try
                {
                    i = float.Parse(obj.ToString());
                }
                catch (Exception e)
                {
                    if (throwError)
                    {
                        throw new NCIStringConversionFailedException("The object obj is not a valid float", e);
                    }
                }
            }
            else
            {
                if (throwError)
                    throw new NCIStringConversionFailedException("The object obj is null");
            }

            return i;
        }

        /// <summary>
        /// Converts an array of strings to an array of floats
        /// </summary>
        /// <param name="starr">The array of strings to be converted</param>
        /// <returns>The array of Floats if starr is != null</returns>
        public static float[] ToFloatArray(string[] starr)
        {

            return ToFloatArray(starr, false);
        }

        /// <summary>
        /// Converts an Array of Strings to an Array of Integers
        /// </summary>
        /// <param name="starr">The array of strings to be converted</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The array of Floats if starr is != null,error if throwError is true</returns>
        public static float[] ToFloatArray(string[] starr, bool throwError)
        {
            float[] floatarr;

            try
            {
                floatarr = Array.ConvertAll(starr, new Converter<string, float>(delegate(string s) { return ToFloat(s, throwError); }));
            }
            catch (Exception ex)
            {
                if (throwError)
                {
                    if (ex is NCIStringConversionFailedException)
                        throw ex;
                    // if (ex is ArgumentNullException || ex is FormatException)
                    //  throw new NCIStringConversionFailedException("Failed to convert to Array of Floats due to invalid input", ex);
                }
                return null;
            }
            return floatarr;
        }

        /// <summary>
        /// Converts an object to an Array of floats
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <returns>The array of Floats</returns>
        public static float[] ToFloatArray(Object obj)
        {
            return ToFloatArray(obj, ",", false);
        }

        /// <summary>
        /// Converts an object to an Array of floats
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="separator">the separator</param>
        /// <returns>The Array of Floats if obj is != null</returns>
        public static float[] ToFloatArray(Object obj, string separator)
        {
            return ToFloatArray(obj, separator, false);
        }

        /// <summary>
        /// Converts an object to an Array of floats
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The Array of Floats if obj is != null</returns>
        public static float[] ToFloatArray(Object obj, bool throwError)
        {
            return ToFloatArray(obj, ",", throwError);
        }

        /// <summary>
        /// Converts an object to an Array of floats
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The Array of Float if obj is != null</returns>
        public static float[] ToFloatArray(Object obj, string separator, bool throwError)
        {
            string[] strArr = ToStringArray(obj, separator, throwError);
            return ToFloatArray(strArr, throwError);
        }

        #endregion

        #region Guid Converter Functions
        /// <summary>
        /// Converts an object to a Guid
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <returns>The Guid value if obj is != null</returns>
        public static Guid ToGuid(object obj)
        {
            return ToGuid(obj, Guid.Empty, false);
        }
        /// <summary>
        /// Converts an object to a Guid
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The Guid value if obj is != null</returns>
        public static Guid ToGuid(object obj, bool throwError)
        {
            return ToGuid(obj, Guid.Empty, throwError);
        }
        /// <summary>
        /// Converts an object to a Guid
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="defValue">defValue siginfies the default value for the Guid returned</param>
        /// <returns>The Guid value if obj is != null</returns>
        public static Guid ToGuid(object obj, Guid defValue)
        {
            return ToGuid(obj, defValue, false);
        }
        /// <summary>
        /// Converts an object to a Guid
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="defValue">defValue siginfies the default value for the Guid returned</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The Guid value if obj is != null</returns>
        private static Guid ToGuid(object obj, Guid defValue, bool throwError)
        {
            Guid i = defValue;

            if (obj != null)
            {
                try
                {
                    i = new Guid(obj.ToString());
                }
                catch (Exception e)
                {
                    if (throwError)
                    {
                      
                            throw new NCIStringConversionFailedException("The object obj is not a valid int", e);
                    }
                }
            }
            else
            {
                if (throwError)
                    throw new NCIStringConversionFailedException("The object obj is null");
            }

            return i;
        }

        /// <summary>
        /// Converts an Array of strings to an Array of Guids
        /// </summary>
        /// <param name="obj">The Array of strings to be converted</param>
        /// <returns>The Array of Guids if Array of strings is != null</returns>
        public static Guid[] ToGuidArray(string[] starr)
        {
            return ToGuidArray(starr, false);
            
        }
        /// <summary>
        /// Converts an Array of strings to an Array of Guids
        /// </summary>
        /// <param name="obj">The Array of strings to be converted</param>
        /// <returns>The Array of Guids if Array of strings is != null</returns>
        public static Guid[] ToGuidArray(string[] starr, bool throwError)
        {
            Guid[] Guidarr;
            try
            {
                Guidarr = Array.ConvertAll(starr, new Converter<string, Guid>(delegate(string s) { return ToGuid(s, throwError); }));
            }
            catch (Exception ex)
            {
                if (throwError)
                {
                    if (ex is NCIStringConversionFailedException)
                        throw ex;
                    //if (ex is ArgumentNullException || ex is FormatException)
                       // throw new NCIStringConversionFailedException("Failed to convert to Array of Guids due to invalid input", ex);
                }
                return null;
            }
            return Guidarr;
                 

        }

        /// <summary>
        /// Converts an object to an Array of Guids
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <returns>The Array of Guids if Array of strings is != null</returns>
        public static Guid[] ToGuidArray(Object obj)
        {
            return ToGuidArray(obj, ",", false);
        }
        /// <summary>
        /// Converts an object to an Array of Guids
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="separator">the separator</param>
        /// <returns>The Array of Guids if Array of strings is != null</returns>
        public static Guid[] ToGuidArray(Object obj, string separator)
        {
            return ToGuidArray(obj, separator, false);
        }
        /// <summary>
        /// Converts an object to an Array of Guids
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The Array of Guids if Array of strings is != null</returns>
        public static Guid[] ToGuidArray(Object obj, bool throwError)
        {
            return ToGuidArray(obj, ",", throwError);
        }
        /// <summary>
        /// Converts an object to an Array of Guids
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="separator">the separator</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The Array of Guids if Array of strings is != null</returns>
        public static Guid[] ToGuidArray(Object obj, string separator, bool throwError)
        {
            //Strings.obj.ToString()
            string[] stringarr = Strings.ToStringArray(obj, separator, throwError);
            return ToGuidArray(stringarr);

        }

        #endregion

        #region DateTime Converter Functions
        /// <summary>
        /// Converts an object to a DateTime
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <returns>The DateTime value is object != null</returns>
        public static DateTime ToDateTime(object obj)
        {
            return ToDateTime(obj, DateTime.MinValue, false);
        }
        /// <summary>
        /// Converts an object to a DateTime
        /// </summary>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <param name="obj">The object to be converted</param>
        /// <returns>The DateTime value is object != null</returns>
        public static DateTime ToDateTime(object obj, bool throwError)
        {
            return ToDateTime(obj, DateTime.MinValue, throwError);
        }
        /// <summary>
        /// Converts an object to a DateTime
        /// </summary>
        /// <param name="defValue">defValue siginfies the default value for the DateTime returned</param>
        /// <returns>The DateTime value is object != null</returns>
        public static DateTime ToDateTime(object obj, DateTime defValue)
        {
            return ToDateTime(obj, defValue, false);
        }
        /// <summary>
        /// Converts an object to a DateTime
        /// </summary>
        /// <param name="defValue">defValue siginfies the default value for the DateTime returned</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The DateTime value is object != null</returns>
        private static DateTime ToDateTime(object obj, DateTime defValue, bool throwError)
        {
            DateTime i = defValue;

            if (obj != null)
            {
                try
                {
                    i = Convert.ToDateTime(obj.ToString());
                }
                catch (Exception e)
                {
                    if (throwError)
                    {

                        throw new NCIStringConversionFailedException("The object obj is not a valid DateTime", e);
                    }
                }
            }
            else
            {
                if (throwError)
                    throw new NCIStringConversionFailedException("The object obj is null");
            }

            return i;
        }

        /// <summary>
        /// Converts an object to an Array of DateTimes
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The Array of DateTimes if Array of strings is != null</returns>
        public static DateTime[] ToDateTimeArray(Object obj, bool throwError)
        {
            return ToDateTimeArray(obj, ",", throwError);
        }
        /// <summary>
        /// Converts an Array of strings to an Array of DateTime Values
        /// </summary>
        /// <returns>The DateTime value if the Array of strings != null</returns>
        public static DateTime[] ToDateTimeArray(Object obj)
        {
            return ToDateTimeArray(obj, false);
        }
        /// <summary>
        /// Converts an Array of strings to an Array of DateTime Values
        /// </summary>
        /// <returns>The DateTime value if the Array of strings != null</returns>
        public static DateTime[] ToDateTimeArray(string[] starr)
        {
            return ToDateTimeArray(starr, false);
        }
        /// <summary>
        /// Converts an Array of strings to an Array of DateTime Values
        /// </summary>
        /// <returns>The DateTime value if the Array of strings != null</returns>
        public static DateTime[] ToDateTimeArray(string[] starr, bool throwError)
        {
            DateTime[] datetimearr;
            try
            {
                datetimearr = Array.ConvertAll(starr, new Converter<string, DateTime>(delegate(string s) { return ToDateTime(s, throwError); }));
            }
            catch (Exception ex)
            {
                if (throwError)
                {                    
                   // if (ex is ArgumentNullException || ex is FormatException)
                      //  throw new NCIStringConversionFailedException("Failed to convert to Array of DataTimes due to invalid input", ex);
                    if (ex is NCIStringConversionFailedException)
                        throw ex;
                }
                return null;
            }
            return datetimearr;


        }


        /// <summary>
        /// Converts an object to an Array of DateTimes
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <returns>The Array of DateTimes if Array of strings is != null</returns>
        //public static DateTime[] ToDateTimeArray(Object obj)
        //{
        //    return ToDateTimeArray(obj, ",", false);
        //}
        /// <summary>
        /// Converts an object to an Array of DateTimes
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="separator">the separator</param>
        /// <returns>The Array of DateTimes if Array of strings is != null</returns>
        //public static DateTime[] ToDateTimeArray(Object obj, string separator)
        //{
        //    return ToDateTimeArray(obj, separator, false);
        //}
       
        /// <summary>
        /// Converts an object to an Array of DateTimes
        /// </summary>
        /// <param name="obj">The object to be converted</param>
        /// <param name="separator">the separator</param>
        /// <param name="throwError">throwError signifies if error should be thrown incase of invalid object</param>
        /// <returns>The Array of DateTimes if Array of strings is != null</returns>
        public static DateTime[] ToDateTimeArray(Object obj, string separator, bool throwError)
        {
            string[] strArr = ToStringArray(obj, separator, throwError);
            if (strArr != null)
            {
                return ToDateTimeArray(strArr, throwError);
            }
            else
                return null;
        }


        #endregion


        #region Boolean Converter Functions
        /*
         * 
         * I am doing the boolean because it is slightly different than the rest
         * 
         */
        //public static bool ToBoolean(object obj, bool defValue)
        //{
        //    bool retVal = defValue;

        //    if (obj != null)
        //    {
        //        try
        //        {
        //            string s = obj.ToString().Trim();
        //            //s = s.Trim();

        //            if (s.ToUpper() == "YES")
        //                retVal = true;
        //            else if (s.ToUpper() == "Y")
        //                retVal = true;
        //            else if (s.ToUpper() == "1")
        //                retVal = true;
        //            else if (Convert.ToBoolean(obj.ToString().Trim()))
        //                retVal = true;
        //        }
        //        catch { }
        //    }

        //    return retVal;
        //}

        /// <summary>
        /// Converts an Object to Bool
        /// </summary>
        /// <param name="obj">The object to converted to bool</param>
        /// <returns></returns>
        public static bool ToBoolean(object obj)
        {
            return ToBoolean(obj, false, false);
        }
         
        /// <summary>
        /// Converts an Object to Bool
        /// </summary>
        /// <param name="obj">The object to converted to bool</param>
        /// <param name="defValue">The default value of the boolean value returned from the function, incase the object is not coverted.</param>
        /// <param name="throwError">Determines if an error should be thrown incase of an Exception on converting the object to boolean</param>
        /// <returns></returns>
    
        public static bool ToBoolean(object obj, bool defValue, bool throwError)
        {
            bool retVal = defValue;

           try
            {
                if (obj != null)
                {
               
                    string s = obj.ToString().Trim().ToUpper();
                    //s = s.Trim();

                    if (s == "YES")
                        retVal = true;
                    else if (s == "Y")
                        retVal = true;
                    else if (s == "1")
                        retVal = true;
                    else if (Convert.ToBoolean(obj.ToString().Trim()))
                        retVal = true;
                 }
                 else
                 {
                     if (throwError)
                         throw new NCIStringConversionFailedException("The object obj is not a valid boolean");
                    
                 }
                 
            }
            catch (Exception e)
            {
                if (throwError)
                   {
                         throw new NCIStringConversionFailedException("The object obj is not a valid boolean", e);
                   }
            }
           

            return retVal;
        }
      
        #endregion
        #endregion

        #region String Comparison Functions
        /// <summary>
        /// Determines if a string matches a simple wildcard pattern
        /// </summary>
        /// <param name="stringToMatch">The string to match</param>
        /// <param name="pattern">The match pattern</param>
        /// <param name="ignoreCase">Make the comparison case-insensitive</param>
        /// <returns></returns>
        public static bool StringMatchesPattern(string stringToMatch, string pattern, bool ignoreCase)
        {
                        
            //string to match
            stringToMatch = Strings.Clean(stringToMatch,true); 
            //pattern to match the string again
            pattern = Strings.Clean(pattern,true);

            if ((pattern == null) || (stringToMatch == null))
                throw new ArgumentNullException("Pattern to match or the input string to match against the pattern is null");

            //We are going to ignore casing
            if (ignoreCase)
            {
                stringToMatch = stringToMatch.ToUpper();
                pattern = pattern.ToUpper();
            }

            //There is no wildcard so lets see if the strings match
            if (!pattern.Contains("*"))
            {
                return stringToMatch == pattern;
            }

            //temp variable that will hold the pattern as we move along the while loop 
            string _tmppattern;
            int _index;
            //variable holds the index of the wild card
            int _indexwc;

            if (pattern.CompareTo("*") == 0)
            {
                return true;
            }

            do
            {
                //if pattern starts with Wild card, then we go into the string matching loop between any two wild cards
                if (pattern.StartsWith("*"))
                {
                    //if pattern string contains a second wildcard character
                    if (pattern.Substring(1, pattern.Length - 1).Contains("*"))
                    {
                        //index of the second wildcard character
                        _indexwc = pattern.Substring(1, pattern.Length - 1).IndexOf("*");
                        //temporary pattern from index 1 to the second wildcard character
                        _tmppattern = pattern.Substring(1, _indexwc);

                        if (stringToMatch.Contains(_tmppattern))
                        {
                            _index = stringToMatch.IndexOf(_tmppattern);
                            stringToMatch = stringToMatch.Substring(_index + _tmppattern.Length, stringToMatch.Length - _index - _tmppattern.Length);
                            pattern = pattern.Substring(_indexwc + 1, pattern.Length - _tmppattern.Length - 1);
                        }
                        else
                        {
                            return false;
                        }

                    }
                    else
                    {
                        if (pattern.CompareTo("*") == 0)
                        {
                            return true;
                        }

                        if (stringToMatch.EndsWith(pattern.Substring(1, pattern.Length - 1)))
                            return true;
                        else
                            return false;

                    }


                }
                //if pattern does not start with Wild card, then match the starting string between first alphabet of the pattern and the *
                else
                {
                    if (pattern.Substring(0, pattern.Length).Contains("*"))
                    {
                        _indexwc = pattern.Substring(0, pattern.Length).IndexOf("*");
                        _tmppattern = pattern.Substring(0, _indexwc);
                        if (stringToMatch.StartsWith(_tmppattern))
                        {
                            stringToMatch = stringToMatch.Substring(_tmppattern.Length, stringToMatch.Length - _tmppattern.Length);
                            pattern = pattern.Substring(_indexwc, pattern.Length - _tmppattern.Length);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            } while (pattern.StartsWith("*"));


            return false;
        }

        /// <summary>
        /// Substitutes default string for a null string
        /// </summary>
        /// <param name="val">Test string</param>
        /// <param name="valDefault">Default string</param>
        /// <returns>Test string or default string</returns>
        public static string IfNull(string val, string valDefault)
        {
            if (val == null)
            {
                return valDefault;
            }
            else
            {
                return val;
            }
        }

        #endregion


        #region StripHTMLTags method

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string StripHTMLTags(string text)
        {
            Regex tagExpr = new Regex("<.+?>", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

            return tagExpr.Replace(text, "");
        }

        #endregion

    }

   
}
