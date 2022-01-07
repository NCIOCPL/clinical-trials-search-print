using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using Common.Logging;

using CancerGov.DataManagers;

namespace NCI.OCPL.ClinicalTrialSearchPrint
{
    public class CTSPrintRequestHandler : HttpTaskAsyncHandler
    {
        static ILog log = LogManager.GetLogger(typeof(PrintCacheManager));

        public override bool IsReusable => false;

        public async override Task ProcessRequestAsync(HttpContext context)
        {
            HttpRequest request = context.Request;

            if (request.HttpMethod == "POST")
            {
                context.Response.StatusCode = 200;
                context.Response.Write("Looks like a POST request.");
            }
            else if (request.HttpMethod == "GET")
            {
                (int StatusCode, string Content) result = await GetCachedContent(request.QueryString["printid"]);

                context.Response.StatusCode = result.StatusCode;
                context.Response.Write(result.Content);
            }
            else
            {
                context.Response.StatusCode = 405;
                context.Response.Write("Method not allowed.");
            }
        }

        /// <summary>
        /// Helper method to encapsulate the high-level logic of parsing the cache ID and interpreting
        /// the results of the page retrieval.
        /// </summary>
        /// <param name="printID">String containing the GUID of the cached page being retrieved.</param>
        /// <returns>A tuple containing the HTTP status code and response body to return.</returns>
        public async Task<(int StatusCode, string Content)> GetCachedContent(string printID)
        {
            PrintCacheManager cacheManager = GetPrintCacheManager();

            Guid cacheID;
            int statusCode;
            string responseBody;

            if (Guid.TryParse(printID, out cacheID))
            {
                string data = await cacheManager.GetPage(cacheID);

                if (!String.IsNullOrWhiteSpace(data))
                {
                    statusCode = 200;
                    responseBody = data;
                }
                else
                {
                    statusCode = 404;
                    responseBody = "Not Found";
                }
            }
            else
            {
                statusCode = 400;
                responseBody = "Invalid printid";
            }

            return (statusCode, responseBody);
        }

        private PrintCacheManager GetPrintCacheManager()
        {
            string connString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;

            if(String.IsNullOrWhiteSpace(connString))
            {
                log.Error("DbConnectionString not configured.");
                throw new ConfigurationErrorsException("DbConnectionString not configured.");
            }

            return new PrintCacheManager(connString);
        }
    }
}
