using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CancerGov.ClinicalTrialsAPI.Test.Testdata.LookupDiseaseNames
{
    internal class LookupDiseaseNames_Multiple : LookupDiseaseNames_Base
    {
        public override string ApiResponse => @"
            {
                ""data"": [
                    {
                        ""name"": ""Breast Cancer"",
                        ""codes"": [
                            ""C4872""
                        ]
                    },
                    {
                        ""name"": ""Invasive Breast Cancer"",
                        ""codes"": [
                            ""C9245""
                        ]
                    },
                    {
                        ""name"": ""Stage I Non-Small Cell Lung Cancer"",
                        ""codes"": [
                            ""C6671""
                        ]
                    },
                    {
                        ""name"": ""Stage IB Lung Cancer"",
                        ""codes"": [
                            ""C5643""
                        ]
            }
                ],
                ""total "": 4
            }";

        public override JObject ExpectedResult => JObject.Parse(ApiResponse);
    }
}
