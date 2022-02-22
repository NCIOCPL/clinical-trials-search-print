using Newtonsoft.Json.Linq;

namespace CancerGov.CTS.Print.Models.Tests
{
    /// <summary>
    /// Verify criteria are selected when a string is used instead of a boolean
    /// for the property (checked) specified as the selectionProperty.
    /// </summary>
    public class TrialTypes_StringSelect : CriteriaList_Base
    {
        public override JObject MockCriteria => JObject.Parse(@"
            {
                ""trialTypes"": [
                    {""label"": ""Treatment"",       ""value"": ""treatment"",       ""checked"": ""true"" },
                    {""label"": ""Prevention"",      ""value"": ""prevention"",      ""checked"": ""false"" },
                    {""label"": ""Supportive Care"", ""value"": ""supportive_care"", ""checked"": ""ignore"" },
                    {""label"": ""Basic Science"",   ""value"": ""basic_science"",   ""checked"": ""yes"" }
                ]
            }");

        public override string ExpectedLabel => SearchCriteriaFactory.LABEL_TRIAL_TYPE;

        public override string ExpectedCriteriaValue => "Treatment, Basic Science";
    }
}
