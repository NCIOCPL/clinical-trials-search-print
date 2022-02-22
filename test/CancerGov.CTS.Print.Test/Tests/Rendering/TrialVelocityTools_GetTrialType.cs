using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json.Linq;
using Xunit;

using NCI.Test.IO;

namespace CancerGov.CTS.Print.Rendering.Tests
{
    public class TrialVelocityTools_GetTrialType
    {
        public static IEnumerable<object[]> TrialData = new[]
        {
            new object[] { new TypeIsMissing() },  // primary_purpose node is not present.
            new object[] { new TypeIsNull() },     // primary_purpose node is present, but set to null.
            new object[] { new TypeHasOneWord() }, // primary_purpose node is a single word.
            new object[] { new PhaseHasMultipleWords() }, // primary_purpose node value uses multiple words.
        };

        /// <summary>
        /// Variations on retrieving the trial type.
        /// </summary>
        [Theory]
        [MemberData(nameof(TrialData))]
        public void GetIDs(TestData data)
        {
            string trialFilePath = TestFileTools.GetPathToTestFile(typeof(TrialVelocityTools_GetSecondaryIDsString), Path.Combine(new string[] { "Testdata", "TrialVelocityTools", "GetTrialType", data.DataFileName }));
            JToken testTrial = TestFileTools.GetTestFileAsJSON(trialFilePath);

            var tools = new TrialVelocityTools();
            string actual = tools.GetTrialType((JObject)testTrial);

            Assert.Equal(data.ExpectedResult, actual);
        }

        public abstract class TestData
        {
            /// <summary>
            /// Name of a file containing a test trial.
            /// </summary>
            abstract public string DataFileName { get; }

            /// <summary>
            /// The expected list of IDs in a string.
            /// </summary>
            abstract public string ExpectedResult { get; }
        }

        public class TypeIsMissing : TestData
        {
            override public string DataFileName => "no_type.json";
            override public string ExpectedResult => String.Empty;
        }

        public class TypeIsNull : TestData
        {
            override public string DataFileName => "null_type.json";
            override public string ExpectedResult => String.Empty;
        }

        public class TypeHasOneWord : TestData
        {
            override public string DataFileName => "simple_type.json";
            override public string ExpectedResult => "Treatment";
        }

        public class PhaseHasMultipleWords : TestData
        {
            override public string DataFileName => "multi_word_type.json";
            override public string ExpectedResult => "Supportive care";
        }

    }

}
