using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCI.Core
{

    public static class HashMaster
    {
        const int MULTIPLIER = 69;
        const int MAX_SALTNUM = 999;
        const string HASHSTRING_FIELD = "a";
        const string SALTSTRING_FIELD = "b";
        const string STRING_DELIMITER = "~";



        /// <summary>
        /// Returns obfuscated salt value and  hash code for salterd incoming 
        /// string created by SaltedHash.  Returns two field-value pairs 
        /// for adding to query strings consisting of field “a” that 
        /// contains the hash code and field “b” that contains the 
        /// obfuscated salt value. 
        /// </summary>
        public static string SaltedHashURL(string incoming)
        {
            string hashString, saltString;

            SaltedHash(incoming, out hashString, out saltString);

            return "&" + HASHSTRING_FIELD + "=" + hashString + "&" + SALTSTRING_FIELD + "=" + saltString;

        }

        /// <summary>
        /// Overload for SaltedHashURL that adds a test point 
        /// field-value pair to query string where field contains "TestPoint" 
        /// and value contains incoming testPointText. FOR TESTING.
        /// </summary>
        public static string SaltedHashURL(string incoming, string testPointText)
        {
            return SaltedHashURL(incoming) + "&TestPoint=" + testPointText;
        }

        /// <summary>
        /// Returns obfuscated salt value and  hash code for salterd incoming 
        /// string created by SaltedHash.  Returns a string containing a delimiter,
        /// the hash code for the salted incoming string, a delimiter, and the  
        /// obfuscated salt value. 
        /// </summary>
        public static string SaltedHashString(string incoming)
        {
            string hashString, saltString;

            SaltedHash(incoming, out hashString, out saltString);

            return STRING_DELIMITER + hashString + STRING_DELIMITER + saltString;

        }

        /// <summary>
        /// Returns a compound string containing a prefex and  the obfuscated salt value 
        /// and hash code for salterd incoming string created by SaltedHash.  
        /// Returns a string containing the prefix, a delimiter, the hash code for the 
        /// salted incoming string, a delimiter, and the obfuscated salt value. 
        /// </summary>
        public static string SaltedHashCompoundString(string incoming, string prefix)
        {
            return prefix + SaltedHashString(incoming);
        }


        /// <summary>
        /// Creates obfuscated salted hash code for salted incoming string.  
        /// Returns hashString that contains the hash code and 
        /// saltString that contains the obfuscated salt value. 
        /// </summary>
        public static void SaltedHash(string incoming, out string hashString, out string saltString)
        {
            Random randNum = new Random();

            //Generate random salt number
            int saltNum = randNum.Next(MAX_SALTNUM);
            //Get hashcode from salted incoming 
            int hash = (saltNum.ToString() + incoming).GetHashCode();

            //convert hash code to string 
            hashString = hash.ToString();
            //Obfuscate salt number and convert to string  
            saltString = (saltNum * MULTIPLIER).ToString();

            //Generate random uppercase letter to stuff into hashString
            char randStringHash = (char)((short)'A' + new Random(hash).Next(26));
            //Generate random lowercase letter to stuff into saltString
            char randStringSalt = (char)((short)'a' + new Random(saltNum).Next(26));
            //Insert random uppercase letter into hashString at a random location
            hashString = hashString.Insert(randNum.Next(hashString.Length), randStringHash.ToString());
            //Insert random lowercase lettrer into saltString at a random location 
            saltString = saltString.Insert(randNum.Next(saltString.Length), randStringSalt.ToString());

        }

        /// <summary>
        /// Unobfuscates hash code string and salt string with attached prefix created by 
        /// SaltedHashCompoundString then compares the unobfuscated hash code 
        /// with the locally generated hash  code for the salted string 
        /// incoming (using the unobfuscated salt string) and returns true if hash code match and false 
        /// if they do not.  It also return the output parameter prefix containing the unobfuscated prefix contained in saltedHashCompoundString
        /// </summary>
        public static bool SaltedHashCompareCompound(string incoming, string saltedHashCompoundString, out string prefix)
        {
            int delimiterCount = 0;
            string prefixString = "";
            string hashString = "";
            string saltString = "";

            // Dissect compound variable saltedHashCompoundString 
            foreach (char c in saltedHashCompoundString)
            {
                // remove the three components contained in saltedHashCompoundString (prefixString, hashString, and saltString)
                if (c.Equals(STRING_DELIMITER[0]))
                    delimiterCount++;
                else
                {
                    if (delimiterCount == 0)
                        prefixString += c;
                    if (delimiterCount == 1)
                        hashString += c;
                    if (delimiterCount == 2)
                        saltString += c;
                }
            }

            // Very basic validity checking. prefixString, hashString, and saltString must be at least one character in length
            if (prefixString.Length < 1 || hashString.Length < 1 || saltString.Length < 1)
            {
                prefix = "";
                return false;
            }
            else
            {
                // Set output parameter - prefix removed from saltedHashCompoundString
                prefix = prefixString;
                // Validate hash code with hash code stored in saltedHashCompoundString
                return SaltedHashCompare(incoming, hashString, saltString);
            }
        }


        /// <summary>
        /// Unobfuscates hash code string and salt string created by 
        /// SaltedHashURL then compares the unobfuscated hash code 
        /// with the locally generated hash  code for the salted string 
        /// incoming and returns true if hash code match and false 
        /// if they do not.   
        /// </summary>
        public static bool SaltedHashCompare(string incoming, string hashString, string saltString)
        {
            string dissectedHashString = "";
            string dissectedSaltString = "";
            int saltNum;
            int hash;

            try
            {
                //Remove random letter from hashString
                foreach (char c in hashString)
                {
                    if (!Char.IsLetter(c))
                        dissectedHashString = dissectedHashString + c;
                }
                //convert hashString to number
                hash = int.Parse(dissectedHashString);

                //Remove random letter from saltString
                foreach (char c in saltString)
                {
                    if (!Char.IsLetter(c))
                        dissectedSaltString = dissectedSaltString + c;
                }
                //Unobfuscate salt number by dividing by constant MULTIPLIER 
                saltNum = int.Parse(dissectedSaltString) / MULTIPLIER;

                // return Boolean from compare of the hash of saltNum + incoming and the incoming hashString
                return (hash == (saltNum.ToString() + incoming).GetHashCode());

            }
            // General purpose error trap 
            catch (Exception)
            {
                return false;
            }

        }

    }
}
