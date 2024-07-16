using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;

using Moq;
using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;
using Xunit;

using NCI.Test.Net;

using CancerGov.ClinicalTrialsAPI.Test.Testdata.LookupDiseaseNames;
using System.Threading.Tasks;

namespace CancerGov.ClinicalTrialsAPI.Test
{
    public class ClinicalTrialsAPIClientTests_LookupDiseaseNames
    {
        const string JSON_CONTENT = "application/json";
        const string BASE_URL = "https://example.org/api/v2/";
        const string API_KEY = "key1234";

        public static IEnumerable<object[]> RequestStructure_Data = new[]
        {
            new object[] { new string[] { "C1234",     "C5678" },     $"{BASE_URL}diseases?codes=C1234&codes=C5678&include=name&include=codes" },
            new object[] { new string[] { " C9012",    "C3456 " },    $"{BASE_URL}diseases?codes=C9012&codes=C3456&include=name&include=codes" },
            new object[] { new string[] { "\tC7890",   "C1234\t" },   $"{BASE_URL}diseases?codes=C7890&codes=C1234&include=name&include=codes" },
            new object[] { new string[] { "\t\tC5678", "C9012\t\t" }, $"{BASE_URL}diseases?codes=C5678&codes=C9012&include=name&include=codes" },
        };

        [Theory]
        [MemberData(nameof(RequestStructure_Data))]
        public async Task RequestStructure(IEnumerable<string> diseaseList, string expectedUrl)
        {
            string expectedString = "{\"expectation\":\"expectation met\"}";
            JObject expectedResult = JObject.Parse(expectedString);

            Mock<IClinicalTrialSearchAPISection> mockConfig = new Mock<IClinicalTrialSearchAPISection>();
            mockConfig.SetupGet(x => x.APIKey).Returns(API_KEY);

            MockHttpMessageHandler mockHandler = new MockHttpMessageHandler();
            ByteArrayContent expectedContent = HttpClientMockHelper.CreateResponseBody(JSON_CONTENT, expectedString);
            ByteArrayContent errorContent = HttpClientMockHelper.CreateResponseBody(JSON_CONTENT, "{\"path\":\"fallback expectation\"}");

            mockHandler
                .Expect(expectedUrl)
                .With(request => request.Method == HttpMethod.Get)
                .WithHeaders("x-api-key", API_KEY)
                .With(request => request.Content == null)
                .Respond(HttpStatusCode.OK, expectedContent);

            mockHandler.Fallback
                .Respond(HttpStatusCode.OK, errorContent);

            HttpClient mockedClient = new HttpClient(mockHandler);
            mockedClient.BaseAddress = new Uri(BASE_URL);

            ClinicalTrialsAPIClient client = new ClinicalTrialsAPIClient(mockedClient, mockConfig.Object);
            JToken result = await client.LookupDiseaseNames(diseaseList);

            Assert.Equal(expectedResult, result, new JTokenEqualityComparer());
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
            string[] diseaseList = { "C1234", "C5678", "C9012", "C3456" };

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
                () => client.LookupDiseaseNames(diseaseList)
            );
        }

        public static IEnumerable<object[]> EmptyArrays = new[]
        {
            new object[] { null },
            new object[] { new string[0] },
        };

        [Theory]
        [MemberData(nameof(EmptyArrays))]
        public async Task EmptyDiseaseList(IEnumerable<string> diseaseList)
        {
            Mock<IClinicalTrialSearchAPISection> mockConfig = new Mock<IClinicalTrialSearchAPISection>();
            mockConfig.SetupGet(x => x.APIKey).Returns(API_KEY);

            MockHttpMessageHandler mockHandler = new MockHttpMessageHandler();

            // No need to set up the handler as it should never be invoked.

            HttpClient mockedClient = new HttpClient(mockHandler);
            mockedClient.BaseAddress = new Uri(BASE_URL);

            ClinicalTrialsAPIClient client = new ClinicalTrialsAPIClient(mockedClient, mockConfig.Object);

            ArgumentNullException ex = await Assert.ThrowsAsync<ArgumentNullException>(
                () => client.LookupDiseaseNames(diseaseList)
            );

            Assert.Equal("diseaseCodes", ex.ParamName);
            Assert.StartsWith(ClinicalTrialsAPIClient.DISEASE_LIST_EMPTY_ARGUMENT_LIST, ex.Message);
        }

        public static IEnumerable<object[]> InvalidConceptIds= new[]
        {
            new object[] { new string[] { "" } },
            new object[] { new string[] { "1234" } },
            new object[] { new string[] { "D1234" } },
            new object[] { new string[] { "d1234" } },
            new object[] { new string[] { "chicken" } },
            new object[] { new string[] { "C1234", "" } },
            new object[] { new string[] { "C1234", "1234" } },
            new object[] { new string[] { "C1234", "D1234" } },
            new object[] { new string[] { "C1234", "d1234" } },
            new object[] { new string[] { "C1234", "chicken" } },
            new object[] { new string[] { "C1234C" } },
            new object[] { new string[] { "C1234C5648" } },
        };

        [Theory]
        [MemberData(nameof(InvalidConceptIds))]
        public async Task InvalidDiseaseList(IEnumerable<string> diseaseList)
        {
            Mock<IClinicalTrialSearchAPISection> mockConfig = new Mock<IClinicalTrialSearchAPISection>();
            mockConfig.SetupGet(x => x.APIKey).Returns(API_KEY);

            MockHttpMessageHandler mockHandler = new MockHttpMessageHandler();

            // No need to set up the handler as it should never be invoked.

            HttpClient mockedClient = new HttpClient(mockHandler);
            mockedClient.BaseAddress = new Uri(BASE_URL);

            ClinicalTrialsAPIClient client = new ClinicalTrialsAPIClient(mockedClient, mockConfig.Object);

            ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(
                () => client.LookupDiseaseNames(diseaseList)
            );

            Assert.Equal("diseaseCodes", ex.ParamName);
            Assert.StartsWith(ClinicalTrialsAPIClient.DISEASE_LIST_INVALID_CCODES, ex.Message);
        }


        public static IEnumerable<object[]> ExistingDiseases = new[]
        {
            new object[] { new LookupDiseaseNames_Single() },
            new object[] { new LookupDiseaseNames_Multiple() },
        };

        [Theory]
        [MemberData(nameof(ExistingDiseases))]
        public async Task DiseaseExists(LookupDiseaseNames_Base data)
        {
            // The actual id list doesn't matter since the simulated response is passed in.
            string[] diseaseIDs = new string[] { "C123", "C456" };

            Mock<IClinicalTrialSearchAPISection> mockConfig = new Mock<IClinicalTrialSearchAPISection>();
            mockConfig.SetupGet(x => x.APIKey).Returns(API_KEY);

            MockHttpMessageHandler mockHandler = new MockHttpMessageHandler();
            ByteArrayContent content = HttpClientMockHelper.CreateResponseBody(JSON_CONTENT, data.ApiResponse);

            mockHandler
                .Expect("*")
                .Respond(HttpStatusCode.OK, content);

            HttpClient mockedClient = new HttpClient(mockHandler);
            mockedClient.BaseAddress = new Uri(BASE_URL);

            ClinicalTrialsAPIClient client = new ClinicalTrialsAPIClient(mockedClient, mockConfig.Object);

            JToken actual = await client.LookupDiseaseNames(diseaseIDs);

            Assert.Equal(data.ExpectedResult, actual, new JTokenEqualityComparer());
        }

    }
}
