﻿using System;
using System.IO;
using System.Net.Http;
using System.Net;

using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;
using Xunit;

using NCI.Test.IO;
using NCI.Test.Net;

namespace CancerGov.ClinicalTrialsAPI.Test
{
    public class ClinicalTrialsAPIClientTests_GetOneTrial
    {
        const string JSON_CONTENT = "application/json";
        const string BASE_URL = "https://example.org/api/v2/";
        const string API_KEY = "key1234";

        [Fact]
        async public void RequestStructure()
        {
            MockHttpMessageHandler mockHandler = new MockHttpMessageHandler();
            ByteArrayContent content = HttpClientMockHelper.CreateResponseBody(JSON_CONTENT, "{\"unimportant\":\"JSON value\"}");

            mockHandler
                .Expect($"{BASE_URL}trials/NCT1234")
                .With(request => request.Method == HttpMethod.Get)
                .WithHeaders("x-api-key", API_KEY)
                .Respond(HttpStatusCode.OK, content);

            mockHandler.Fallback
                .Respond(HttpStatusCode.OK, content);

            HttpClient mockedClient = new HttpClient(mockHandler);

            ClinicalTrialsAPIClient client = new ClinicalTrialsAPIClient(mockedClient, BASE_URL, API_KEY);
            await client.GetOneTrial("NCT1234");

            mockHandler.VerifyNoOutstandingExpectation();
        }


        [Theory]
        [InlineData(HttpStatusCode.Unauthorized)] // 401
        [InlineData(HttpStatusCode.Forbidden)] // 403
        [InlineData(HttpStatusCode.RequestTimeout)] // 408
        [InlineData(HttpStatusCode.InternalServerError)] //
        [InlineData(HttpStatusCode.BadGateway)] // 502
        [InlineData(HttpStatusCode.ServiceUnavailable)] // 503
        [InlineData(HttpStatusCode.GatewayTimeout)] // 504
        async public void ServerError(HttpStatusCode status)
        {
            MockHttpMessageHandler mockHandler = new MockHttpMessageHandler();
            ByteArrayContent content = HttpClientMockHelper.CreateResponseBody(JSON_CONTENT, "{\"unimportant\":\"JSON value\"}");

            mockHandler
                .Expect("*")
                .Respond(status, content);

            HttpClient mockedClient = new HttpClient(mockHandler);

            ClinicalTrialsAPIClient client = new ClinicalTrialsAPIClient(mockedClient, BASE_URL, API_KEY);

            await Assert.ThrowsAsync<APIServerErrorException>(
                ()=> client.GetOneTrial("NCT1234")
            );

        }


        [Fact]
        async public void TrialExists()
        {
            string trialID = "NCT02194738";

            string trialFilePath = TestFileTools.GetPathToTestFile(typeof(ClinicalTrialsAPIClientTests_GetOneTrial), Path.Combine(new string[] { "TrialExamples", trialID + ".json" }));
            JToken expected = TestFileTools.GetTestFileAsJSON(typeof(ClinicalTrialsAPIClientTests_GetOneTrial), Path.Combine(new string[] { "TrialExamples", trialID + ".json" }));

            HttpClient mockedClient = HttpClientMockHelper.GetClientMockForURLWithFileResponse(String.Format("{0}trials/{1}", BASE_URL, trialID), trialFilePath);

            ClinicalTrialsAPIClient client = new ClinicalTrialsAPIClient(mockedClient, BASE_URL, API_KEY);

            JObject actual= await client.GetOneTrial(trialID);

            Assert.Equal(trialID, actual["nct_id"]);
            Assert.Equal(expected, actual, new JTokenEqualityComparer());
        }


        [Fact]
        async public void TrialNotFound()
        {
            string trialID = "NCT0999999999";

            string trialFilePath = TestFileTools.GetPathToTestFile(typeof(ClinicalTrialsAPIClientTests_GetOneTrial), Path.Combine(new string[] { "TrialExamples", "NotFound-GetOne.json" }));

            HttpClient mockedClient = HttpClientMockHelper.GetClientMockForURLWithFileResponse(String.Format("{0}trials/{1}", BASE_URL, trialID), trialFilePath, HttpStatusCode.NotFound);

            ClinicalTrialsAPIClient client = new ClinicalTrialsAPIClient(mockedClient, BASE_URL, API_KEY);

            JObject actual = await client.GetOneTrial(trialID);

            Assert.Null(actual);
        }

    }
}
