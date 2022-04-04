using Newtonsoft.Json.Linq;
using Xunit;


namespace CancerGov.CTS.Print.Rendering.Tests
{
    public class VelocityTools_HtmlEncode
    {
        /// <summary>
        /// Verify various blank/empty strings are not encoded.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("       ")]
        public void EmptyStrings(string input)
        {
            var tools = new VelocityTemplate.VelocityTools();

            string actual = tools.HtmlEncode(input);

            Assert.Equal(input, actual);
        }

        /// <summary>
        /// Verify various blank/empty strings are not encoded. JValue version.
        /// </summary>
        [Theory]
        [InlineData("null", null)]  // Non-obvious way to encode a standalone null value, as opposed to one in a structure.
        [InlineData("\"\"", "")]
        [InlineData("\"       \"", "       ")]
        public void EmptyStrings_JValue(string input, string expected)
        {
            var tools = new VelocityTemplate.VelocityTools();

            JValue val = (JValue)JToken.Parse(input);
            string actual = tools.HtmlEncode(val);

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Verify strings with no special characters are unchanged.
        /// </summary>
        [Theory]
        [InlineData("chicken")]
        [InlineData("This **is** _markdown_ and has no HTML special characters.")]
        [InlineData("This is a multicenter, Phase 2a, open-label, 2-part study to investigate the safety, tolerability, and anti-tumor activity of ZW25 (zanidatamab) in combination with palbociclib plus fulvestrant. Eligible patients include those with locally advanced (unresectable) and / or metastatic human epidermal growth factor receptor 2 (HER2)-positive, hormone receptor (HR)-positive breast cancer.")]
        public void NoSpecialCharacters(string input)
        {
            var tools = new VelocityTemplate.VelocityTools();

            string actual = tools.HtmlEncode(input);

            Assert.Equal(input, actual);
        }

        /// <summary>
        /// Verify strings with no special characters are unchanged. JValue version.
        /// </summary>
        [Theory]
        [InlineData("\"chicken\"", "chicken")]
        [InlineData("\"This **is** _markdown_ and has no HTML special characters.\"", "This **is** _markdown_ and has no HTML special characters.")]
        [InlineData("\"This is a multicenter, Phase 2a, open-label, 2-part study to investigate the safety, tolerability, and anti-tumor activity of ZW25 (zanidatamab) in combination with palbociclib plus fulvestrant. Eligible patients include those with locally advanced (unresectable) and / or metastatic human epidermal growth factor receptor 2 (HER2)-positive, hormone receptor (HR)-positive breast cancer.\"",      "This is a multicenter, Phase 2a, open-label, 2-part study to investigate the safety, tolerability, and anti-tumor activity of ZW25 (zanidatamab) in combination with palbociclib plus fulvestrant. Eligible patients include those with locally advanced (unresectable) and / or metastatic human epidermal growth factor receptor 2 (HER2)-positive, hormone receptor (HR)-positive breast cancer.")]
        public void NoSpecialCharacters_JValue(string input, string expected)
        {
            var tools = new VelocityTemplate.VelocityTools();

            JValue val = (JValue)JToken.Parse(input);
            string actual = tools.HtmlEncode(val);

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Verify strings with special characters are encoded.
        /// </summary>
        [Theory]
        [InlineData("Age <= 35 && Age > 20", "Age &lt;= 35 &amp;&amp; Age &gt; 20")]
        [InlineData("(e.g., history of severe infusion reactions to trastuzumab, >/= Grade 2 peripheral neuropathy, or platelet count < 100 x 10^9/L)", "(e.g., history of severe infusion reactions to trastuzumab, &gt;/= Grade 2 peripheral neuropathy, or platelet count &lt; 100 x 10^9/L)")]
        [InlineData("Prior treatment with chemotherapy, other anti-cancer therapy not otherwise specified, or hormonal cancer therapy </= 3 weeks before the first dose of ZW25", "Prior treatment with chemotherapy, other anti-cancer therapy not otherwise specified, or hormonal cancer therapy &lt;/= 3 weeks before the first dose of ZW25")]
        public void SpecialCharacters(string input, string expected)
        {
            var tools = new VelocityTemplate.VelocityTools();

            string actual = tools.HtmlEncode(input);

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Verify strings with special characters are encoded. JValue version
        /// </summary>
        [Theory]
        [InlineData("\"Age <= 35 && Age > 20\"", "Age &lt;= 35 &amp;&amp; Age &gt; 20")]
        [InlineData("\"(e.g., history of severe infusion reactions to trastuzumab, >/= Grade 2 peripheral neuropathy, or platelet count < 100 x 10^9/L)\"", "(e.g., history of severe infusion reactions to trastuzumab, &gt;/= Grade 2 peripheral neuropathy, or platelet count &lt; 100 x 10^9/L)")]
        [InlineData("\"Prior treatment with chemotherapy, other anti-cancer therapy not otherwise specified, or hormonal cancer therapy </= 3 weeks before the first dose of ZW25\"",      "Prior treatment with chemotherapy, other anti-cancer therapy not otherwise specified, or hormonal cancer therapy &lt;/= 3 weeks before the first dose of ZW25")]
        public void SpecialCharacters_JValue(string input, string expected)
        {
            var tools = new VelocityTemplate.VelocityTools();

            JValue val = (JValue)JToken.Parse(input);
            string actual = tools.HtmlEncode(val);

            Assert.Equal(expected, actual);
        }


    }
}
