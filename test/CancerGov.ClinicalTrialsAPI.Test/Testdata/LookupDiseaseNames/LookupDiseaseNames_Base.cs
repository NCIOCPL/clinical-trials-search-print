using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace CancerGov.ClinicalTrialsAPI.Test.Testdata.LookupDiseaseNames
{
    public abstract class LookupDiseaseNames_Base
    {
        /// <summary>
        /// The simulated response from the API server.
        /// </summary>
        public abstract string ApiResponse { get; }

        /// <summary>
        /// The expected output from the call to LookupDiseaseNames().
        /// </summary>
        public abstract JObject ExpectedResult { get; }
    }
}
