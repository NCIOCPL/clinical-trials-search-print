using System;
using System.Configuration;

using Xunit;

namespace CancerGov.ClinicalTrialsAPI.Test
{
    /// These tests are tightly coupled to the project's app.config.
    /// 
    /// Changes to the tests must, must, must also be reflected in the config file, and vice versa.

    // Some of these tests play with an environment variable. We therefore want to disable parallelization
    // for this specific class just in case we later add another set of tests which also change the
    // same variable.
    [CollectionDefinition(nameof(ClinicalTrialSearchAPISection_Test), DisableParallelization = true)]
    public class ClinicalTrialSearchAPISection_Test
    {
        [Fact]
        public void Instance()
        {
            ClinicalTrialSearchAPISection config = ClinicalTrialSearchAPISection.Instance;

            Assert.NotNull(config);
        }

        [Fact]
        public void BaseUrl()
        {
            string expected = "https://someserver/some-path";

            ClinicalTrialSearchAPISection config = ClinicalTrialSearchAPISection.Instance;

            string actual = config.BaseUrl;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ApiKeyVariableName()
        {
            string expected = "ClinicalTrialsAPIKey_VAR_TEST_KEY";

            ClinicalTrialSearchAPISection config = ClinicalTrialSearchAPISection.Instance;

            string actual = config.APIKeyVariableName;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void APIKey_NotSet()
        {
            // According to the docs (https://xunit.net/docs/running-tests-in-parallel.html)
            // tests within a single test class do not run in parallel. We're going to be
            // paranoid here and explicitly clear the variable before running, just in case
            // we add another test class at some future time that messes with this.
            Environment.SetEnvironmentVariable("ClinicalTrialsAPIKey_VAR_TEST_KEY", null, EnvironmentVariableTarget.Process);

            ClinicalTrialSearchAPISection config = ClinicalTrialSearchAPISection.Instance;

            Assert.Throws<ConfigurationErrorsException>(
                () => config.APIKey
            );
        }

        [Fact]
        public void APIKey_Set()
        {
            // Set the expected value to a random-ish value.
            string expected = Guid.NewGuid().ToString();

            Environment.SetEnvironmentVariable("ClinicalTrialsAPIKey_VAR_TEST_KEY", expected, EnvironmentVariableTarget.Process);

            ClinicalTrialSearchAPISection config = ClinicalTrialSearchAPISection.Instance;

            string actual = config.APIKey;

            Assert.Equal(expected, actual);

            // Cleanup
            Environment.SetEnvironmentVariable("ClinicalTrialsAPIKey_VAR_TEST_KEY", null, EnvironmentVariableTarget.Process);
        }
    }
}
