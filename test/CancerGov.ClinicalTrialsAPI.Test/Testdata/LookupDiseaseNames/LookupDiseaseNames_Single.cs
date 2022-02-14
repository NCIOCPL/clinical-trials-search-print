using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CancerGov.ClinicalTrialsAPI.Test.Testdata.LookupDiseaseNames
{
    internal class LookupDiseaseNames_Single : LookupDiseaseNames_Base
    {
        public override string ApiResponse => @"
            {
                ""data"": [
                    {
                        ""name"": ""Breast Cancer"",
                        ""codes"": [
                            ""C4872""
                        ]
                    }
                ],
                ""total"": 1
            }";

        public override JObject ExpectedResult => JObject.Parse(ApiResponse);
    }
}
