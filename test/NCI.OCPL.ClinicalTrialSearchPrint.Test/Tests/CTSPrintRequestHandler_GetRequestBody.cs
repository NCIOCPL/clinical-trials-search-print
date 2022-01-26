using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

using NCI.Test.IO;


namespace NCI.OCPL.ClinicalTrialSearchPrint.Test
{
    public class CTSPrintRequestHandler_GetRequestBody
    {

        /// <summary>
        /// Verify GetRequestBody fails appropriately for invalid JSON documents.
        /// </summary>
        [Theory]
        [InlineData("NonJsonBody.txt")]
        [InlineData("BrokenJsonBody.txt")]
        public void NonJsonBody(string bodyDocument)
        {
            Stream body = TestFileTools.GetTestFileAsStream(this.GetType(), Path.Combine(new string[] { "TestData", bodyDocument }));

            CTSPrintRequestHandler handler = new CTSPrintRequestHandler();

            Assert.Throws<JsonReaderException>(
                () => handler.GetRequestBody(body)
            );
        }

        /// <summary>
        /// Verify GetRequestBody is able to correctly parse a valid JSON document.
        /// </summary>
        [Fact]
        public void ValidJsonBody()
        {
            JToken expected = TestFileTools.GetTestFileAsJSON(this.GetType(), Path.Combine(new string[] { "TestData", "ValidRequestBody.txt" }));

            Stream body = TestFileTools.GetTestFileAsStream(this.GetType(), Path.Combine(new string[] { "TestData", "ValidRequestBody.txt" }));

            CTSPrintRequestHandler handler = new CTSPrintRequestHandler();
            JObject actual = handler.GetRequestBody(body);

            Assert.Equal(expected, actual, new JTokenEqualityComparer());
        }
    }
}
