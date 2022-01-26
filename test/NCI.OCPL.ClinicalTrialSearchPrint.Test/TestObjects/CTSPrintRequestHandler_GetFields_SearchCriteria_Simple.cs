using Newtonsoft.Json.Linq;

namespace NCI.OCPL.ClinicalTrialSearchPrint.Test
{
    class CTSPrintRequestHandler_GetFields_SearchCriteria_Simple
        : CTSPrintRequestHandler_GetFields_Base
    {
        public override JObject RequestData =>
            JObject.Parse(@"
{
    ""trial_ids"": [""NCI-2015-01906""],
    ""link_template"": ""/about-cancer/treatment/clinical-trials/search/v?t=C3208&st=C3471&a=24&loc=1&z=20850&zp=100&rl=2&id=<TRIAL_ID>"",
    ""new_search_link"": ""/about-cancer/treatment/clinical-trials/search/advanced"",
    ""search_criteria"": {
                            ""nci_id"": ""NCI-2015-01906"",
                            ""criteria_2"": ""search value 2""
                         }
}
");

        public override JObject ExpectedSearchCriteria => JObject.Parse(@"
            {
                ""nci_id"": ""NCI-2015-01906"",
                ""criteria_2"": ""search value 2""
            }");
    }
}
