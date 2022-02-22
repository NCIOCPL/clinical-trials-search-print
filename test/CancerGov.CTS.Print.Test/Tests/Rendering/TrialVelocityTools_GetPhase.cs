using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json.Linq;
using Xunit;

using NCI.Test.IO;

namespace CancerGov.CTS.Print.Rendering.Tests
{
    public class TrialVelocityTools_GetPhase
    {
        public static IEnumerable<object[]> TrialData = new[]
        {
            new object[] { new PhaseIsMissing() },  // phase node is not present.
            new object[] { new PhaseIsNull() },     // phase node is present, but set to null.
            new object[] { new PhaseHasOneValue() },// phase node is a single value.
            new object[] { new PhaseHasMultipleValues() }, // phase node value spans multiple phases.
        };

        /// <summary>
        /// Verify conversion of trial phases.
        /// </summary>
        [Theory]
        [MemberData(nameof(TrialData))]
        public void GetIDs(TestData data)
        {
            string trialFilePath = TestFileTools.GetPathToTestFile(typeof(TrialVelocityTools_GetSecondaryIDsString), Path.Combine(new string[] { "Testdata", "TrialVelocityTools", "GetPhase", data.DataFileName }));
            JToken testTrial = TestFileTools.GetTestFileAsJSON(trialFilePath);

            var tools = new TrialVelocityTools();
            string actual = tools.GetPhase((JObject)testTrial);

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

        public class PhaseIsMissing : TestData
        {
            override public string DataFileName => "no_phase.json";
            override public string ExpectedResult => String.Empty;
        }

        public class PhaseIsNull : TestData
        {
            override public string DataFileName => "null_phase.json";
            override public string ExpectedResult => String.Empty;
        }

        public class PhaseHasOneValue : TestData
        {
            override public string DataFileName => "single_phase.json";
            override public string ExpectedResult => "Phase II";
        }

        public class PhaseHasMultipleValues : TestData
        {
            override public string DataFileName => "multiple_phase.json";
            override public string ExpectedResult => "Phase II/III";
        }

    }

}
