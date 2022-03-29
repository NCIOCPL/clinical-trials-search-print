using Newtonsoft.Json.Linq;
using Xunit;

namespace NCI.OCPL.ClinicalTrialSearchPrint.Test
{
    public class CTSPrintRequestHandler_RemoveNonRecruitingSites
    {
        /// <summary>
        /// Verify removal of sites with non-recruiting statuses and only
        /// non-recruiting statuses.
        /// </summary>
        [Fact]
        public void Remove()
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
                                },
                                {
                                    ""org_state_or_province"": ""DC"",
                                    ""contact_name"": ""Site Public Contact"",
                                    ""contact_phone"": ""202-444-2223"",
                                    ""recruitment_status_date"": ""2021-04-26"",
                                    ""org_address_line_2"": ""Lombardi Comprehensive Cancer Center"",
                                    ""org_va"": false,
                                    ""org_address_line_1"": ""3800 Reservoir Road Northwest"",
                                    ""org_tty"": null,
                                    ""org_family"": ""Georgetown Lombardi Comprehensive Cancer Center"",
                                    ""org_postal_code"": ""20007"",
                                    ""contact_email"": null,
                                    ""recruitment_status"": ""CLOSED_TO_ACCRUAL"",
                                    ""org_city"": ""Washington"",
                                    ""org_email"": null,
                                    ""org_country"": ""United States"",
                                    ""org_fax"": ""202-784-4344"",
                                    ""org_phone"": ""202-444-0381\r202-444-4000"",
                                    ""org_name"": ""MedStar Georgetown University Hospital"",
                                    ""org_coordinates"": {
                                        ""lon"": -77.0771,
                                        ""lat"": 38.9147
                                    }
                                },
                                {
                                    ""org_state_or_province"": ""DC"",
                                    ""contact_name"": ""Site Public Contact"",
                                    ""contact_phone"": ""202-877-8839"",
                                    ""recruitment_status_date"": ""2017-09-29"",
                                    ""org_address_line_2"": null,
                                    ""org_va"": false,
                                    ""org_address_line_1"": ""110 Irving Street Northwest"",
                                    ""org_tty"": null,
                                    ""org_family"": ""Georgetown Lombardi Comprehensive Cancer Center"",
                                    ""org_postal_code"": ""20010"",
                                    ""contact_email"": null,
                                    ""recruitment_status"": ""ADMINISTRATIVELY_COMPLETE"",
                                    ""org_city"": ""Washington"",
                                    ""org_email"": null,
                                    ""org_country"": ""United States"",
                                    ""org_fax"": null,
                                    ""org_phone"": ""202-877-8839"",
                                    ""org_name"": ""MedStar Washington Hospital Center"",
                                    ""org_coordinates"": {
                                        ""lon"": -77.028,
                                        ""lat"": 38.9322
                                    }
                                },
                                {
                                    ""org_state_or_province"": ""NC"",
                                    ""contact_name"": ""Site Public Contact"",
                                    ""contact_phone"": ""252-975-4308"",
                                    ""recruitment_status_date"": ""2018-10-08"",
                                    ""org_address_line_2"": null,
                                    ""org_va"": false,
                                    ""org_address_line_1"": ""1209 Brown Street"",
                                    ""org_tty"": null,
                                    ""org_family"": ""UNC Lineberger Comprehensive Cancer Center"",
                                    ""org_postal_code"": ""27889"",
                                    ""contact_email"": null,
                                    ""recruitment_status"": ""CLOSED_TO_ACCRUAL"",
                                    ""org_city"": ""Washington"",
                                    ""org_email"": null,
                                    ""org_country"": ""United States"",
                                    ""org_fax"": null,
                                    ""org_phone"": ""252-975-4308"",
                                    ""org_name"": ""Marion L Shepard Cancer Center at Vidant Beaufort Hospital"",
                                    ""org_coordinates"": {
                                        ""lon"": -77.0955,
                                        ""lat"": 35.5828
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

            JObject expected = JObject.Parse(@"
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
            handler.RemoveNonRecruitingSites(mockTrials);

            Assert.Equal(expected, mockTrials, new JTokenEqualityComparer());
        }
    }
}
