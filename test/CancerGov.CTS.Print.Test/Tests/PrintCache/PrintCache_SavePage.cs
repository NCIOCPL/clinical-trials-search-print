using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Amazon.S3;
using Amazon.S3.Model;
using CancerGov.CTS.Print.DataManager;
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

            // Exact contents don't matter, we just need something.
            string mockMetadata = "{\"trial_ids\": [ \"trial-id-1\", \"trial-id-2\"],\"search_criteria\": null}";

            var cm = new PrintCacheManager(mockClientConfig.Object, "fake-bucket-name");

            PrintSaveFailureException ex = await Assert.ThrowsAsync<PrintSaveFailureException>
                (
                    async () =>
                    {
                        await cm.Save(new Guid(key), mockMetadata, "blob of HTML");
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

            PutObjectResponse mockResponse = new PutObjectResponse
            {
                HttpStatusCode = status
            };

            const string mockBucketName = "fake-bucket-name";

            const string contentKey = "ca761232-ed42-11ce-bacd-00aa0057b223";
            string metadataKey = $"{contentKey}-metadata";  // Can't put this as a const in C# 7.3.

            // The exact strings don't matter, we just need values.
            const string expectedMetadata = "\"trial_ids\": [\"trial-id-1\",\"trial-id-2\"], \"search_criteria\":{\"trialTypes\":[{\"label\":\"Supportive Care\",\"value\":\"supportive_care\",\"checked\":true}],\"location\": \"search-location-country\",\"country\": \"Canada\"}";
            const string expectedBody = "<html><body><p>Blob of HTML</p></body></html>";


            mockClientConfig.Setup(svc => svc.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            var cm = new PrintCacheManager(mockClientConfig.Object, mockBucketName);

            await cm.Save(new Guid(contentKey), expectedMetadata, expectedBody);

            // Verify a request was made to save the metadata.
            mockClientConfig.Verify(svc => svc.PutObjectAsync(
                    It.Is<PutObjectRequest>(req =>
                        req.BucketName == mockBucketName
                        && req.Key == metadataKey
                        && req.ContentBody == expectedMetadata
                        && req.ContentType == "application/json"
                    ),
                    It.IsAny<CancellationToken>()
                ), Times.Once()
            );

            mockClientConfig.Verify(svc => svc.PutObjectAsync(
                    It.Is<PutObjectRequest>(req =>
                        req.BucketName == mockBucketName
                        && req.Key == contentKey
                        && req.ContentBody == expectedBody
                        && req.ContentType == "text/html"
                    ),
                    It.IsAny<CancellationToken>()
                ), Times.Once()
            );

        }
    }
}
