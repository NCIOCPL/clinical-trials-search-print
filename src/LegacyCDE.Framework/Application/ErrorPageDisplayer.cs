using System;
using System.Web;
using Common.Logging;

namespace NCI.Web.CDE.Application
{
    /// <summary>
    /// This is a helper class for wrangling all of the various implementations of RaisePageError
    /// </summary>
    public static class ErrorPageDisplayer
    {
        static readonly ILog log = LogManager.GetLogger(typeof(ErrorPageDisplayer));

        /// <summary>
        /// Display the "ErrorPage" page and a status of 500.
        /// </summary>
        /// <param name="callingClass">The name of the class that called this - for logging.</param>
        public static void RaisePageError(string callingClass, int code = 500, string message = "Application Error")
        {
            // raise a not found page
            RaisePage(callingClass, message, code, "ErrorPage");
        }

        /// <summary>
        /// Display the Invalid Clinical Trials Search Id page and a status of 404.
        /// </summary>
        /// <param name="callingClass">The name of the class that called this - for logging.</param>
        public static void RaisePageNotFound(string callingClass, int code = 404, string message = "Page Not Found")
        {
            // raise a not found page
            RaisePage(callingClass, message, code, "NotFoundPage");
        }

        /// <summary>
        /// Raises a page using the status code to determine if an error ot notfound page should be displayed.
        /// Raises no page if status unrecognized.
        /// </summary>
        /// <param name="callingClass">the name of the calling class (used for logging)</param>
        /// <param name="code">the status code to use for the page.</param>
        public static void RaisePageByCode(string callingClass, int code, string message = "")
        {
            bool emptyMessage = String.IsNullOrWhiteSpace(message);
            if (code >= 400 && code < 500)
            {
                if (emptyMessage)
                {
                    RaisePageNotFound(callingClass, code);
                }
                else
                {
                    RaisePageNotFound(callingClass, code, message);
                }
            }
            else if (code >= 500 && code < 600)
            {
                if (emptyMessage)
                {
                    RaisePageError(callingClass, code);
                }
                else
                {
                    RaisePageError(callingClass, code, message);
                }
            }
            else
            {
                // if the status is not recognized yet, then set the response code and continue
                HttpContext.Current.Response.StatusCode = code;
            }
        }

        /// <summary>
        /// Display a page using an arbitrary configuration string and repsonse code.
        /// </summary>
        /// <param name="callingClass">the calling class name - for logging purposes</param>
        /// <param name="pageDescription">the description of the page, used in the response</param>
        /// <param name="responseCode">the status code of the response</param>
        /// <param name="configurationKey">the name of the key containing the page URL in application settings</param>
        private static void RaisePage(string callingClass, string pageDescription, int responseCode, string configurationKey)
        {
            // Set the status code and description
            HttpContext.Current.Response.StatusDescription = pageDescription;
            HttpContext.Current.Response.StatusCode = responseCode;
            HttpContext.Current.Response.TrySkipIisCustomErrors = true;

            string pageUrl = System.Configuration.ConfigurationManager.AppSettings[configurationKey];
            bool hasDisplayedMessage = false;

            try
            {
                if (pageUrl != null && pageUrl.Trim() != "")
                {
                    HttpContext.Current.Server.Execute(pageUrl);
                    hasDisplayedMessage = true;
                }
                else
                {
                    log.WarnFormat("{0} URL is null or empty", pageDescription);
                }
            }
            catch (Exception ex)
            {
                log.WarnFormat("Could not display '{0}' page.", ex, pageDescription);
            }

            if (!hasDisplayedMessage)
            {
                //There is no way to show the page, so just show the description message instead.
                HttpContext.Current.Response.Write(pageDescription);
            }

            HttpContext.Current.Response.End();
        }
    }
}
