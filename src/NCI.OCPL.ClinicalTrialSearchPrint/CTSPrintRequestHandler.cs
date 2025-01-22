using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

using Amazon.S3;

using Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CancerGov.ClinicalTrialsAPI;
using CancerGov.CTS.Print.DataManager;
using CancerGov.CTS.Print.Models;
using CancerGov.CTS.Print.Rendering;

using CGovLocationType = CancerGov.CTS.Print.Models.LocationType;

namespace NCI.OCPL.ClinicalTrialSearchPrint
{
    // Handles all requests for the CTS.Print service.
    public class CTSPrintRequestHandler : HttpTaskAsyncHandler
    {
        const string DEFAULT_CTS_PRINT_TEMPLATE = "~/VelocityTemplates/PrintResults.vm";

        const string DEFAULT_CTS_PRINT_DISPLAY_URL_FORMAT = "/CTS.Print/Display?printid={0}";

        public static string MISSING_FIELD_MESSAGE { get; } = "Field is empty or not found";

        public static string MUST_BE_RELATIVE_LINK_MESSAGE { get; } = "Must be an absolute path.";

        public static string INVALID_CHARACTERS_MESSAGE { get; } = "Field contains invalid characters";

        static readonly ILog log = LogManager.GetLogger(typeof(PrintCacheManager));

        /// <summary>
        /// Get the location of the printed page velocity template;
        /// </summary>
        private string PrintTemplate
        {
            get
            {
                string printTemplatePath = ConfigurationManager.AppSettings["printTemplate"];
                if (String.IsNullOrWhiteSpace(printTemplatePath))
                    printTemplatePath = DEFAULT_CTS_PRINT_TEMPLATE;
                return printTemplatePath;
            }
        }

        /// <summary>
        /// Get the format string for the URL to use when retrieving the generated page.
        /// </summary>
        private string PrintPageDisplayURLFormat
        {
            get
            {
                string formatString = ConfigurationManager.AppSettings["displayUrlFormat"];
                if (String.IsNullOrWhiteSpace(formatString))
                    formatString = DEFAULT_CTS_PRINT_DISPLAY_URL_FORMAT;
                return formatString;
            }
        }

        /// <summary>
        /// Get the name of the environment variable to retrieve the S3 bucket's name.
        /// </summary>
        private string S3BucketVariableName
        {
            get
            {
                string varName = ConfigurationManager.AppSettings["S3BucketName_Var"];
                if (String.IsNullOrWhiteSpace(varName))
                {
                    varName = "ClinicalTrials_S3BucketName";
                }
                return varName;
            }
        }

        /// <summary>
        /// Retrieve the S3 bucket's name from an environment variable.
        /// </summary>
        private string S3BucketName
        {
            get
            {
                string bucketName = Environment.GetEnvironmentVariable(S3BucketVariableName);
                if (String.IsNullOrWhiteSpace(bucketName))
                {
                    string configMsg = $"S3 bucket environment variable '{bucketName}' is not set.";

                    log.Error(configMsg);
                    throw new ConfigurationErrorsException(configMsg);
                }
                return bucketName;
            }
        }

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

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = baseUrl
            };

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
                (int StatusCode, string ContentType, string Content) = await GenerateCachedPage(request);

