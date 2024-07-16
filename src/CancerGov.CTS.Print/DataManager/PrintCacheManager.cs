using System;
using System.Net;
using System.Threading.Tasks;

using Amazon.S3;
using Amazon.S3.Model;
using Common.Logging;

using CancerGov.CTS.Print.Models;
using System.IO;

namespace CancerGov.CTS.Print.DataManager
{
    public class PrintCacheManager
    {
        public const string METADATA_TRIAL_ID_LIST_KEY = "trial-id-list";
        public const string METADATA_SEARCH_CRITERIA_KEY = "search-criteria";

        static readonly ILog log = LogManager.GetLogger(typeof(PrintCacheManager));

        private IAmazonS3 Client { get; set; }

        private String BucketName { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client">The client for connecting to the S3 bucket.</param>
        /// <param name="bucketName">The name of the bucket where the documents are stored.</param>
        public PrintCacheManager(IAmazonS3 client, string bucketName)
        {
            Client = client;
            BucketName = bucketName;
        }

        /// <summary>
        /// Saves the generated page and its associated metadata in the S3 bucket associated
        /// with this instance
        /// </summary>
        /// <param name="key"></param>
        /// <param name="trialIds"></param>
        /// <param name="searchParams"></param>
        /// <param name="content"></param>
        public async Task Save(Guid key, string[] trialIds, SearchCriteria searchParams, string content)
        {
            try
            {
                // Create a PutObject request
                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = BucketName,
                    Key = key.ToString(),
                    ContentBody = content,
                    ContentType = "text/html"
                };

                // Metadata in case we ever need to search for print pages meeting some criteria.
                request.Metadata.Add(METADATA_TRIAL_ID_LIST_KEY, String.Join(",", trialIds));
                request.Metadata.Add(METADATA_SEARCH_CRITERIA_KEY, searchParams.ToJson());

                // Put object.
                PutObjectResponse response = await this.Client.PutObjectAsync(request);

                // Log "unusual" status codes, but attempt to keep going.
                int status = (int)response.HttpStatusCode;
                if(!(status >= 200 && status < 300))
                {
                    log.Warn($"Unexpected return code '{status}' ({response.HttpStatusCode}) saving document '{key}'.");

                    // Something's definitely wrong with this save attempt.
                    // Log an error and throw.
                    // See "List of error codes" on https://docs.aws.amazon.com/AmazonS3/latest/API/ErrorResponses.html#ErrorCodeList
                    if (status >= 400)
                    {
                        string message = $"Error return code '{status}' ({response.HttpStatusCode}) saving document '{key}'.";
                        log.Error(message);
                        throw new PrintSaveFailureException(message);
                    }
                }
            }
            // No need to relog.
            catch (PrintSaveFailureException)
            {
                throw;
            }
            catch (AmazonS3Exception ex)
            {
                log.Error($"Error saving generated page {key}.\n{ex.AmazonId2} {ex.ErrorCode} {ex.Message}\n\n{ex.ResponseBody}", ex);
                throw;
            }
            catch (Exception ex)
            {
                log.Error($"Error saving generated page {key}.", ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves cached CTS print HTML from the database.
        /// </summary>
        /// <param name="printId">A GUID identifying the cached page to retrieve.</param>
        /// <returns>The page HTML, or NULL if the document does not exist.</returns>
        public async Task<string> GetPage(Guid printID)
        {
            string printPageHtml = null;

            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = BucketName,
                    Key = printID.ToString()
                };

                // Issue request and remember to dispose of the response
                using (GetObjectResponse response = await this.Client.GetObjectAsync(request))
                {
                    // Only try reading content for non-error response codes.
                    // Note: page not found will throw AmazonS3Exception, see catch block.
                    int status = (int)response.HttpStatusCode;
                    if (status < 400)
                    {
                        // Log "unusual" (non-error) status codes, but keep going.
                        if (!(status >= 200 && status < 300))
                        {
                            log.Warn($"Error return code '{status}' ({response.HttpStatusCode}) saving document '{printID}'.");
                        }

                        using (StreamReader reader = new StreamReader(response.ResponseStream))
                        {
                            printPageHtml = reader.ReadToEnd();
                        }
                    }
                    else
                    {
                        // The expectation is that GetObject will throw AmazonS3Exception for all failed
                        // retrievals and not just the "Document Not Found" case.
                        // If this expectation is not met, we want to find out.
                        string message = $"Status {(int)response.HttpStatusCode} retrieving page {{printID.ToString()}}.";
                        log.Error(message);
                        throw new PrintFetchFailureException(message);
                    }
                }
            }
            catch (AmazonS3Exception ex)
            {
                // GetObjectAsync throws AmazonS3Exception if the page wasn't found, in that case we want to
                // swallow the exception and simply return null.
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    printPageHtml = null;
                    log.Debug($"No page found for {nameof(printID)} '{printID}'");
                }
                else
                {
                    log.Error($"Error retrieving generated page {nameof(printID)} '{printID}'\n{ex.AmazonId2} {ex.ErrorCode} {ex.Message}\n\n{ex.ResponseBody}", ex);
                    throw;
                }
            }
            // No need to relog.
            catch (PrintFetchFailureException)
            {
                throw;
            }
            catch (Exception ex)
            {
                log.Error($"Error retrieving {nameof(printID)} {printID}.", ex);
                throw;
            }

            return printPageHtml;
        }
    }
}
