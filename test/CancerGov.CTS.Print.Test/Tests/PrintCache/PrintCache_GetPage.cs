using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using CancerGov.CTS.Print.DataManager;
using CancerGov.CTS.Print.Models;
using Moq;
using Xunit;


namespace CancerGov.CTS.Print.Test.Tests.PrintCache
{
    public class PrintCache_GetPage
    {
        /// <summary>
        /// Test handling of requests for non-existant pages.
        /// </summary>
        [Fact]
        public async Task DocumentNotFound()
        {
            Mock<IAmazonS3> mockClientConfig = new Mock<IAmazonS3>();
            mockClientConfig.Setup(svc => svc.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                // Internally, when the document doesn't exist, AmazonS3Client.GetObjectAsync() throws AmazonS3Exception
                // instead of returning a response object with a 404 status.
                .ThrowsAsync(new AmazonS3Exception("The specified key does not exist.")
                {
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorCode = "NoSuchKey"
                });

            Guid key = new Guid("ca761232-ed42-11ce-bacd-00aa0057b223");

            SearchCriteria criteria = SearchCriteriaFactory.Create(null);

            var cm = new PrintCacheManager(mockClientConfig.Object, "fake-bucket-name");

            string pageValue = await cm.GetPage(key);
            Assert.Null(pageValue);
        }

        /// <summary>
        /// Test handling of system errors (errors other than file not found.
        /// </summary>
        [Theory]
        [InlineData(HttpStatusCode.BadRequest)] // 400
        [InlineData(HttpStatusCode.Forbidden)]  // 403
        [InlineData(HttpStatusCode.InternalServerError)]// 500
        [InlineData(HttpStatusCode.GatewayTimeout)]     // 504
        public async Task SystemErors(HttpStatusCode status)
        {
            Mock<IAmazonS3> mockClientConfig = new Mock<IAmazonS3>();
            mockClientConfig.Setup(svc => svc.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                // Internally, when the document doesn't exist, AmazonS3Client.GetObjectAsync() throws AmazonS3Exception
                // instead of returning a response object with a 404 status.
                .ThrowsAsync(new AmazonS3Exception("kaboom!")
                {
                    StatusCode = status,
                    ErrorCode = status.ToString()
                });

            Guid key = new Guid("ca761232-ed42-11ce-bacd-00aa0057b223");

            SearchCriteria criteria = SearchCriteriaFactory.Create(null);

            var cm = new PrintCacheManager(mockClientConfig.Object, "fake-bucket-name");

            AmazonS3Exception ex = await Assert.ThrowsAsync<AmazonS3Exception>
                (
                    // Not worryinag about the return value because we expect an exception.
                    async () => await cm.GetPage(key)
                );

            Assert.Equal(status, ex.StatusCode);
            Assert.Equal(status.ToString(), ex.ErrorCode);
        }

        /// <summary>
        /// Test structure of a straightforward document retrieval.
        /// </summary>
        [Fact]
        public async Task SuccessfulRetrieval()
        {
            const string expectedKey = "ca761232-ed42-11ce-bacd-00aa0057b223";
            const string expectedBody = "<html><body><p>Blob of HTML</p></body></html>";

            Mock<IAmazonS3> mockClientConfig = new Mock<IAmazonS3>();

            GetObjectRequest actualRequest = null;
            GetObjectResponse mockResponse = new GetObjectResponse
            {
                HttpStatusCode = HttpStatusCode.OK,
                ResponseStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedBody)),
            };

            mockClientConfig.Setup(svc => svc.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                .Callback((GetObjectRequest request, CancellationToken token) => actualRequest = request)
                .ReturnsAsync(mockResponse);

            var cm = new PrintCacheManager(mockClientConfig.Object, "fake-bucket-name");

            string actualDocument = await cm.GetPage(new Guid(expectedKey));

            Assert.Equal(expectedBody, actualDocument);
            Assert.Equal(expectedKey, actualRequest.Key);
        }

    }
}
