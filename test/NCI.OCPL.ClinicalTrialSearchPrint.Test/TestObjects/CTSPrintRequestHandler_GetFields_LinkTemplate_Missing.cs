using Newtonsoft.Json.Linq;

namespace NCI.OCPL.ClinicalTrialSearchPrint.Test
{
    class CTSPrintRequestHandler_GetFields_LinkTemplate_Missing
        : CTSPrintRequestHandler_GetFields_Base
    {
        public override JObject RequestData =>
            JObject.Parse(@"
{
    ""trial_ids"": [""NCI-2015-01906""],
    ""new_search_link"": ""/about-cancer/treatment/clinical-trials/search/advanced"",
    ""search_criteria"": null
}
");

        public override string[] ExpectedTrials => new string[] { "NCI-2015-01906" };
    }
}
