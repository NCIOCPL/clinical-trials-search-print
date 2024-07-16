using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Amazon.S3;
using Amazon.S3.Model;
using CancerGov.CTS.Print.DataManager;
using CancerGov.CTS.Print.Models;
using Moq;
using Xunit;

namespace CancerGov.CTS.Print.PrintCache
{
    public class PrintCache_SavePage
    {
        /// <summary>
        /// Make sure we don't swallow errors.
        /// </summary>
        [Theory]
        [InlineData(HttpStatusCode.BadRequest)] // 400
        [InlineData(HttpStatusCode.Forbidden)]  // 403
        [InlineData(HttpStatusCode.NotFound)]   // 404
        [InlineData(HttpStatusCode.InternalServerError)]// 500
        [InlineData(HttpStatusCode.GatewayTimeout)]     // 504
        public async Task SaveFailure(HttpStatusCode statusCode)
        {
            Mock<IAmazonS3> mockClientConfig = new Mock<IAmazonS3>();
            mockClientConfig.Setup(svc => svc.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutObjectResponse
                    {
                        HttpStatusCode = statusCode
                    });

            const string key = "ca761232-ed42-11ce-bacd-00aa0057b223";

            SearchCriteria criteria = SearchCriteriaFactory.Create(null);

            var cm = new PrintCacheManager(mockClientConfig.Object, "fake-bucket-name");

            PrintSaveFailureException ex = await Assert.ThrowsAsync<PrintSaveFailureException>
                (
                    async () =>
                    {
                        await cm.Save(new Guid(key), new string[] { "trial-id-1", "trial-id-2" }, criteria, "blob of HTML");
                    }
                );
            Assert.Equal(
                $"Error return code '{(int)statusCode}' ({statusCode}) saving document '{key}'.",
                ex.Message
            );
        }

        /// <summary>
        /// Make sure the object being sent to S3 has the expected values.
        /// Verify handling of "good" response codes
        /// </summary>
        [Theory]
        [InlineData(HttpStatusCode.OK)]         // 200
        [InlineData(HttpStatusCode.Accepted)]   // 202
        [InlineData(HttpStatusCode.NoContent)]  // 204
        [InlineData(HttpStatusCode.NotModified)]// 304
        public async Task ObjectStructure(HttpStatusCode status)
        {
            Mock<IAmazonS3> mockClientConfig = new Mock<IAmazonS3>();

            PutObjectRequest actualRequest = null;

            PutObjectResponse mockResponse = new PutObjectResponse
            {
                HttpStatusCode = status
            };

            const string expectedKey = "ca761232-ed42-11ce-bacd-00aa0057b223";
            const string expectedBody = "<html><body><p>Blob of HTML</p></body></html>";

            // Need to accomodate AWS prepending "x-amz-meta-" to the metadata field names.
            string trial_id_key = String.Format("x-amz-meta-{0}", PrintCacheManager.METADATA_TRIAL_ID_LIST_KEY);
            string search_criteria_key = String.Format("x-amz-meta-{0}", PrintCacheManager.METADATA_SEARCH_CRITERIA_KEY);
            const string expectedTrialList = "trial-id-1,trial-id-2";
            const string expectedSearchCriteria = "[{\"Label\":\"Country\",\"Value\":\"Canada\"},{\"Label\":\"Supportive Care\",\"Value\":\"supportive_care\"}]";

            mockClientConfig.Setup(svc => svc.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .Callback((PutObjectRequest request, CancellationToken tok) => actualRequest = request)
                .ReturnsAsync(mockResponse);

            SearchCriteria criteria = SearchCriteriaFactory.Create(null);
            criteria.Add("Country", "Canada");
            criteria.Add("Supportive Care", "supportive_care");

            var cm = new PrintCacheManager(mockClientConfig.Object, "fake-bucket-name");

            await cm.Save(new Guid(expectedKey), new string[] { "trial-id-1", "trial-id-2" }, criteria, expectedBody);

            Assert.NotNull(actualRequest);
            Assert.Equal(expectedKey, actualRequest.Key);
            Assert.Equal(expectedBody, actualRequest.ContentBody);

            Assert.NotEmpty(actualRequest.Metadata.Keys);
            Assert.Contains(trial_id_key, actualRequest.Metadata.Keys);
            Assert.Contains(search_criteria_key, actualRequest.Metadata.Keys);

            Assert.Equal(expectedTrialList, actualRequest.Metadata[trial_id_key]);
            Assert.Equal(expectedSearchCriteria, actualRequest.Metadata[search_criteria_key]);

        }
    }
}
