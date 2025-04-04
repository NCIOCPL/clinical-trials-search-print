﻿using System;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;

using Moq;
using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;
using Xunit;

using NCI.Test.IO;
using NCI.Test.Net;

namespace CancerGov.ClinicalTrialsAPI.Test
{
    public class ClinicalTrialsAPIClientTests_GetMultipleTrials
    {
        const string JSON_CONTENT = "application/json";
        const string BASE_URL = "https://example.org/api/v2/";
        const string API_KEY = "key1234";

        [Fact]
        public async Task RequestStructure()
        {
            string[] trialIDs = { "NCI-2014-01507", "NCI-2015-00054", "NCI-2013-00875" };
            JObject expectedBody = JObject.Parse(@"
                {
                    ""from"": 0,
                    ""size"": 3,
                    ""nci_id"": [""NCI-2014-01507"",""NCI-2015-00054"",""NCI-2013-00875""]
                }
            ");

            Mock<IClinicalTrialSearchAPISection> mockConfig = new Mock<IClinicalTrialSearchAPISection>();
            mockConfig.SetupGet(x => x.APIKey).Returns(API_KEY);

            MockHttpMessageHandler mockHandler = new MockHttpMessageHandler();
            ByteArrayContent content = HttpClientMockHelper.CreateResponseBody(JSON_CONTENT, "{\"unimportant\":\"JSON value\"}");

            mockHandler
                .Expect($"{BASE_URL}trials")
                .With(request => request.Method == HttpMethod.Post)
                .WithHeaders("x-api-key", API_KEY)
                .With(request =>
                {
                    string bodyString = request.Content.ReadAsStringAsync().Result;
                    JObject actualBody = JObject.Parse(bodyString);
                    return (new JTokenEqualityComparer()).Equals(expectedBody, actualBody);
                })
                .Respond(HttpStatusCode.OK, content);

            mockHandler.Fallback
                .Respond(HttpStatusCode.OK, content);

            HttpClient mockedClient = new HttpClient(mockHandler);
            mockedClient.BaseAddress = new Uri(BASE_URL);

            ClinicalTrialsAPIClient client = new ClinicalTrialsAPIClient(mockedClient, mockConfig.Object);
            await client.GetMultipleTrials(trialIDs);

            mockHandler.VerifyNoOutstandingExpectation();
        }


        [Theory]
        [InlineData(HttpStatusCode.Unauthorized)] // 401
        [InlineData(HttpStatusCode.Forbidden)] // 403
        [InlineData(HttpStatusCode.NotFound)] // 404
        [InlineData(HttpStatusCode.RequestTimeout)] // 408
        [InlineData(HttpStatusCode.InternalServerError)] //
        [InlineData(HttpStatusCode.BadGateway)] // 502
        [InlineData(HttpStatusCode.ServiceUnavailable)] // 503
        [InlineData(HttpStatusCode.GatewayTimeout)] // 504
        public async Task ServerError(HttpStatusCode status)
        {
            string[] trialIDs = { "NCI-2014-01507", "NCI-2015-00054", "NCI-2013-00875" };

            Mock<IClinicalTrialSearchAPISection> mockConfig = new Mock<IClinicalTrialSearchAPISection>();
            mockConfig.SetupGet(x => x.APIKey).Returns(API_KEY);

            MockHttpMessageHandler mockHandler = new MockHttpMessageHandler();
            ByteArrayContent content = HttpClientMockHelper.CreateResponseBody(JSON_CONTENT, "{\"unimportant\":\"JSON value\"}");

            mockHandler
                .Expect("*")
                .Respond(status, content);

            HttpClient mockedClient = new HttpClient(mockHandler);
            mockedClient.BaseAddress = new Uri(BASE_URL);

            ClinicalTrialsAPIClient client = new ClinicalTrialsAPIClient(mockedClient, mockConfig.Object);

            await Assert.ThrowsAsync<APIServerErrorException>(
                () => client.GetMultipleTrials(trialIDs)
            );
        }

        [Fact]
        public async Task TrialsExist()
        {
            string[] trialIDs = { "NCI-2014-01507", "NCI-2015-00054", "NCI-2013-00875" };

            Mock<IClinicalTrialSearchAPISection> mockConfig = new Mock<IClinicalTrialSearchAPISection>();
            mockConfig.SetupGet(x => x.APIKey).Returns(API_KEY);

            string trialFilePath = TestFileTools.GetPathToTestFile(typeof(ClinicalTrialsAPIClientTests_GetOneTrial), Path.Combine(new string[] { "Testdata", "TrialExamples", "MultipleTrials.json" }));
            JToken expected = TestFileTools.GetTestFileAsJSON(typeof(ClinicalTrialsAPIClientTests_GetOneTrial), Path.Combine(new string[] { "Testdata", "TrialExamples", "MultipleTrials.json" }));

            HttpClient mockedClient = HttpClientMockHelper.GetClientMockForURLWithFileResponse($"{BASE_URL}trials", trialFilePath);
            mockedClient.BaseAddress = new Uri(BASE_URL);

            ClinicalTrialsAPIClient client = new ClinicalTrialsAPIClient(mockedClient, mockConfig.Object);

            JObject actual = await client.GetMultipleTrials(trialIDs);

            Assert.Equal(expected, actual, new JTokenEqualityComparer());
        }


        [Fact]
        public async Task TrialNotFound()
        {
            string[] trialIDs = { "NCI-2014-999999", "NCI-2015-999999", "NCI-2013-999999" };

            Mock<IClinicalTrialSearchAPISection> mockConfig = new Mock<IClinicalTrialSearchAPISection>();
            mockConfig.SetupGet(x => x.APIKey).Returns(API_KEY);

            string trialFilePath = TestFileTools.GetPathToTestFile(typeof(ClinicalTrialsAPIClientTests_GetOneTrial), Path.Combine(new string[] { "Testdata", "TrialExamples", "NotFound-GetMultiple.json" }));
            JToken expected = TestFileTools.GetTestFileAsJSON(trialFilePath);

            HttpClient mockedClient = HttpClientMockHelper.GetClientMockForURLWithFileResponse($"{BASE_URL}trials", trialFilePath, HttpStatusCode.OK);
            mockedClient.BaseAddress = new Uri(BASE_URL);

            ClinicalTrialsAPIClient client = new ClinicalTrialsAPIClient(mockedClient, mockConfig.Object);

            JObject actual = await client.GetMultipleTrials(trialIDs);

            Assert.Equal(expected, actual, new JTokenEqualityComparer());
        }

    }
}
