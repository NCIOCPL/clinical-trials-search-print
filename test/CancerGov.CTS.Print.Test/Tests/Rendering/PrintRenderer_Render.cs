using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;
using Xunit;

using NCI.Test.IO;

using CancerGov.CTS.Print.Models;

namespace CancerGov.CTS.Print.Rendering.Tests
{
    public class PrintRenderer_Render
    {
        [Fact]
        public void MissingTrialList()
        {
            var renderer = new PrintRenderer("c:\\somepath\\sometemplate.vm");

            var emptyCriteria = new SearchCriteria();

            Assert.Throws<ArgumentNullException>(
                () => renderer.Render(null, emptyCriteria)
            );
        }

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

            Assert.Throws<ArgumentNullException>(
                () => renderer.Render(fakeTrialList, null)
            );
        }

        [Fact]
        public void SimpleRendering()
        {
            string expected = TestFileTools.GetTestFileAsString(typeof(PrintRenderer_Render), Path.Combine(new string[] { "TestData", "Templates", "SimpleRendering.html" }));

            string templatePath = TestFileTools.GetPathToTestFile(typeof(PrintRenderer_Render), Path.Combine(new string[] { "TestData", "Templates", "SimpleRendering.vm" }));
            JObject data = (JObject)TestFileTools.GetTestFileAsJSON(typeof(PrintRenderer_Render), Path.Combine(new string[] { "TestData", "Templates", "SimpleRendering.json" }));

            var emptyCriteria = new SearchCriteria();

            var renderer = new PrintRenderer(templatePath);

            string actual = renderer.Render(data, emptyCriteria);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CriteriaRendering()
        {
            string expected = TestFileTools.GetTestFileAsString(typeof(PrintRenderer_Render), Path.Combine(new string[] { "TestData", "Templates", "CriteriaRendering.html" }));

            string templatePath = TestFileTools.GetPathToTestFile(typeof(PrintRenderer_Render), Path.Combine(new string[] { "TestData", "Templates", "CriteriaRendering.vm" }));
            JObject data = (JObject)TestFileTools.GetTestFileAsJSON(typeof(PrintRenderer_Render), Path.Combine(new string[] { "TestData", "Templates", "CriteriaRendering.json" }));

            var simpleCriteria = new SearchCriteria();
            simpleCriteria.Add("Label 1", "Criterion 1");
            simpleCriteria.Add("Label 2", "Criterion 2");
            simpleCriteria.Add("Label 3", "Criterion 3");

            var renderer = new PrintRenderer(templatePath);

            string actual = renderer.Render(data, simpleCriteria);

            Assert.Equal(expected, actual);
        }
    }
}
