using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CancerGov.ClinicalTrialsAPI.Test.Testdata.LookupInterventionNames
{
    internal class LookupInterventionNames_Multiple : LookupInterventionNames_Base
    {
        public override string ApiResponse => @"
            {
                ""data"": [
                    {
                        ""name"": ""ATM Kinase Inhibitor AZD0156"",
                        ""codes"": [
                            ""C124648""
                        ]
                    },
                    {
                        ""Name"": ""Aerobic Exercise"",
                        ""codes"": [
                            ""C39774""
                        ]
                    },
                    {
                        ""name"": ""Bevacizumab"",
                        ""codes"": [
                            ""C2039""
                        ]
                    }
                ],
                ""total"": 3
            }";

        public override JObject ExpectedResult => JObject.Parse(ApiResponse);
    }
}
