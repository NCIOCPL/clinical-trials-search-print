using System;
using System.Net.Http;

using Moq;
using Xunit;

namespace CancerGov.ClinicalTrialsAPI.Test
{
    public class ClinicalTrialsAPIClientTests_General
    {
        [Fact]
        public void NoHttpClient()
        {
            Mock<IClinicalTrialSearchAPISection> mockConfig = new Mock<IClinicalTrialSearchAPISection>();  

            ArgumentNullException ex = Assert.Throws<ArgumentNullException>
            (
                () => new ClinicalTrialsAPIClient(null, mockConfig.Object)
            );

            Assert.Equal("client", ex.ParamName);
            Assert.StartsWith(ClinicalTrialsAPIClient.ARGUMENT_NOT_NULL_MSG, ex.Message);
        }


        [Fact]
        public void NoConfig()
        {
            HttpClient dummyClient = new HttpClient();

            ArgumentNullException ex = Assert.Throws<ArgumentNullException>
            (
                () => new ClinicalTrialsAPIClient(dummyClient, null)
            );

            Assert.Equal("config", ex.ParamName);
            Assert.StartsWith(ClinicalTrialsAPIClient.ARGUMENT_NOT_NULL_MSG, ex.Message);
        }

        [Fact]
        public void BaseAddressNotSet()
        {
            Mock<IClinicalTrialSearchAPISection> mockConfig = new Mock<IClinicalTrialSearchAPISection>();

            HttpClient dummyClient = new HttpClient();

            ArgumentException ex = Assert.Throws<ArgumentException>
            (
                () => new ClinicalTrialsAPIClient(dummyClient, mockConfig.Object)
            );

            Assert.Equal("client", ex.ParamName);
            Assert.StartsWith(ClinicalTrialsAPIClient.BASE_ADDRESS_REQUIRED_MSG, ex.Message);
        }

        [Fact]
        public void SuccessfulInstantiation()
        {
            Mock<IClinicalTrialSearchAPISection> mockConfig = new Mock<IClinicalTrialSearchAPISection>();

            HttpClient dummyClient = new HttpClient();
            dummyClient.BaseAddress = new Uri("https://some.server/some/path/");

            ClinicalTrialsAPIClient apiClient = new ClinicalTrialsAPIClient(dummyClient, mockConfig.Object);

            Assert.NotNull(apiClient);
        }
    }
}