                context.Response.StatusCode = StatusCode;
                context.Response.ContentType = ContentType;
                context.Response.Write(Content);
            }
            // Retrieve the page on GET requests.
            else if (request.HttpMethod == "GET")
            {
                (int StatusCode, string Content) = await GetCachedContent(request.QueryString["printid"], context);

                context.Response.StatusCode = StatusCode;
                context.Response.ContentType = "text/html";
                context.Response.Write(Content);
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
        /// <param name="context">The HttpContext for the request being processed.</param>
        /// <returns>A tuple containing the HTTP status code and response body to return.</returns>
        public async Task<(int StatusCode, string Content)> GetCachedContent(string printID, HttpContext context)
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

            JObject requestBody;
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
            catch (ArgumentException ex)
            {
                string errorMsg;

                // Handle fields which are completely missing. (MissingFieldException is a subclass of ArgumentException.)
                if (ex is MissingFieldException)
                    errorMsg = $"Field '{ex.ParamName}' not found.";
                // Handle fields with invalid values.
                else
                    errorMsg = $"Field '{ex.ParamName}' has an invalid value.";

                log.Debug(errorMsg, ex);
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
                // Renderable search criteria, not to be confused with the JSON object.
                SearchCriteria criteria = SearchCriteriaFactory.Create(searchCriteria);

                LocationCriteria locationData = LocationCriteriaFactory.Create(searchCriteria);

                RemoveNonRecruitingSites(trialDetails);

                if((locationData.IsVAOnly && locationData.LocationType != CGovLocationType.Hospital) || (locationData.LocationType == CGovLocationType.CountryCityState || locationData.LocationType == CGovLocationType.Zip || locationData.LocationType == CGovLocationType.AtNIH))
                {
                    foreach(JObject trial in trialDetails["data"].Values<JObject>())
                    {
                        JArray rawSites = new JArray(trial["sites"].Values<JToken>().ToArray());
                        JArray filteredSites = GetFilteredLocations(rawSites, locationData);
                        trial["sites"] = filteredSites;
                    }

                }

                // Important: After this call, trial sites contain state NAMES instead of codes.
                SetLocationStateNames(trialDetails);

                trialDetails = EnforceTrialOrder(trialDetails, trialIDs);

                // Get the path to the page template.
                string template = PrintTemplate;
                if (String.IsNullOrWhiteSpace(template))
                    throw new ConfigurationErrorsException("printTemplate not set");
                template = request.RequestContext.HttpContext.Server.MapPath(template);

                // Render the page.
                var renderer = new PrintRenderer(template);
                string page = renderer.Render(trialDetails, criteria, locationData, linkTemplate, newSearchLink);

                // Generate document key
                Guid key = Guid.NewGuid();

                // Insert link to the page.
                string referenceURL = String.Format(PrintPageDisplayURLFormat, key);
                page = page.Replace("${generatePrintURL}", referenceURL);

                // Use the entire request body as metadata.
                string rawRequestData = requestBody.ToString(Formatting.None);

                // Save the page.
                var datamgr = GetPrintCacheManager();
                await datamgr.Save(key, rawRequestData, page);

                // Set up the return.
                statusCode = 200;
                contentType = "application/json";
                responseBody = $"{{\"printID\": \"{key}\"}}";
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
            GetFields(JObject requestBody)
        {
            // Finder for characters not allowed in the new_search_link field.
            Regex newLinkDisallowedCharacters = new Regex("[^a-zA-Z0-9-/.]+");

            // Finder for characters not allowed in the link_template field.
            // This is broader in order to allow URL encoded characters.
            Regex linkTemplateDisallowedCharacters = new Regex("[^a-zA-Z0-9-/.&=?%_]+");

            string[] trialIDs;
            string linkTemplate;
            string newSearchLink = null;
            JObject searchCriteria = null;

            JToken field;

            // Get trial IDs
            field = requestBody["trial_ids"];
            if (field == null || !field.HasValues)
                throw new MissingFieldException(MISSING_FIELD_MESSAGE, "trial_ids");

            trialIDs = field.Values<string>().ToArray();

            // Get link template
            field = requestBody["link_template"];
            if (field == null || String.IsNullOrWhiteSpace(field.Value<string>()))
                throw new MissingFieldException(MISSING_FIELD_MESSAGE, "link_template");

            linkTemplate = field.Value<string>().Trim();

            if (!linkTemplate.StartsWith("/"))
                throw new ArgumentException(MUST_BE_RELATIVE_LINK_MESSAGE, "link_template");

            // Remove <TRIAL_ID> placeholder before checking for invalid characters.
            if (linkTemplateDisallowedCharacters.IsMatch(linkTemplate.Replace("<TRIAL_ID>", string.Empty)))
                throw new ArgumentException(INVALID_CHARACTERS_MESSAGE, "link_template");

            // Get new search link, or default if missing.
            field = requestBody["new_search_link"];
            if (field != null && !String.IsNullOrWhiteSpace(field.Value<string>()))
            {
                newSearchLink = field.Value<string>().Trim();
                if(!newSearchLink.StartsWith("/"))
                    throw new ArgumentException(MUST_BE_RELATIVE_LINK_MESSAGE, "new_search_link");

                if (newLinkDisallowedCharacters.IsMatch(newSearchLink))
                    throw new ArgumentException(INVALID_CHARACTERS_MESSAGE, "new_search_link");
            }
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
        /// Alters the list of trials to remove all sites which are not current recruiting.
        /// </summary>
        /// <param name="trialData">The JSON data structure returned by a successful call to the <see cref="IClinicalTrialsAPIClient.GetMultipleTrials(IEnumerable{string}, int, int)"/> method. </param>
        public void RemoveNonRecruitingSites(JObject trialData)
        {
            foreach (JToken trial in trialData["data"])
            {
                JToken original = trial["sites"];
                if(original != null && original.Type == JTokenType.Array)
                {
                    var clean = ((JArray)original).Where(site => SiteStatus.IsActivelyRecruiting(site["recruitment_status"]?.Value<string>()));
                    trial["sites"] = new JArray(clean);
                }
                else
                {
                    log.WarnFormat("Site List is empty. {0}", trial.ToString());
                }
            }
        }

        /// <summary>
        /// Alters all trial sites to replace the 'org_state_or_province' property's state abbreviation with its actual name.
        /// </summary>
        /// <param name="trialData">The JSON data structure returned by a successful call to the <see cref="IClinicalTrialsAPIClient.GetMultipleTrials(IEnumerable{string}, int, int)"/> method. </param>
        public void SetLocationStateNames(JObject trialData)
        {
            foreach (JToken trial in trialData["data"])
            {
                foreach(JToken site in trial["sites"])
                {
                    string code = site["org_state_or_province"]?.Value<string>();
                    if (!String.IsNullOrWhiteSpace(code))
                        site["org_state_or_province"] = StateNameHelper.GetStateName(code);
                }
            }
        }


        /// <summary>
        /// Filters a list of trial sites to match the search location criteria.
        ///
        ///     LocationType.AtNIH - returns only the sites at the NIH campus.
        ///
        ///     LocationType.CountryCityState - returns only the sites matching the specified
        ///                                     Country, City, and State. No filtering occurs
        ///                                     for sub-criteria which were not included in
        ///                                     the criteria object.
        ///
        ///     LocationType.Zip - returns only the sites within searchParams.ZipRadius miles
        ///                        of searchParams.GeoLocation.
        ///
        /// NOTE: LocationTypes for Hospital and None will not be filtered.
        /// </summary>
        /// <returns>An array of trial sites matching the location criteria.</returns>
        public JArray GetFilteredLocations(JArray sites, LocationCriteria searchParams)
        {
            IEnumerable<JToken> rtnSites = sites;

            switch (searchParams.LocationType)
            {
                case CGovLocationType.AtNIH:
                    {
                        rtnSites = rtnSites.Where(s => s["org_postal_code"]?.Value<string>() == "20892");
                        break;
                    }
                case CGovLocationType.CountryCityState:
                    {
                        if (searchParams.HasCountry)
                        {
                            rtnSites = rtnSites.Where(s => StringComparer.CurrentCultureIgnoreCase.Equals(s["org_country"]?.Value<string>(), searchParams.Country));
                        }

                        if (searchParams.HasCity)
                        {
                            rtnSites = rtnSites.Where(s => StringComparer.CurrentCultureIgnoreCase.Equals(s["org_city"]?.Value<string>(), searchParams.City));
                        }

                        if (searchParams.HasState)
                        {
                            rtnSites = rtnSites.Where(s => searchParams.States.Contains(s["org_state_or_province"]?.Value<string>()));
                        }

                        break;
                    }
                case CGovLocationType.Zip:
                    {
                        rtnSites = rtnSites.Where(site =>
                        {
                            JToken jLat = site.SelectToken("org_coordinates.lat");
                            JToken jLon = site.SelectToken("org_coordinates.lon");
                            string country = site["org_country"]?.Value<string>();

                            return StringComparer.CurrentCultureIgnoreCase.Equals(country, "United States")
                                && jLat != null
                                && jLon != null
                                && searchParams.GeoLocation.DistanceBetween(new GeoLocation(jLat.Value<float>(), jLon.Value<float>())) <= searchParams.ZipRadius;
                        });

                        break;
                    }
                default:
                    {
                        //Basically we can't/shouldn't filter.
                        break;
                    }
            }

            if (searchParams.IsVAOnly && searchParams.LocationType != CGovLocationType.Hospital)
            {
                rtnSites = rtnSites.Where(s => s["org_va"] != null && s["org_va"].Value<bool>());
            }

            // Convert to an array so we get the correct constructor. (IEnumerable<JToken> is treated as a single Object,
            // which would lead to an array with a single element, containing a nested array with the actual result.
            JArray ret = new JArray(rtnSites.ToArray());
            return ret;
        }


        /// <summary>
        /// Reorders the trial details in a clinical trials API response to match the
        /// order specified in the <c>trialIDs</c> list.
        /// </summary>
        /// <param name="trialData">The response object from a clinical trial search.</param>
        /// <param name="trialIDs">An array of strings containing NCI IDs in the desired order.</param>
        /// <returns></returns>
        public JObject EnforceTrialOrder(JObject trialData, string[] trialIDs)
        {
            JArray trialList = (JArray)trialData["data"];
            List<JToken> reorderedTrials = new List<JToken>();

            Array.ForEach(trialIDs, id => {
                IEnumerable<JToken> match = trialList.Where(trial => trial["nci_id"].Value<string>() == id);

                // It shouldn't be possible to have anything other than exactly one match for a given
                // trial ID. If something weird happens, we'll handle it gracefully, returning as much data
                // as we have and logging the anomaly on the theory we'll look at it later.
                if (match.Count() > 0)
                {
                    reorderedTrials.AddRange(match);
                    if (match.Count() > 1)
                        log.WarnFormat("Multiple matches for ID '{0}'.", id);
                }
                else
                {
                    log.WarnFormat("No match found for ID '{0}'.", id);
                }
            });

            trialData["data"] = new JArray(reorderedTrials.ToArray());

            return trialData;
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
            AmazonS3Config config = GetS3ClientConfig();
            AmazonS3Client client = new AmazonS3Client(config);
            return new PrintCacheManager(client, S3BucketName);
        }

        /// <summary>
        /// Get an <see cref="https://docs.aws.amazon.com/sdkfornet/v3/apidocs/items/S3/TS3Config.html">AmazonS3Config</see>
        /// object with our custom settings.
        /// </summary>
        /// <returns>A customized AmazonS3Config instance.</returns>
        private AmazonS3Config GetS3ClientConfig()
        {
            AmazonS3Config config = new AmazonS3Config()
            {
                AllowAutoRedirect = true    // Follow any redirects on the S3.
            };

            return config;
        }
    }
}
