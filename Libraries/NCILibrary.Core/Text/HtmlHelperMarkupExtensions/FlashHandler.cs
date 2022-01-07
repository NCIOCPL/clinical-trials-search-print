using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using NCI.Util;
using NCI.Text;

namespace NCI.Web
{
    /// <summary>
    /// Note: once we stop rendering the head with <%=, and start rendering it as a literal control 
    /// then can use javascript manager and we should put SWFObject in page via javascript manager 
    /// rather than manual HasSWFObjectBeenLoaded check.
    /// </summary>
    [MarkupExtensionHandler("Embeds a flash movie.",
    Usage = "{mx:HtmlHelpers.Flash()} which embeds a flash movie. ")]
    public class FlashHandler : MarkupExtensionHandler
    {
        private const string _swfLoadedKey = "HasSWFObjectBeenLoaded";

        private string _flashTargetElemID = string.Empty;
        private string _src = string.Empty;
        private int _height = -1;
        private int _width = -1;
        private string _requiredFlashVersion = string.Empty;
        private string _flashvars = string.Empty;
        private string _flashparams = string.Empty;
        private string _objectAttributes = string.Empty;


        /// <summary>
        /// Gets a value indicating if the SWFObject javascript code has been loaded yet.
        /// </summary>
        private bool HasSWFObjectBeenLoaded
        {
            get
            {
                if (HttpContext.Current.Items.Contains(_swfLoadedKey))
                    return (bool)HttpContext.Current.Items[_swfLoadedKey];
                else
                    return false;
            }
            set
            {
                if (HttpContext.Current.Items.Contains(_swfLoadedKey))
                    HttpContext.Current.Items[_swfLoadedKey] = value;
                else
                    HttpContext.Current.Items.Add(_swfLoadedKey, value);
            }
        }


        private void ParseParams(string[] parameters)
        {
            // Process and validate parameters.
            int maxParameterCount = 8;
            if (parameters.Length > maxParameterCount)
            {
                throw new MarkupExtensionException(String.Format(MarkupExtensionHandler.TooManyParametersError, this.Name, maxParameterCount, parameters.Length));
            }

            // Make sure all required parameters were passed in.
            int minParameterCount = 5;
            if (parameters.Length < minParameterCount)
            {
                throw new MarkupExtensionException(String.Format(MarkupExtensionHandler.TooFewParametersError, this.Name, minParameterCount, parameters.Length));
            }
            
            // Get the required parameters.

            // The Target HTML ID parameter is required and must not be null or empty.  If it is Strings.Clean will return null.
            int parameterIndex = -1;
            _flashTargetElemID = Strings.Clean(parameters[++parameterIndex]);
            if (_flashTargetElemID == null)
                throw new MarkupExtensionException(String.Format(MarkupExtensionHandler.RequiredParameterNotSpecifiedOrInvalidError, this.Name, "Target HTML ID", parameterIndex, typeof(string)));

            _src = Strings.Clean(parameters[++parameterIndex]);
            if (_src == null)
                throw new MarkupExtensionException(String.Format(MarkupExtensionHandler.RequiredParameterNotSpecifiedOrInvalidError, this.Name, "Source", parameterIndex, typeof(string)));

            _width = Strings.ToInt(parameters[++parameterIndex]);
            if (_width <= 0)
                throw new MarkupExtensionException(String.Format(MarkupExtensionHandler.RequiredParameterNotSpecifiedOrInvalidError, this.Name, "Width", parameterIndex, typeof(int)));

            _height = Strings.ToInt(parameters[++parameterIndex]);
            if (_height <= 0)
                throw new MarkupExtensionException(String.Format(MarkupExtensionHandler.RequiredParameterNotSpecifiedOrInvalidError, this.Name, "Height", parameterIndex, typeof(int)));

            //Note, this is a string since they have had versions like 6.5e that mattered.
            _requiredFlashVersion = Strings.Clean(parameters[++parameterIndex]);
            if (_requiredFlashVersion == null)
                throw new MarkupExtensionException(String.Format(MarkupExtensionHandler.RequiredParameterNotSpecifiedOrInvalidError, this.Name, "Required Flash Version", parameterIndex, typeof(string)));

            // Get optional parameters.

            // The following three parameters should look like:
            // key : value, key : value
            // For example: allowscriptaccess: "always", allowfullscreen: "true", wmode:"transparent"
            // They will then be passed in as objects, i.e. {key : value, key : value} to the embedswfobject JS method.

            // Flash Vars
            if (parameters.Length > ++parameterIndex)
            {
                _flashvars = Strings.Clean(parameters[parameterIndex], "");
            }

            // Flash Params
            if (parameters.Length > ++parameterIndex)
            {
                _flashparams = Strings.Clean(parameters[parameterIndex], "");
            }

            // Object Attributes
            if (parameters.Length > ++parameterIndex)
            {
                _objectAttributes = Strings.Clean(parameters[parameterIndex], "");
            }
        }


        public override string Name
        {
            get { return "HtmlHelpers.Flash"; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters">
        /// 
        /// TODO-v.NEXT: get this info into attribute:
        /// 
        /// Index   Param                   Requirements
        /// 0       id                      Non-empty string
        /// 1       flashSrc                Non-empty string
        /// 2       flashWidth              Number > 0
        /// 3       flashHeight             Number > 0
        /// 4       requiredFlashVersion    Non-empty string
        /// 5       Flash Variables         string
        /// 6       Flash Parameters        string
        /// 7       Flash Object Attributes string
        /// 
        /// TODO-v.NEXT: get error/log text from custom attribute data for param name and requirements?  Don't want to use reflection in general practice with these but maybe ok in case of exception since that shouldn't happen in production.
        /// </param>
        /// <returns></returns>
        public override string Process(string[] parameters)
        {
            ParseParams(parameters);

            StringBuilder sb = new StringBuilder();

            if (!HasSWFObjectBeenLoaded)
            {
                // This script will now be included in very cde page template originating from percussion.
                // commenting this line so we will not have duplicate script includes.
                //sb.AppendLine(@"<script type=""text/javascript"" src=""http://ajax.googleapis.com/ajax/libs/swfobject/2.2/swfobject.js""></script>");
                HasSWFObjectBeenLoaded = true;
            }

            sb.AppendLine(string.Format(@"
                <script type=""text/javascript"">
                swfobject.embedSWF(
                    '{0}', 
                    '{1}', 
                    {2}, 
                    {3}, 
                    '{4}', 
                    null, 
                    {{ {5} }},
                    {{ {6} }}, 
                    {{ {7} }}
                );
                </script>
            ", _src, _flashTargetElemID, _width, _height, _requiredFlashVersion, _flashvars, _flashparams, _objectAttributes));

            return sb.ToString();
        }
    }
}
