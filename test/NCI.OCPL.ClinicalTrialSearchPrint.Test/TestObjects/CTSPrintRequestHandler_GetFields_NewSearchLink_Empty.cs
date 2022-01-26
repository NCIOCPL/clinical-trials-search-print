using Newtonsoft.Json.Linq;

namespace NCI.OCPL.ClinicalTrialSearchPrint.Test
{
    class CTSPrintRequestHandler_GetFields_NewSearchLink_Empty
        : CTSPrintRequestHandler_GetFields_Base
    {
        public override JObject RequestData =>
            JObject.Parse(@"
{
    ""trial_ids"": [""NCI-2015-01906""],
    ""link_template"": ""/about-cancer/treatment/clinical-trials/search/v?t=C3208&st=C3471&a=24&loc=1&z=20850&zp=100&rl=2&id=<TRIAL_ID>"",
    ""new_search_link"": """",
    ""search_criteria"": null
}
");

        // This value is tightly coupled to the /appSettings/defaultNewSearchLink value, the CTSPrintRequestHandler_GetFields::NewSearchLinkMissing test,
        // and the test's data classes.
        // Any cnanges to this value must mirrored in all of these locations.
        public override string ExpectedNewSearchLink => "/default/new/search/page"; 
    }
}
