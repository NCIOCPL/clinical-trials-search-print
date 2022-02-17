using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CancerGov.ClinicalTrialsAPI.Test.Testdata.LookupInterventionNames
{
    internal class LookupInterventionNames_Single : LookupInterventionNames_Base
    {
        public override string ApiResponse => @"
            {
                ""data"": [
                    {
                        ""name"": ""Bevacizumab"",
                        ""codes"": [
                            ""C2039""
                        ]
                }
                ],
                ""total"": 1
            }";

        public override JObject ExpectedResult => JObject.Parse(ApiResponse);
    }
}
