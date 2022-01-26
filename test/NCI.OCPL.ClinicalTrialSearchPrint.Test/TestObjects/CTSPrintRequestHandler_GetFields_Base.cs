using System;
using Newtonsoft.Json.Linq;

namespace NCI.OCPL.ClinicalTrialSearchPrint.Test
{
    public abstract class CTSPrintRequestHandler_GetFields_Base
    {
        public abstract JObject RequestData { get; }

        // The Expected.... fields don't need to be overridden unless a test
        // case uses that particular value. Rather than 
        public virtual string[] ExpectedTrials => new string[] { "This is the default value." };

        public virtual string ExpectedLinkTemplate => "This is the defaualt value.";

        public virtual string ExpectedNewSearchLink => "This is the default value.";

        public virtual JObject ExpectedSearchCriteria => JObject.Parse("{\"default-key\": \"This is the default value.\"}");
    }
}
