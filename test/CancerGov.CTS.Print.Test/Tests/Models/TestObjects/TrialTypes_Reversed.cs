using Newtonsoft.Json.Linq;

namespace CancerGov.CTS.Print.Models.Tests
{
    /// <summary>
    /// Reversed from TrialTypes_List.
    /// Verifies that order of the value is comes from the array order.
    /// </summary>
    public class TrialTypes_Reversed : CriteriaList_Base
    {
        public override JObject MockCriteria => JObject.Parse(@"
            {
                ""trialTypes"": [
                    {""label"": ""Basic Science"",   ""value"": ""basic_science"",   ""checked"": true },
                    {""label"": ""Prevention"",      ""value"": ""prevention"",      ""checked"": false },
                    {""label"": ""Supportive Care"", ""value"": ""supportive_care"", ""checked"": false },
                    {""label"": ""Treatment"",       ""value"": ""treatment"",       ""checked"": true }
                ]
            }");

        public override string ExpectedLabel => SearchCriteriaFactory.LABEL_TRIAL_TYPE;

        public override string ExpectedCriteriaValue => "Basic Science, Treatment";
    }
}
