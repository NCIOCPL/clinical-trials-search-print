using System.Collections.Generic;

using Newtonsoft.Json.Linq;
using Xunit;

namespace CancerGov.CTS.Print.Models.Tests
{
    public class SearchCriteriaFactory_Create
    {
        /// <summary>
        /// Verify no search criteria entries for empty JSON.
        /// </summary>
        [Fact]
        public void NoCriteria()
        {
            JObject emptyCriteria = new JObject
            {
            };

            var criteria = SearchCriteriaFactory.Create(emptyCriteria);

            Assert.False(criteria.HasCriteria);
            Assert.Empty(criteria.Criteria);
        }

        /// <summary>
        /// Verify handling of the cancerType criterion.
        /// </summary>
        [Fact]
        public void CancerType()
        {
            var mockCriteria = JObject.FromObject(new
            {
                cancerType = new
                {
                    name = "Acute Myeloid Leukemia (AML)",
                    codes = new List<string>
                    {
                        "C3171"
                    }
                }
            });

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.True(criteria.HasCriteria);
            Assert.Single(criteria.Criteria);

            Criterion crit = criteria.Criteria[0];
            Assert.Equal(SearchCriteriaFactory.LABEL_CANCER_TYPE, crit.Label);
            Assert.Equal("Acute Myeloid Leukemia (AML)", crit.Value);
        }

