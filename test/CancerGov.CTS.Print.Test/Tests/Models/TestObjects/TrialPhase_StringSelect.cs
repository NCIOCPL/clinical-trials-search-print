using Newtonsoft.Json.Linq;

namespace CancerGov.CTS.Print.Models.Tests
{
    /// <summary>
    /// Regular list of trial types.
    /// </summary>
    public class TrialPhase_StringSelect : CriteriaList_Base
    {
        public override JObject MockCriteria => JObject.Parse(@"
            {
                ""trialPhases"": [
                    {""label"": ""Phase I"",   ""value"": ""i"",   ""checked"": ""true"" },
                    {""label"": ""Phase II"",  ""value"": ""ii"",  ""checked"": ""yes"" },
                    {""label"": ""Phase III"", ""value"": ""iii"", ""checked"": ""false"" },
                    {""label"": ""Phase IV"",  ""value"": ""iv"",  ""checked"": ""ignore"" }
                ]
            }");

        public override string ExpectedLabel => SearchCriteriaFactory.LABEL_TREATMENT_PHASE;

        public override string ExpectedCriteriaValue => "Phase I, Phase II";
    }
}
