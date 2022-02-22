using Newtonsoft.Json.Linq;

namespace CancerGov.CTS.Print.Models.Tests
{
    /// <summary>
    /// Regular list of trial types.
    /// </summary>
    public class TrialTypes_List : CriteriaList_Base
    {
        public override JObject MockCriteria => JObject.Parse(@"
            {
                ""trialTypes"": [
                    {""label"": ""Treatment"",       ""value"": ""treatment"",       ""checked"": true },
                    {""label"": ""Prevention"",      ""value"": ""prevention"",      ""checked"": false },
                    {""label"": ""Supportive Care"", ""value"": ""supportive_care"", ""checked"": false },
                    {""label"": ""Basic Science"",   ""value"": ""basic_science"",   ""checked"": true }
                ]
            }");

        public override string ExpectedLabel => SearchCriteriaFactory.LABEL_TRIAL_TYPE;

        public override string ExpectedCriteriaValue => "Treatment, Basic Science";
    }
}