        /// <summary>
        /// Verify handling of the cancer subtype criterion.
        /// </summary>
        [Fact]
        public void CancerSubtypes()
        {
            var mockCriteria = JObject.Parse(@"
            {
                ""cancerType"": {""name"": ""Acute Myeloid Leukemia (AML)"", ""codes"": [""C3171""] },
                ""subtypes"": [
                    {""name"": ""Refractory Acute Myeloid Leukemia (AML)"", ""codes"": [""C134319""] },
                    {""name"": ""Acute Erythroid Leukemia"", ""codes"": [""C8923""] },
                ]
            }");

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.True(criteria.HasCriteria);
            Assert.Equal(2, criteria.Criteria.Count);

            Criterion crit = criteria.Criteria[0];
            Assert.Equal(SearchCriteriaFactory.LABEL_CANCER_TYPE, crit.Label);
            Assert.Equal("Acute Myeloid Leukemia (AML)", crit.Value);

            crit = criteria.Criteria[1];
            Assert.Equal(SearchCriteriaFactory.LABEL_CANCER_SUBTYPE, crit.Label);
            Assert.Equal("Refractory Acute Myeloid Leukemia (AML), Acute Erythroid Leukemia", crit.Value);
        }

        /// <summary>
        /// Verify handling of the cancer stage criterion.
        /// </summary>
        [Fact]
        public void CancerStages()
        {
            var mockCriteria = JObject.Parse(@"
            {
                ""cancerType"": {""name"": ""Appendix Cancer"", ""codes"": [""C9330""] },
                ""stages"": [
                    {""name"": ""Stage IV Appendix Cancer"", ""codes"": [""C134129""] },
                    {""name"": ""Stage IVA Appendix Cancer"", ""codes"": [""C87806"", ""C134130""] },
                ]
            }");

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.True(criteria.HasCriteria);
            Assert.Equal(2, criteria.Criteria.Count);

            Criterion crit = criteria.Criteria[0];
            Assert.Equal(SearchCriteriaFactory.LABEL_CANCER_TYPE, crit.Label);
            Assert.Equal("Appendix Cancer", crit.Value);

            crit = criteria.Criteria[1];
            Assert.Equal(SearchCriteriaFactory.LABEL_CANCER_STAGES, crit.Label);
            Assert.Equal("Stage IV Appendix Cancer, Stage IVA Appendix Cancer", crit.Value);
        }

        /// <summary>
        /// Verify handling of the side effects, biomarker, participant attributes criterion.
        /// </summary>
        [Fact]
        public void CancerSideEffectsBiomarkersParticipantAttributes()
        {
            var mockCriteria = JObject.Parse(@"
            {
                ""cancerType"": {""name"": ""Acute Myeloid Leukemia (AML)"", ""codes"": [""C3171""] },
                ""findings"": [
                    {""name"": ""Acute Myeloid Leukemia in Remission"", ""codes"": [""C3588""] },
                    {""name"": ""Blasts 10 Percent or More of Peripheral Blood White Cells"", ""codes"": [""C138056""] },
                ]
            }");

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.True(criteria.HasCriteria);
            Assert.Equal(2, criteria.Criteria.Count);

            Criterion crit = criteria.Criteria[0];
            Assert.Equal(SearchCriteriaFactory.LABEL_CANCER_TYPE, crit.Label);
            Assert.Equal("Acute Myeloid Leukemia (AML)", crit.Value);

            crit = criteria.Criteria[1];
            Assert.Equal(SearchCriteriaFactory.LABEL_CANCER_SIDE_EFFECTS_BIOMARKERS_ATTRIBUTES, crit.Label);
            Assert.Equal("Acute Myeloid Leukemia in Remission, Blasts 10 Percent or More of Peripheral Blood White Cells", crit.Value);
        }

        /// <summary>
        /// Verify handling of the age criterion.
        /// </summary>
        [Fact]
        public void Age()
        {
            var mockCriteria = JObject.FromObject(new
            {
                age = 34
            });

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.True(criteria.HasCriteria);
            Assert.Single(criteria.Criteria);

            Criterion crit = criteria.Criteria[0];
            Assert.Equal(SearchCriteriaFactory.LABEL_AGE, crit.Label);
            Assert.Equal("34", crit.Value);
        }

        /// <summary>
        /// Verify handling of an age criterion with the wrong shape.
        /// </summary>
        [Fact]
        public void Age_BadShape()
        {
            var mockCriteria = JObject.FromObject(new
            {
                age = new
                {
                    years = 34
                }
            });

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.False(criteria.HasCriteria);
            Assert.Empty(criteria.Criteria);
        }

        /// <summary>
        /// Verify handling of the keywords or phrases criterion.
        /// </summary>
        [Fact]
        public void KeyWords()
        {
            var mockCriteria = JObject.FromObject(new
            {
                keywordPhrases = "Breast Cancer"
            });

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.True(criteria.HasCriteria);
            Assert.Single(criteria.Criteria);

            Criterion crit = criteria.Criteria[0];
            Assert.Equal(SearchCriteriaFactory.LABEL_KEYWORDS, crit.Label);
            Assert.Equal("Breast Cancer", crit.Value);
        }

        /// <summary>
        /// Verify special characters in the keywords are properly encoded.
        /// </summary>
        [Fact]
        public void KeyWords_Encoding()
        {
            var mockCriteria = JObject.FromObject(new
            {
                keywordPhrases = "Age <= 35 && Age > 20"
            });

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.True(criteria.HasCriteria);
            Assert.Single(criteria.Criteria);

            Criterion crit = criteria.Criteria[0];
            Assert.Equal(SearchCriteriaFactory.LABEL_KEYWORDS, crit.Label);
            Assert.Equal("Age &lt;= 35 &amp;&amp; Age &gt; 20", crit.Value);
        }

        /// <summary>
        /// Don't explode if the keywords and phrases criterion is an unexpected shape.
        /// </summary>
        [Fact]
        public void KeyWords_BadShape()
        {
            var mockCriteria = JObject.FromObject(new
            {
                keywordPhrases = new List<string>
                { "Word 1", "Word 2", "Word 3" }
            });

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.False(criteria.HasCriteria);
            Assert.Empty(criteria.Criteria);
        }

        /// <summary>
        /// Verify output when vaOnly is true.
        /// </summary>
        [Fact]
        public void VAOnly_True()
        {
            var mockCriteria = JObject.FromObject(new
            {
                vaOnly = true
            });

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.True(criteria.HasCriteria);
            Assert.Single(criteria.Criteria);

            Criterion crit = criteria.Criteria[0];
            Assert.Equal(SearchCriteriaFactory.LABEL_VA_ONLY, crit.Label);
            Assert.Equal(SearchCriteriaFactory.TEXT_VA_ONLY_TRUE, crit.Value);
        }

        /// <summary>
        /// vaOnly always has a value, verify that false results in nothing being added.
        /// </summary>
        [Fact]
        public void VAOnly_False()
        {
            var mockCriteria = JObject.FromObject(new
            {
                vaOnly = false
            });

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.False(criteria.HasCriteria);
            Assert.Empty(criteria.Criteria);
        }

        /// <summary>
        /// Don't crash if vaOnly is non-boolean.
        /// </summary>
        [Fact]
        public void VAOnly_NonBoolean()
        {
            var mockCriteria = JObject.FromObject(new
            {
                vaOnly = "Text"
            });

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.False(criteria.HasCriteria);
            Assert.Empty(criteria.Criteria);
        }

        /// <summary>
        /// Verify search-location-all leads to no location criteria being recorded.
        /// </summary>
        [Fact]
        public void Location_All()
        {
            var mockCriteria = JObject.Parse(
            @"{
                ""location"" : ""search-location-all"",
                ""zip"" : 20852,
                ""zipRadius"" : 250,
                ""zipCoords"" : {
                    ""lat"" : ""39.0003"",
                    ""long"" : ""-77.1056""
                }
            }");

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.False(criteria.HasCriteria);
            Assert.Empty(criteria.Criteria);
        }

        /// <summary>
        /// Verify handling of search-location-zip.
        /// </summary>
        [Fact]
        public void Location_Zip()
        {
            var mockCriteria = JObject.Parse(
            @"{
                ""location"" : ""search-location-zip"",
                ""zip"" : 20852,
                ""zipRadius"" : 250,
                ""zipCoords"" : {
                    ""lat"" : ""39.0003"",
                    ""long"" : ""-77.1056""
                }
            }");

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.True(criteria.HasCriteria);
            Assert.Single(criteria.Criteria);

            Criterion crit = criteria.Criteria[0];
            Assert.Equal(SearchCriteriaFactory.LABEL_LOCATION_ZIP, crit.Label);
            Assert.Equal("within 250 miles of 20852", crit.Value);
        }

        /// <summary>
        /// Verify search-location-zip with no zip code results in no criteria being recorded.
        /// </summary>
        [Fact]
        public void Location_Zip_NoZip()
        {
            var mockCriteria = JObject.Parse(
            @"{
                ""location"" : ""search-location-zip"",
                ""zip"" : """",
                ""zipRadius"" : 250,
                ""zipCoords"" : {
                    ""lat"" : ""39.0003"",
                    ""long"" : ""-77.1056""
                }
            }");

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.False(criteria.HasCriteria);
            Assert.Empty(criteria.Criteria);
        }

        /// <summary>
        /// Verify handing of search-location-country.
        /// </summary>
        [Fact]
        public void Location_Country()
        {
            var mockCriteria = JObject.FromObject(new
            {
                location = "search-location-country",
                country = "United States",
                states = new[]
                {
                    new {
                        abbr = "MD",
                        name = "Maryland"
                    },
                    new {
                        abbr = "IL",
                        name = "Illinois"
                    },
                    new {
                        abbr = "DC",
                        name = "District of Columbia"
                    }
                },
                city = "Washington",
                zip = "20850"
            });

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.True(criteria.HasCriteria);
            Assert.Equal(3, criteria.Criteria.Count);

            Criterion crit = criteria.Criteria[0];
            Assert.Equal(SearchCriteriaFactory.LABEL_LOCATION_COUNTRY, crit.Label);
            Assert.Equal("United States", crit.Value);

            crit = criteria.Criteria[1];
            Assert.Equal(SearchCriteriaFactory.LABEL_LOCATION_STATE, crit.Label);
            Assert.Equal("District of Columbia, Illinois, Maryland", crit.Value);

            crit = criteria.Criteria[2];
            Assert.Equal(SearchCriteriaFactory.LABEL_LOCATION_CITY, crit.Label);
            Assert.Equal("Washington", crit.Value);
        }

        /// <summary>
        /// Verify handing of search-location-hospital.
        /// </summary>
        [Fact]
        public void Location_Hospital()
        {
            var mockCriteria = JObject.FromObject(new
            {
                location = "search-location-hospital",
                hospital = new {
                    term = "St. Eligius",
                    termKey = "elsewhere"
                }
            });

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.True(criteria.HasCriteria);
            Assert.Single(criteria.Criteria);

            Criterion crit = criteria.Criteria[0];
            Assert.Equal(SearchCriteriaFactory.LABEL_LOCATION_HOSPITAL, crit.Label);
            Assert.Equal("St. Eligius", crit.Value);
        }

        /// <summary>
        /// Verify handling of search-location-nih.
        /// </summary>
        [Fact]
        public void Location_AtNIH()
        {
            var mockCriteria = JObject.FromObject(new
            {
                location = "search-location-nih"
            });

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.True(criteria.HasCriteria);
            Assert.Single(criteria.Criteria);

            Criterion crit = criteria.Criteria[0];
            Assert.Equal(SearchCriteriaFactory.LABEL_LOCATION_AT_NIH, crit.Label);
            Assert.Equal(SearchCriteriaFactory.TEXT_LOCATION_AT_NIH, crit.Value);
        }

        /// <summary>
        /// Verify handing of healthy volunteers criteria when set true.
        /// </summary>
        [Fact]
        public void HealthyVolunteers_True()
        {
            var mockCriteria = JObject.FromObject(new
            {
                healthyVolunteers = true
            });

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.True(criteria.HasCriteria);
            Assert.Single(criteria.Criteria);

            Criterion crit = criteria.Criteria[0];
            Assert.Equal(SearchCriteriaFactory.LABEL_HEALTHY_VOLUNTEERS, crit.Label);
            Assert.Equal(SearchCriteriaFactory.TEXT_LIMIT_TO_HEALTHY_VOLUNTEERS, crit.Value);
        }

        /// <summary>
        /// healthyVolunteers always has a value, verify that false results in nothing being added.
        /// </summary>
        [Fact]
        public void HealthyVolunteers_False()
        {
            var mockCriteria = JObject.FromObject(new
            {
                healthyVolunteers = false
            });

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.False(criteria.HasCriteria);
            Assert.Empty(criteria.Criteria);
        }

        /// <summary>
        /// Verify handling of an empty trialTypes list.
        /// </summary>
        [Fact]
        public void TrialTypes_EmptyList()
        {
            var mockCriteria = JObject.Parse(@"
            {
                ""trialTypes"": []
            }");

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.False(criteria.HasCriteria);
            Assert.Empty(criteria.Criteria);
        }

        /// <summary>
        /// Verify handling of a trial types list with nothing selected.
        /// </summary>
        [Fact]
        public void TrialTypes_NothingChecked()
        {
            var mockCriteria = JObject.Parse(@"
            {
                ""trialTypes"": [
                    {""label"": ""Treatment"",       ""value"": ""treatment"",       ""checked"": false },
                    {""label"": ""Prevention"",      ""value"": ""prevention"",      ""checked"": false },
                    {""label"": ""Supportive Care"", ""value"": ""supportive_care"", ""checked"": false },
                    {""label"": ""Basic Science"",   ""value"": ""basic_science"",   ""checked"": false }
                ]
            }");

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.False(criteria.HasCriteria);
            Assert.Empty(criteria.Criteria);
        }

        public static IEnumerable<object[]> TrialType_Data = new[]
        {
            new object[] { new TrialTypes_List() },
            new object[] { new TrialTypes_Reversed() },
            new object[] { new TrialTypes_StringSelect() },
        };

        /// <summary>
        /// Verify handling of a trial types list with some items selected.
        /// </summary>
        /// <param name="data"></param>
        [Theory]
#pragma warning disable xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
        [MemberData(nameof(TrialType_Data))]
#pragma warning restore xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
        public void TrialTypes_ItemsChecked(CriteriaList_Base data)
        {
            var mockCriteria = data.MockCriteria;

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.True(criteria.HasCriteria);
            Assert.Single(criteria.Criteria);

            Criterion crit = criteria.Criteria[0];
            Assert.Equal(data.ExpectedLabel, crit.Label);
            Assert.Equal(data.ExpectedCriteriaValue, crit.Value);
        }

        /// <summary>
        /// Don't explode if the trial types criterion has an unexpected shape.
        /// </summary>
        [Fact]
        public void TrialTypes_WrongShape()
        {
            var mockCriteria = JObject.Parse(@"
            {
                ""trialTypes"": {
                    ""label"": ""Treatment"", ""value"": ""treatment"", ""checked"": true
                }
            }");

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.False(criteria.HasCriteria);
            Assert.Empty(criteria.Criteria);
        }

        /// <summary>
        /// Verify handling of a drug list.
        /// </summary>
        [Fact]
        public void DrugList()
        {
            var mockCriteria = JObject.Parse(@"
            {
                ""drugs"": [
                    {""name"": ""Bevacizumab"", ""codes"": [""C2039""] },
                    {""name"": ""Aspirin"", ""codes"": [""C287""] },
                ]
            }");

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.True(criteria.HasCriteria);
            Assert.Single(criteria.Criteria);

            Criterion crit = criteria.Criteria[0];
            Assert.Equal(SearchCriteriaFactory.LABEL_DRUG_LIST, crit.Label);
            Assert.Equal("Bevacizumab, Aspirin", crit.Value);
        }

        public static IEnumerable<object[]> TrialPhase_Data = new[]
        {
            new object[] { new TrialPhase_List() },
            new object[] { new TrialPhase_Reversed() },
            new object[] { new TrialPhase_StringSelect() },
        };

        /// <summary>
        /// Handle variations of a valid trial phase list.
        /// </summary>
        [Theory]
#pragma warning disable xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
        [MemberData(nameof(TrialPhase_Data))]
#pragma warning restore xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
        public void TrialPhases(CriteriaList_Base data)
        {
            var mockCriteria = data.MockCriteria;

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.True(criteria.HasCriteria);
            Assert.Single(criteria.Criteria);

            Criterion crit = criteria.Criteria[0];
            Assert.Equal(data.ExpectedLabel, crit.Label);
            Assert.Equal(data.ExpectedCriteriaValue, crit.Value);
        }

        /// <summary>
        /// Handle the trialID criterion.
        /// </summary>
        [Fact]
        public void TrialID()
        {
            var mockCriteria = JObject.FromObject(new
            {
                trialId = "NCI-2015-00054, ea5163"
            });

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.True(criteria.HasCriteria);
            Assert.Single(criteria.Criteria);

            Criterion crit = criteria.Criteria[0];
            Assert.Equal(SearchCriteriaFactory.LABEL_TRIAL_ID, crit.Label);
            Assert.Equal("NCI-2015-00054, ea5163", crit.Value);
        }

        /// <summary>
        /// Handle the trial investigator criterion.
        /// </summary>
        [Fact]
        public void TrialInvestigators()
        {
            var mockCriteria = JObject.FromObject(new
            {
                investigator = new {
                    term = "Hossein Borghaei",
                    termKey = "hossein_borghaei"
                }
            });

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.True(criteria.HasCriteria);
            Assert.Single(criteria.Criteria);

            Criterion crit = criteria.Criteria[0];
            Assert.Equal(SearchCriteriaFactory.LABEL_TRIAL_INVESTIGATORS, crit.Label);
            Assert.Equal("Hossein Borghaei", crit.Value);
        }

        /// <summary>
        /// Handle the lead organization criterion.
        /// </summary>
        [Fact]
        public void LeadOrganization()
        {
            var mockCriteria = JObject.FromObject(new
            {
                leadOrg = new
                {
                    term = "University of Texas MD Anderson Cancer Center LAO",
                    termKey = "university_texas_md_anderson_cancer_center_lao"
                }
            });

            var criteria = SearchCriteriaFactory.Create(mockCriteria);

            Assert.True(criteria.HasCriteria);
            Assert.Single(criteria.Criteria);

            Criterion crit = criteria.Criteria[0];
            Assert.Equal(SearchCriteriaFactory.LABEL_TRIAL_LEAD_ORG, crit.Label);
            Assert.Equal("University of Texas MD Anderson Cancer Center LAO", crit.Value);
        }

    }
}
