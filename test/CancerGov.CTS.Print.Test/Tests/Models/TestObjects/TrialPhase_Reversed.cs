using Newtonsoft.Json.Linq;

namespace CancerGov.CTS.Print.Models.Tests
{
    /// <summary>
    /// Reversed from TrialTypes_List.
    /// Verifies that order of the value is comes from the array order.
    /// </summary>
    public class TrialPhase_Reversed : CriteriaList_Base
    {
        public override JObject MockCriteria => JObject.Parse(@"
            {
                ""trialPhases"": [
                    {""label"": ""Phase II"",  ""value"": ""ii"",  ""checked"": true },
                    {""label"": ""Phase I"",   ""value"": ""i"",   ""checked"": true },
                    {""label"": ""Phase III"", ""value"": ""iii"", ""checked"": false },
                    {""label"": ""Phase IV"",  ""value"": ""iv"",  ""checked"": false }
                ]
            }");

        public override string ExpectedLabel => SearchCriteriaFactory.LABEL_TREATMENT_PHASE;

        public override string ExpectedCriteriaValue => "Phase II, Phase I";
    }
}
