using System;
using System.IO;

using Newtonsoft.Json.Linq;
using Xunit;

using NCI.Test.IO;

using CancerGov.CTS.Print.Models;

namespace CancerGov.CTS.Print.Rendering.Tests
{
    public class PrintRenderer_Render
    {
        /// <summary>
        /// Fail when no trial list is provided.
        /// </summary>
        [Fact]
        public void MissingTrialList()
        {
            var renderer = new PrintRenderer("c:\\somepath\\sometemplate.vm");

            var emptyCriteria = SearchCriteriaFactory.Create(null);
            var fakeLocationData = LocationCriteriaFactory.Create(null);

            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
                () => renderer.Render(null, emptyCriteria, fakeLocationData, "/fake/link/path/template", "/fake/search/path/link")
            );

            Assert.Equal("trials", ex.ParamName);
        }

        /// <summary>
        /// Fail when no search criteria object is provided.
        /// </summary>
        [Fact]
        public void MissingSearchCriteria()
        {
            var renderer = new PrintRenderer("c:\\somepath\\sometemplate.vm");

            var fakeTrialList = JObject.Parse(@"
                {
                    ""total"": 1,
                    ""data"": [
                        { ""trial_details"": ""go here"" }
                    ]
                }
            ");
            var fakeLocationData = LocationCriteriaFactory.Create(null);

            ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
                () => renderer.Render(fakeTrialList, null, fakeLocationData, "/fake/link/path/template", "/fake/search/path/link")
            );

            Assert.Equal("criteria", ex.ParamName);
        }

        /// <summary>
        /// Fail when the link template isn't relative to the server.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("http://absolute/link")]
        [InlineData("not/relative/to/server")]
        public void BadLinkTemplate(string badPath)
        {
            var renderer = new PrintRenderer("c:\\somepath\\sometemplate.vm");

            var fakeTrialList = JObject.Parse(@"
                {
                    ""total"": 1,
                    ""data"": [
                        { ""trial_details"": ""go here"" }
                    ]
                }
            ");
            var emptyCriteria = SearchCriteriaFactory.Create(null);
            var fakeLocationData = LocationCriteriaFactory.Create(null);

            ArgumentException ex = Assert.Throws<ArgumentException>(
                () => renderer.Render(fakeTrialList, emptyCriteria, fakeLocationData, badPath, "/fake/search/path/link")
            );

            Assert.Equal("linkTemplate", ex.ParamName);
        }

        /// <summary>
        /// Fail when the new search link isn't relative to the server.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("http://absolute/link")]
        [InlineData("not/relative/to/server")]
        public void BadSearchLink(string badPath)
        {
            var renderer = new PrintRenderer("c:\\somepath\\sometemplate.vm");

            var fakeTrialList = JObject.Parse(@"
                {
                    ""total"": 1,
                    ""data"": [
                        { ""trial_details"": ""go here"" }
                    ]
                }
            ");
            var emptyCriteria = SearchCriteriaFactory.Create(null);
            var fakeLocationData = LocationCriteriaFactory.Create(null);

            ArgumentException ex = Assert.Throws<ArgumentException>(
                () => renderer.Render(fakeTrialList, emptyCriteria, fakeLocationData, "/fake/link/path/template", badPath)
            );

            Assert.Equal("newSearchLink", ex.ParamName);
        }

        /// <summary>
        ///  Render something simple.
        /// </summary>
        [Fact]
        public void SimpleRendering()
        {
            string expected = TestFileTools.GetTestFileAsString(typeof(PrintRenderer_Render), Path.Combine(new string[] { "TestData", "Templates", "SimpleRendering.html" }));

            string templatePath = TestFileTools.GetPathToTestFile(typeof(PrintRenderer_Render), Path.Combine(new string[] { "TestData", "Templates", "SimpleRendering.vm" }));
            JObject data = (JObject)TestFileTools.GetTestFileAsJSON(typeof(PrintRenderer_Render), Path.Combine(new string[] { "TestData", "Templates", "SimpleRendering.json" }));

            var emptyCriteria = SearchCriteriaFactory.Create(null);
            var fakeLocationData = LocationCriteriaFactory.Create(null);

            var renderer = new PrintRenderer(templatePath);

            string actual = renderer.Render(data, emptyCriteria, fakeLocationData, "/fake/link/path/template", "/fake/search/path/link");

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Render with criteria.
        /// </summary>
        [Fact]
        public void CriteriaRendering()
        {
            string expected = TestFileTools.GetTestFileAsString(typeof(PrintRenderer_Render), Path.Combine(new string[] { "TestData", "Templates", "CriteriaRendering.html" }));

            var fakeLocationData = LocationCriteriaFactory.Create(null);

            string templatePath = TestFileTools.GetPathToTestFile(typeof(PrintRenderer_Render), Path.Combine(new string[] { "TestData", "Templates", "CriteriaRendering.vm" }));
            JObject data = (JObject)TestFileTools.GetTestFileAsJSON(typeof(PrintRenderer_Render), Path.Combine(new string[] { "TestData", "Templates", "CriteriaRendering.json" }));

            var simpleCriteria = SearchCriteriaFactory.Create(null);
            simpleCriteria.Add("Label 1", "Criterion 1");
            simpleCriteria.Add("Label 2", "Criterion 2");
            simpleCriteria.Add("Label 3", "Criterion 3");

            var renderer = new PrintRenderer(templatePath);

            string actual = renderer.Render(data, simpleCriteria, fakeLocationData, "/fake/link/path/template", "/fake/search/path/link");

            Assert.Equal(expected, actual);
        }
    }
}
