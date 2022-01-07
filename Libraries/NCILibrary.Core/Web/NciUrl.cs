using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace NCI.Web
{
    public class NciUrl
    {
        private string _uriStem;
        private bool _decodeParameters = false;
        private bool _collapseSameParams = false;

        public string UriStem
        {
            get
            {
                return _uriStem;
            }
            set
            {
                if (value == null) { throw new ArgumentNullException("value", "UriStem cannot be set to null."); }
                if (value.StartsWith("/") == false) { throw new ArgumentException("UriStem must start with a forward slash."); }

                _uriStem = value;
            }
        }

        public Dictionary<string, string> QueryParameters { get; set; }
        
        /// <summary>
        /// Creates a new instance of a case-sensitive NciUrl that does not decode parameters
        /// </summary>
        public NciUrl() : this(false) {}

        /// <summary>
        /// Creates a new instance of a case-sensitive NciUrl that will optionally decode parameters
        /// </summary>
        /// <param name="decodeParameters">True if we should decode the individual parameters</param>
        public NciUrl(bool decodeParameters) : this(decodeParameters, false) { }

        /// <summary>
        /// Creates a new instance of a case-sensitive NciUrl that will optionally decode parameters
        /// </summary>
        /// <param name="decodeParameters">True if we should decode the individual parameters</param>
        /// <param name="caseInsensitive">Treat parameters as case insensative</param>
        public NciUrl(bool decodeParameters, bool caseInsensitive) : this(decodeParameters, caseInsensitive, false) { }

        /// <summary>
        /// Creates a new instance of a NciUrl that will optionally decode parameters and optionally be case-insensitive
        /// </summary>
        /// <param name="decodeParameters">True if we should decode the individual parameters</param>
        /// <param name="caseInsensitive">Treat parameters as case insensative</param>
        /// <param name="collapseSameParams">When handling urls with multiple params with the same name, combine the values using commas</param>
        public NciUrl(bool decodeParameters, bool caseInsensitive, bool collapseSameParams)
        {
            if (caseInsensitive)
            {
                QueryParameters = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            }
            else
            {
                QueryParameters = new Dictionary<string, string>();
            }
            
            _decodeParameters = decodeParameters;
            _collapseSameParams = collapseSameParams;
        }

        public void Clear()
        {
            _uriStem = null;
            QueryParameters.Clear();
        }

        /// <summary>
        /// This function will not make any chnages to url that is passed in. This value is 
        /// set as is on URIStem. This function alos does not perform any validation.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="externalLink"></param>
        public void SetUrl(string url, bool externalLink)
        {
            if (externalLink)
                _uriStem = url;
            else
                SetUrl(url);
        }
        /// <summary>
        /// Loads (or reloads) the data of the current instance's fields based on the passed in URL.
        /// </summary>
        /// <param name="uriString"></param>
        public void SetUrl(string url)
        {
            if (url == null) { throw new ArgumentNullException(); }

            // Get the path and query string part by first creating a System.Uri.
            Uri uri = null;

            // Try to create a Uri assuming they passed in an absolute URI.
            if (Uri.TryCreate(url, UriKind.Absolute, out uri) == false)
            {
                // They didn't pass an absolute URI, so try again assuming it was a relative one.  
                // Note: tempuri.org is just a random host used to create a valid aboslute Uri.
                if (Uri.TryCreate(new Uri("http://tempuri.org"), url, out uri) == false)
                {
                    throw new ArgumentException(String.Format("{0} is an invalid url.", url));
                }
            }

            // At this point we have a valid Uri instance we can use to get the data we need.

            // Clear out existing data.
            Clear();

            // Set the URI.
            UriStem = uri.AbsolutePath; // TODO: is this what we want?  Should we change our "Stem" to be "Path" for ease of understanding?

            // Load the query parameters.
            string[] nameValuePairs = uri.Query.Split(new char[] { '&', '?' }, StringSplitOptions.RemoveEmptyEntries); // TODO: unit test - e.g. what if "&amp;" not "&" ???
            foreach (string nameValuePair in nameValuePairs)
            {
                // TODO: is there built in stuff to do this type of thing?
                string[] nameAndValue = nameValuePair.Split(new char[] { '=' }, StringSplitOptions.None);
                string name = nameAndValue[0];
                string value = nameAndValue[1];

                if(_decodeParameters)
                {
                    value = HttpUtility.UrlDecode(value);
                }

                if (_collapseSameParams && QueryParameters.ContainsKey(name))
                {
                    QueryParameters[name] += "," + value;
                }
                else
                {
                    QueryParameters.Add(name, value);
                }                
            }
        }

        public NciUrl CopyWithLowerCaseQueryParams()
        {
            NciUrl rtn = new NciUrl();
            rtn._uriStem = this._uriStem;

            foreach (KeyValuePair<string, string> item in this.QueryParameters)
            {
                rtn.QueryParameters.Add(item.Key.ToLower(), item.Value);
            }

            return rtn;
        }

        /// <summary>
        /// Helper function to check if a param is used. (And not just set with an empty string.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public bool IsInUrl(string paramName)
        {
            return this.QueryParameters.ContainsKey(paramName) && !String.IsNullOrWhiteSpace(this.QueryParameters[paramName]);
        }

        /// <summary>
        /// Appends a segment onto the end of an existing url path, handling "slash issues" so we 
        /// are sure we have one and only one slash.
        /// 
        /// Note: you'd think Uri.TryCreate(new Uri("http://tempuri.org/path1"), "path2", out uri) 
        /// would result in a uri of http://tempuri.org/path1/path2 but it doens't.  Tnstead you 
        /// get http://tempuri.org/path2.  A trailing slash is required after "path1" to get the 
        /// expected result:
        /// 
        /// Uri.TryCreate(new Uri("http://tempuri.org/path1"), "path2", out uri)
        /// true
        /// uri.ToString()
        /// "http://tempuri.org/path2"
        /// Uri.TryCreate(new Uri("http://tempuri.org/path1/"), "path2", out uri)
        /// true
        /// uri.ToString()
        /// "http://tempuri.org/path1/path2"
        /// 
        /// So we have to ensure below that UriStem ends in slash or we have to 
        /// add it in manually.  
        /// 
        /// The same issue exists with VirtualPathUtility.Combine():
        /// 
        /// VirtualPathUtility.Combine("/path1", "path2")
        /// "/path2"
        /// VirtualPathUtility.Combine("/path1/", "path2")
        /// "/path1/path2"
        /// </summary>
        /// <param name="segment"></param>
        public void AppendPathSegment(string pathSegment)
        {
            if (pathSegment == null) { throw new ArgumentNullException(); }
            if (pathSegment == String.Empty) { return; }

            // Prepare the stem and segment for combinging.
            string uriStem = VirtualPathUtility.AppendTrailingSlash(UriStem); // Make sure there IS a slash at the end of the base path.
            string segment = pathSegment.TrimStart(new char[] { '/' }); // Make sure there is NOT a slash at the beginning of the segment being appended.

            // Use the built in Uri class to do the combining.
            Uri uri = null;
            if (Uri.TryCreate(new Uri("http://tempuri.org" + uriStem), segment, out uri) == false)
            {
                throw new ArgumentException(String.Format("The path segment \"{0}\" could not be appended to the uri stem \"{1}\" to create a valid Uri", pathSegment, UriStem));
            }

            UriStem = uri.AbsolutePath;
        }

        /// <summary>
        /// Returns a string version of the URL represented by the NciUrl instance.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder queryParametersStringBuilder = new StringBuilder();
            foreach (string name in QueryParameters.Keys)
            {
                if (queryParametersStringBuilder.Length > 0)
                {
                    queryParametersStringBuilder.Append("&");
                }
                queryParametersStringBuilder.AppendFormat("{0}={1}", name, QueryParameters[name]);
            }
            string queryParametersString = queryParametersStringBuilder.ToString();

            return UriStem + ((queryParametersString == String.Empty) ? String.Empty : "?" + queryParametersString);
        }
    }
}
