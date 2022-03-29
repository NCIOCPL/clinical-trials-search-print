using Newtonsoft.Json.Linq;
using Xunit;


namespace NCI.OCPL.ClinicalTrialSearchPrint.Test
{
    public class CTSPrintRequestHandler_SetLocationStateNames
    {

        /// <summary>
        /// Verify handling of no org_state_or_province element.
        /// </summary>
        [Fact]
        public void SingleTrialSingleSiteNoState()
        {
            JObject trials = JObject.Parse(@"
                {
                    ""total"": 1,
                    ""data"": [
                        {
                            ""sites"": [
                                {
                                    ""contact_name"": ""Site Public Contact"",
                                    ""contact_phone"": ""907-212-6871"",
                                    ""recruitment_status_date"": ""2022-02-23"",
                                    ""org_address_line_2"": null,
                                    ""org_va"": false,
                                    ""org_address_line_1"": ""3200 Providence Drive"",
                                    ""org_tty"": null,
                                    ""org_family"": null,
                                    ""org_postal_code"": ""99508"",
                                    ""contact_email"": ""AKPAMC.OncologyResearchSupport@providence.org"",
                                    ""recruitment_status"": ""ACTIVE"",
                                    ""org_city"": ""Anchorage"",
                                    ""org_email"": ""AKPAMC.OncologyResearchSupport@providence.org"",
                                    ""org_country"": ""United States"",
                                    ""org_fax"": null,
                                    ""org_phone"": ""907-261-3109\r907-224-5205"",
                                    ""org_name"": ""Providence Alaska Medical Center"",
                                    ""org_coordinates"": {
                                        ""lon"": -149.8115,
                                        ""lat"": 61.198
                                    }
                                }
                            ],
                            ""nci_id"": ""NCI-2015-00054""
                        }
                    ]
                }");

            CTSPrintRequestHandler handler = new CTSPrintRequestHandler();
            handler.SetLocationStateNames(trials);

            Assert.Null(trials["data"][0]["sites"][0]["org_state_or_province"]);
        }

        [Fact]
        public void SingleTrialSingleSite()
        {
            JObject trials = JObject.Parse(@"
                {
                    ""total"": 1,
                    ""data"": [
                        {
                            ""sites"": [
                                {
                                    ""org_state_or_province"": ""AK"",
                                    ""contact_name"": ""Site Public Contact"",
                                    ""contact_phone"": ""907-212-6871"",
                                    ""recruitment_status_date"": ""2022-02-23"",
                                    ""org_address_line_2"": null,
                                    ""org_va"": false,
                                    ""org_address_line_1"": ""3200 Providence Drive"",
                                    ""org_tty"": null,
                                    ""org_family"": null,
                                    ""org_postal_code"": ""99508"",
                                    ""contact_email"": ""AKPAMC.OncologyResearchSupport@providence.org"",
                                    ""recruitment_status"": ""ACTIVE"",
                                    ""org_city"": ""Anchorage"",
                                    ""org_email"": ""AKPAMC.OncologyResearchSupport@providence.org"",
                                    ""org_country"": ""United States"",
                                    ""org_fax"": null,
                                    ""org_phone"": ""907-261-3109\r907-224-5205"",
                                    ""org_name"": ""Providence Alaska Medical Center"",
                                    ""org_coordinates"": {
                                        ""lon"": -149.8115,
                                        ""lat"": 61.198
                                    }
                                }
                            ],
                            ""nci_id"": ""NCI-2015-00054""
                        }
                    ]
                }");

            CTSPrintRequestHandler handler = new CTSPrintRequestHandler();
            handler.SetLocationStateNames(trials);

            Assert.Equal("Alaska", trials["data"][0]["sites"][0]["org_state_or_province"].Value<string>());
        }


        [Fact]
        public void MultipleTrialsMultipleSites()
        {
            JObject mockTrials = JObject.Parse(@"
                {
                    ""total"": 1,
                    ""data"": [
                        {
                            ""sites"": [
                                {
                                    ""org_state_or_province"": ""AK"",
                                    ""contact_name"": ""Site Public Contact"",
                                    ""contact_phone"": ""907-212-6871"",
                                    ""recruitment_status_date"": ""2022-02-23"",
                                    ""org_address_line_2"": null,
                                    ""org_va"": false,
                                    ""org_address_line_1"": ""3200 Providence Drive"",
                                    ""org_tty"": null,
                                    ""org_family"": null,
                                    ""org_postal_code"": ""99508"",
                                    ""contact_email"": ""AKPAMC.OncologyResearchSupport@providence.org"",
                                    ""recruitment_status"": ""ACTIVE"",
                                    ""org_city"": ""Anchorage"",
                                    ""org_email"": ""AKPAMC.OncologyResearchSupport@providence.org"",
                                    ""org_country"": ""United States"",
                                    ""org_fax"": null,
                                    ""org_phone"": ""907-261-3109\r907-224-5205"",
                                    ""org_name"": ""Providence Alaska Medical Center"",
                                    ""org_coordinates"": {
                                        ""lon"": -149.8115,
                                        ""lat"": 61.198
                                    }
                                },
                                {
                                    ""org_state_or_province"": ""AL"",
                                    ""contact_name"": ""Site Public Contact"",
                                    ""contact_phone"": ""251-435-3942"",
                                    ""recruitment_status_date"": ""2022-02-23"",
                                    ""org_address_line_2"": null,
                                    ""org_va"": false,
                                    ""org_address_line_1"": ""Five Mobile Infirmary Circle"",
                                    ""org_tty"": null,
                                    ""org_family"": null,
                                    ""org_postal_code"": ""36607"",
                                    ""contact_email"": null,
                                    ""recruitment_status"": ""ACTIVE"",
                                    ""org_city"": ""Mobile"",
                                    ""org_email"": null,
                                    ""org_country"": ""United States"",
                                    ""org_fax"": null,
                                    ""org_phone"": ""251-435-3942"",
                                    ""org_name"": ""Mobile Infirmary Medical Center"",
                                    ""org_coordinates"": {
                                        ""lon"": -88.1057,
                                        ""lat"": 30.6982
                                    }
                                }
                            ],
                            ""nci_id"": ""NCI-2015-00054""
                        },
                        {
                            ""sites"": [
                                {
                                    ""org_state_or_province"": ""NH"",
                                    ""contact_name"": ""Site Public Contact"",
                                    ""contact_phone"": ""603-577-4282"",
                                    ""recruitment_status_date"": ""2020-03-10"",
                                    ""org_address_line_2"": null,
                                    ""org_va"": false,
                                    ""org_address_line_1"": ""2300 Southwood Drive"",
                                    ""org_tty"": null,
                                    ""org_family"": ""Norris Cotton Cancer Center"",
                                    ""org_postal_code"": ""03063"",
                                    ""contact_email"": ""Laura.A.Menken@hitchcock.org"",
                                    ""recruitment_status"": ""ACTIVE"",
                                    ""org_city"": ""Nashua"",
                                    ""org_email"": ""Laura.A.Menken@hitchcock.org"",
                                    ""org_country"": ""United States"",
                                    ""org_fax"": null,
                                    ""org_phone"": null,
                                    ""org_name"": ""Norris Cotton Cancer Center-Nashua"",
                                    ""org_coordinates"": {
                                        ""lon"": -71.5136,
                                        ""lat"": 42.7771
                                    }
                                },
                                {
                                    ""org_state_or_province"": ""CT"",
                                    ""contact_name"": ""Site Public Contact"",
                                    ""contact_phone"": ""203-785-5702"",
                                    ""recruitment_status_date"": ""2021-11-12"",
                                    ""org_address_line_2"": null,
                                    ""org_va"": false,
                                    ""org_address_line_1"": ""230 Waterford Parkway South"",
                                    ""org_tty"": null,
                                    ""org_family"": ""Yale Cancer Center"",
                                    ""org_postal_code"": ""06385"",
                                    ""contact_email"": ""canceranswers@yale.edu"",
                                    ""recruitment_status"": ""ACTIVE"",
                                    ""org_city"": ""Waterford"",
                                    ""org_email"": ""canceranswers@yale.edu"",
                                    ""org_country"": ""United States"",
                                    ""org_fax"": null,
                                    ""org_phone"": ""860-444-3744"",
                                    ""org_name"": ""Smilow Cancer Hospital Care Center - Waterford"",
                                    ""org_coordinates"": {
                                        ""lon"": -72.1558,
                                        ""lat"": 41.3583
                                    }
                                }
                            ],
                            ""nci_id"": ""NCI-2014-01509""
                        }
                    ]
                }");

            CTSPrintRequestHandler handler = new CTSPrintRequestHandler();
            handler.SetLocationStateNames(mockTrials);

            // First trial (NCI-2015-00054)
            Assert.Equal("Alaska", mockTrials["data"][0]["sites"][0]["org_state_or_province"].Value<string>());
            Assert.Equal("Alabama", mockTrials["data"][0]["sites"][1]["org_state_or_province"].Value<string>());

            // Second trial (NCI-2014-01509)
            Assert.Equal("New Hampshire", mockTrials["data"][1]["sites"][0]["org_state_or_province"].Value<string>());
            Assert.Equal("Connecticut", mockTrials["data"][1]["sites"][1]["org_state_or_province"].Value<string>());
        }

    }
}
