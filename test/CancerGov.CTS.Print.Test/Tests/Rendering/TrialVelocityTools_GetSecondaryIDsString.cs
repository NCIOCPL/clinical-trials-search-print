using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json.Linq;
using Xunit;

using NCI.Test.IO;

namespace CancerGov.CTS.Print.Rendering.Tests
{
    public class TrialVelocityTools_GetSecondaryIDsString
    {
        public static IEnumerable<object[]> TrialData = new[]
        {
            new object[] { new CCRIDIsMissing() },  // ccr_id node is missing instead of null.
            new object[] { new OtherIsMissing() },  // other_ids node is not present.
            new object[] { new OtherIsNull() },     // other_ids node is present, but set to null.
            new object[] { new OtherHasOneValue() },// other_ids node has one entry.
            new object[] { new OtherHasMultipleValues() }, // other_ids node has multiple entries.
        };

        /// <summary>
        /// Handle variations on gathering secondary IDs.
        /// </summary>
        [Theory]
        [MemberData(nameof(TrialData))]
        public void GetIDs(TestData data)
        {
            string trialFilePath = TestFileTools.GetPathToTestFile(typeof(TrialVelocityTools_GetSecondaryIDsString), Path.Combine(new string[] { "Testdata", "TrialVelocityTools", "GetSecondaryIDsString", data.DataFileName }));
            JToken testTrial = TestFileTools.GetTestFileAsJSON(trialFilePath);

            var tools = new TrialVelocityTools();
            string actual = tools.GetSecondaryIDsString((JObject)testTrial);

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

        public class CCRIDIsMissing : TestData
        {
            override public string DataFileName => "secondary_id_missing_ccrid.json";
            override public string ExpectedResult => "NCI-2015-01906, s16-01365";
        }

        public class OtherIsMissing : TestData
        {
            override public string DataFileName => "secondary_id_other_ids_missing.json";
            override public string ExpectedResult => "NCI-2017-01240";
        }

        public class OtherIsNull : TestData
        {
            override public string DataFileName => "secondary_id_other_ids_null.json";
            override public string ExpectedResult => "NCI-2018-03695";
        }

        public class OtherHasOneValue : TestData
        {
            override public string DataFileName => "secondary_id_other_ids_single.json";
            override public string ExpectedResult => "NCI-2015-01906, s16-01365";
        }

        public class OtherHasMultipleValues : TestData
        {
            override public string DataFileName => "secondary_id_other_ids_multiple.json";
            override public string ExpectedResult => "NCI-2014-00635, NCT02179164, RTOG-1305, s16-00025";
        }

    }

}
