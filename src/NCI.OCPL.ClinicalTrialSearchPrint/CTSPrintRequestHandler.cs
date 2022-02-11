using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

using Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CancerGov.ClinicalTrialsAPI;
using CancerGov.CTS.Print.DataManager;
using CancerGov.CTS.Print.Models;
using CancerGov.CTS.Print.Rendering;

namespace NCI.OCPL.ClinicalTrialSearchPrint
{
    public class CTSPrintRequestHandler : HttpTaskAsyncHandler
    {
        static ILog log = LogManager.GetLogger(typeof(PrintCacheManager));

        // A single instance of HttpClient is intended to be shared by all requests within
        // an application.
        // See: https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=netframework-4.6.1
        static readonly HttpClient _httpClient;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static CTSPrintRequestHandler()
        {
            Uri baseUrl = new Uri(ClinicalTrialSearchAPISection.Instance.BaseUrl);

            // The ClinicalTrials API always returns using gzip encoding, regardless of whether the client sends
            // an Accept header. Even if this weren't the case, we'd likely want to save some time and bandwidth.
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            _httpClient = new HttpClient(handler);
            _httpClient.BaseAddress = baseUrl;

            // Formally add the accept headers. NOTE: Brotli compression is not supported in the 4.x framework. That requires .Net Core 3.0 and later.
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool IsReusable => false;

        /// <inheritdoc/>
        public async override Task ProcessRequestAsync(HttpContext context)
        {
            HttpRequest request = context.Request;

            // Generate on Post requests.
            if (request.HttpMethod == "POST")
            {
                (int StatusCode, string ContentType, string Content) result = await GenerateCachedPage(request);

                context.Response.StatusCode = result.StatusCode;
                context.Response.ContentType = result.ContentType;
                context.Response.Write(result.Content);
            }
            // Retrieve the page on GET requests.
            else if (request.HttpMethod == "GET")
            {
                (int StatusCode, string Content) result = await GetCachedContent(request.QueryString["printid"]);

                context.Response.StatusCode = result.StatusCode;
                context.Response.ContentType = "text/html";
                context.Response.Write(result.Content);
            }
            // Anything else, return an error.
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

        /// <summary>
        /// Helper method to encapsulate the high-level logic of creating the trial print page.
        /// </summary>
        /// <param name="request">The HTTP request for this session.</param>
        /// <returns>A tuple containing the HTTP status code, content type, and response body to return.</returns>
        public async Task<(int StatusCode, string ContentType, string Content)> GenerateCachedPage(HttpRequest request)
        {
            int statusCode = 200;
            string contentType = "application/json";
            string responseBody = String.Empty;

            JToken requestBody;
            try
            {
                requestBody = GetRequestBody(request.InputStream);
            }
            catch (Exception ex)
            {
                log.Debug("Error parsing request body.", ex);
                return (400, "text/plain", "Unable to parse request body.");
            }

            string[] trialIDs;
            string linkTemplate;
            string newSearchLink;
            JObject searchCriteria;

            // Get the details of what's being printed.
            try
            {
                (trialIDs, linkTemplate, newSearchLink, searchCriteria) = GetFields(requestBody);
            }
            catch (MissingFieldException ex)
            {
                string errorMsg = $"Field '{ex.FieldName}' not found.";
                log.Debug(errorMsg);
                return (400, "text/plain", errorMsg);
            }

            // Get the trial details.
            JObject trialDetails;
            try
            {
                IClinicalTrialsAPIClient apiClient = GetClinicalTrialsApiClient();
                trialDetails = await apiClient.GetMultipleTrials(trialIDs);
            }
            catch (Exception ex)
            {
                log.Error("Error retrieving trial details.", ex);
                throw;
            }

            try
            {
                // TODO: Create the SearchCriteriaFactory to populate the criteria object.
                SearchCriteria criteria = new SearchCriteria();

                // Get the path to the page template.
                string template = ConfigurationManager.AppSettings["printTemplate"];
                if (String.IsNullOrWhiteSpace(template))
                    throw new ConfigurationErrorsException("printTemplate not set");
                template = request.RequestContext.HttpContext.Server.MapPath(template);

                // Render the page.
                var renderer = new PrintRenderer(template);
                string page = renderer.Render(trialDetails, criteria);

                // Save the results.
                string connString = ConfigurationManager.ConnectionStrings["DbConnectionString"].ConnectionString;
                var datamgr = new PrintCacheManager(connString);
                Guid cacheId = datamgr.Save(trialIDs, criteria, page);

                // Set up the return.
                statusCode = 200;
                contentType = "application/json";
                responseBody = $"{{\"printID\": \"{cacheId}\"}}";
            }
            catch (Exception ex)
            {
                log.Error("Error rendering and saving the page.", ex);
                throw;
            }

            return (statusCode, contentType, responseBody);
        }

        /// <summary>
        /// Helper method to break out the various components of the request into a more easily used form.
        /// </summary>
        /// <param name="requestBody">JObject containing the request body.</param>
        /// <returns>A tuple with the fields:<br />
        /// TrialIDs - the list of trials to include in the report.<br />
        /// LinkTemplate - the template for constructing links to a specific trial.<br />
        /// NewSearchLink - the link to perform a new search.<br />
        /// SearchCriteria - the criteria used to perform the search.</returns>
        /// <exception cref="MissingFieldException">Thrown when a required field (trial_ids and link_template) is not included. The 'Field'
        /// property will contain the name of the missing field.</exception>
        /// <exception cref="ConfigurationErrorsException">Thrown when the new_search_link field is not included and no default is configured.</exception>
        public (string[] TrialIDs, string LinkTemplate, string NewSearchLink, JObject SearchCriteria)
            GetFields(JToken requestBody)
        {
            string[] trialIDs;
            string linkTemplate;
            string newSearchLink = null;
            JObject searchCriteria = null;

            JToken field;

            // Get trial IDs
            field = requestBody["trial_ids"];
            if (field == null || !field.HasValues)
                throw new MissingFieldException("trial_ids");

            trialIDs = field.Values<string>().ToArray();

            // Get link template
            field = requestBody["link_template"];
            if (field == null || String.IsNullOrWhiteSpace(field.Value<string>()))
                throw new MissingFieldException("link_template");

            linkTemplate = field.Value<string>();

            // Get new search link, or default if missing.
            field = requestBody["new_search_link"];
            if (field != null && !String.IsNullOrWhiteSpace(field.Value<string>()))
                newSearchLink = field.Value<string>();
            else
            {
                newSearchLink = ConfigurationManager.AppSettings["defaultNewSearchLink"];
                if (String.IsNullOrWhiteSpace(newSearchLink))
                    throw new ConfigurationErrorsException("defaultNewSearchLink not set.");
            }

            field = requestBody["search_criteria"];
            if (field == null || !field.HasValues)
                searchCriteria = null;
            else
                searchCriteria = field.Value<JObject>();

            return (trialIDs, linkTemplate, newSearchLink, searchCriteria);
        }

        /// <summary>
        /// Helper method to convert the HttpRequest object's input stream into
        /// a more useful format.
        /// </summary>
        /// <param name="inputStream">Stream containing the Http request body.</param>
        /// <returns>JObject containing request body.</returns>
        public JObject GetRequestBody(Stream inputStream)
        {
            JObject requestBody;

            inputStream.Position = 0;
            using (StreamReader sr = new StreamReader(inputStream))
            {
                using (JsonTextReader reader = new JsonTextReader(sr))
                {
                    requestBody = JObject.Load(reader);
                }
            }

            return requestBody;
        }

        /// <summary>
        /// Factory method to create the Clinical Trials API client.
        /// </summary>
        /// <returns>An insteance of the API client.</returns>
        private IClinicalTrialsAPIClient GetClinicalTrialsApiClient()
        {
            ClinicalTrialSearchAPISection config = ClinicalTrialSearchAPISection.Instance;
            return new ClinicalTrialsAPIClient(_httpClient, config);
        }

        /// <summary>
        /// Factory method to create the print cache manager.
        /// </summary>
        /// <returns>An instance of the client.</returns>
        /// <exception cref="ConfigurationErrorsException"></exception>
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
