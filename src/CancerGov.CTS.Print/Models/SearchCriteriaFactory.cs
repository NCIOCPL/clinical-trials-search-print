using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Newtonsoft.Json.Linq;

namespace CancerGov.CTS.Print.Models
{
    public static class SearchCriteriaFactory
    {
        public const string LABEL_CANCER_TYPE = "Primary Cancer Type/Condition";
        public const string LABEL_CANCER_SUBTYPE = "Subtype";
        public const string LABEL_CANCER_STAGES = "Stage";
        public const string LABEL_CANCER_SIDE_EFFECTS_BIOMARKERS_ATTRIBUTES = "Side Effects / Biomarkers / Participant Attributes";
        public const string LABEL_AGE = "Age";
        public const string LABEL_KEYWORDS = "Keywords/Phrases";
        public const string LABEL_VA_ONLY = "Veterans Affairs Facilities";
        public const string TEXT_VA_ONLY_TRUE = "Results limited to trials at Veterans Affairs facilities";
        public const string TEXT_VA_ONLY_FALSE = "Match all trial sites";
        public const string LABEL_LOCATION_ZIP = "Near ZIP Code";
        public const string LABEL_LOCATION_COUNTRY = "Country";
        public const string LABEL_LOCATION_STATE = "State";
        public const string LABEL_LOCATION_CITY = "City";
        public const string LABEL_LOCATION_HOSPITAL = "At Hospital/Institution";
        public const string LABEL_LOCATION_AT_NIH = "At NIH";
        public const string TEXT_LOCATION_AT_NIH = "Only show trials at the NIH Clinical Center (Bethesda, MD)";
        public const string LABEL_HEALTHY_VOLUNTEERS = "Healthy Volunteers";
        public const string TEXT_LIMIT_TO_HEALTHY_VOLUNTEERS = "Results limited to trials accepting healthy volunteers";
        public const string LABEL_TRIAL_TYPE = "Trial Type";
        public const string LABEL_DRUG_LIST = "Drug/Drug Family";
        public const string LABEL_TREATMENT_PHASE = "Trial Phase";
        public const string LABEL_TRIAL_ID = "Trial ID";
        public const string LABEL_TRIAL_INVESTIGATORS = "Trial Investigators";
        public const string LABEL_TRIAL_LEAD_ORG = "Lead Organization";

        /// <summary>
        /// Encapsulates the logic for creating a SearchCriteria object. Use this instead
        /// of constructing a SearchCriteria object directly.
        /// </summary>
        /// <param name="rawCriteria"></param>
        /// <returns></returns>
        public static SearchCriteria Create(JObject rawCriteria)
        {
            SearchCriteria criteria = new SearchCriteria();

            if (rawCriteria != null)
            {
                /*
                 *  NOTE: The previous wcms-cde implementation of the search criteria output included logic for multiple
                 *        possible outputs for the vaOnly (two strings) and healthyVolunteers (three strings). These are
                 *        boolean flags and newer implemntations only sent the values when they are set to true, with the
                 *        result that the other outputs were never used. In order to accomdate the newest implmentation
                 *        with the values always being set, the outputs for these criteria are based on the value being
                 *        present *AND* being set to true.
                 */

                GetCriterion(rawCriteria, criteria, LABEL_CANCER_TYPE, "cancerType", "name");
                GetCriteriaList(rawCriteria, criteria, LABEL_CANCER_SUBTYPE, "subtypes", "name");
                GetCriteriaList(rawCriteria, criteria, LABEL_CANCER_STAGES, "stages", "name");
                GetCriteriaList(rawCriteria, criteria, LABEL_CANCER_SIDE_EFFECTS_BIOMARKERS_ATTRIBUTES, "findings", "name");
                GetCriterion(rawCriteria, criteria, LABEL_AGE, "age");
                GetCriterion(rawCriteria, criteria, LABEL_KEYWORDS, "keywordPhrases");
                UseValueTextIfMatch(rawCriteria, criteria, LABEL_VA_ONLY, "vaOnly", true, TEXT_VA_ONLY_TRUE);
                GetLocationCriteria(rawCriteria, criteria);
                UseValueTextIfMatch(rawCriteria, criteria, LABEL_HEALTHY_VOLUNTEERS, "healthyVolunteers", true, TEXT_LIMIT_TO_HEALTHY_VOLUNTEERS);
                GetSelectedCriteria(rawCriteria, criteria, "trialTypes", LABEL_TRIAL_TYPE, "label", "checked");
                GetCriteriaList(rawCriteria, criteria, LABEL_DRUG_LIST, "drugs", "name");
                GetSelectedCriteria(rawCriteria, criteria, "trialPhases", LABEL_TREATMENT_PHASE, "label", "checked");
                GetCriterion(rawCriteria, criteria, LABEL_TRIAL_ID, "trialId");
                GetCriterion(rawCriteria, criteria, LABEL_TRIAL_INVESTIGATORS, "investigator", "term");
                GetCriterion(rawCriteria, criteria, LABEL_TRIAL_LEAD_ORG, "leadOrg", "term");
            }

            return criteria;
        }

        /// <summary>
        /// Encapsulates the logic for outputing location criteria.
        /// </summary>
        /// <param name="rawCriteria">A JSON object containing the search criteria.</param>
        /// <param name="criteria">An instance of <see cref="SearchCriteria"/> to store the criteria.</param>
        private static void GetLocationCriteria(JObject rawCriteria, SearchCriteria criteria)
        {
            LocationCriteria loc = LocationCriteriaFactory.Create(rawCriteria);

            switch (loc.LocationType)
            {
                case LocationType.Zip:
                    string zip = rawCriteria["zip"]?.Value<string>();
                    string zipRadius = rawCriteria["zipRadius"]?.Value<string>();
                    if(!string.IsNullOrEmpty(zip))
                    {
                        string value = $"within {zipRadius} miles of {zip}";
                        criteria.Add(LABEL_LOCATION_ZIP, HttpUtility.HtmlEncode(value));
                    }
                    break;
                case LocationType.CountryCityState:
                    if (loc.HasCountry) criteria.Add(LABEL_LOCATION_COUNTRY, HttpUtility.HtmlEncode(loc.Country));
                    if (loc.HasState)
                    {
                        // Sorted list of state names.
                        var names = loc.States.Select(code => StateNameHelper.GetStateName(code)).OrderBy(name => name);
                        string stateList = String.Join(", ", names);
                        criteria.Add(LABEL_LOCATION_STATE, HttpUtility.HtmlEncode(stateList));
                    }
                    if (loc.HasCity) criteria.Add(LABEL_LOCATION_CITY, HttpUtility.HtmlEncode(loc.City));
                    break;
                case LocationType.Hospital:
                    GetCriterion(rawCriteria, criteria, LABEL_LOCATION_HOSPITAL, "hospital", "term");
                    break;
                case LocationType.AtNIH:
                    criteria.Add(LABEL_LOCATION_AT_NIH, TEXT_LOCATION_AT_NIH);
                    break;

                // Otherwise, do nothing.
                case LocationType.All:
                default:
                    break;
            }
        }

        /// <summary>
        /// Get the value of a simple, scalar property.
        /// </summary>
        /// <param name="rawCriteria">A JSON object containing the search criteria.</param>
        /// <param name="criteria">An instance of <see cref="SearchCriteria"/> to store the criteria.</param>
        /// <param name="criterionLabel">The label to use with this criterion.</param>
        /// <param name="propertyName">Name of the property to retrieve</param>
        private static void GetCriterion(JToken rawCriteria, SearchCriteria criteria, string criterionLabel, string propertyName)
        {
            JToken jValue = rawCriteria[propertyName];
            if(jValue != null && !jValue.HasValues)
            {
                string value = jValue.Value<string>();
                if(!String.IsNullOrWhiteSpace(value))
                {
                    // These values are coming from the HTTP Request and must not be trusted.
                    value = HttpUtility.HtmlEncode(value);
                    criteria.Add(criterionLabel, value);
                }
            }
        }

        /// <summary>
        /// Get the value of a property nested in another property.
        /// </summary>
        /// <param name="rawCriteria">A JSON object containing the search criteria.</param>
        /// <param name="criteria">An instance of <see cref="SearchCriteria"/> to store the criteria.</param>
        /// <param name="propertyName">Name of the property to retrieve</param>
        /// <param name="criterionLabel">The label to use with this criterion.</param>
        /// <param name="childPropertyName">Name of the child property to retrieve.</param>
        /// <param name="labelProperty">The name of the property to retrieve.</param>
        private static void GetCriterion(JToken rawCriteria, SearchCriteria criteria, string criterionLabel, string propertyName, string childPropertyName)
        {
            JToken parent = rawCriteria[propertyName];
            if (parent != null && parent.Type == JTokenType.Object)
            {
                JToken property = parent[childPropertyName];
                if(property != null && !property.HasValues)
                {
                    string value = property.Value<string>();
                    if (!String.IsNullOrWhiteSpace(value))
                    {
                        // These values are coming from the HTTP Request and must not be trusted.
                        value = HttpUtility.HtmlEncode(value);
                        criteria.Add(criterionLabel, value);
                    }
                }
            }
        }

        /// <summary>
        /// Get the values from an array of criteria items.
        /// </summary>
        /// <param name="rawCriteria">A JSON object containing the search criteria.</param>
        /// <param name="criteria">An instance of <see cref="SearchCriteria"/> to store the criteria.</param>
        /// <param name="criterionLabel">The label to use with this criterion.</param>
        /// <param name="propertyName">Name of the property to retrieve</param>
        /// <param name="childPropertyName">Name of the child property to retrieve.</param>
        /// <param name="labelProperty">The name of the property to retrieve.</param>
        private static void GetCriteriaList(JToken rawCriteria, SearchCriteria criteria, string criterionLabel, string propertyName, string childPropertyName)
        {
            JToken parent = rawCriteria[propertyName];
            if (parent != null && parent.Type == JTokenType.Array)
            {
                // For each list element which is an object,
                // If the object has the named child property, then retrieve that value.
                IEnumerable<string> values =
                    ((JArray)parent)
                        .Where(el => el.Type == JTokenType.Object
                            && !String.IsNullOrWhiteSpace(el[childPropertyName]?.Value<string>()))
                        .Select(el => el[childPropertyName]?.Value<string>());

                if(values.Count() > 0)
                {
                    string value = String.Join(", ", values);
                    value = HttpUtility.HtmlEncode(value);
                    criteria.Add(criterionLabel, value);
                }
            }
        }

        /// <summary>
        /// Creates a search criteria entry using <see cref="textValue"/> if the value of <see cref="propertyName"/>
        /// matches <see cref="targetValue"/>. Outputs the content of <see cref="textValue"/> instead of the
        /// property's actual value.
        /// </summary>
        /// <param name="rawCriteria">A JSON object containing the search criteria.</param>
        /// <param name="criteria">An instance of <see cref="SearchCriteria"/> to store the criteria.</param>
        /// <param name="criterionLabel">The label to use with this criterion.</param>
        /// <param name="propertyName">Name of the property to match against.</param>
        /// <param name="targetValue">The value to match.</param>
        /// <param name="textValue">The string to output as the criterion's value.</param>
        private static void UseValueTextIfMatch(JToken rawCriteria, SearchCriteria criteria, string criterionLabel, string propertyName, bool targetValue, string textValue)
        {
            JToken jv = rawCriteria[propertyName];
            if(jv != null && !jv.HasValues && jv.Type == JTokenType.Boolean)
            {
                bool value = jv.Value<bool>();
                if(value == targetValue)
                {
                    criteria.Add(criterionLabel, HttpUtility.HtmlEncode(textValue));
                }
            }
        }

        /// <summary>
        /// For an array of criteria objects identified by <see cref="parentNode"/>, retrieves the
        /// value of each element's <see cref="labelProperty"/> property when the matching
        /// <see cref="selectionProperty"/> is set to boolean true, the string "true",
        /// or the string "yes".
        ///
        /// <example>
        /// 
        /// If <c>JObject rawCriteria</c> contains
        /// 
        /// <code>
        /// {
        ///     "trialTypes": [
        ///         {"label": "Basic Science",    "value": "basic_science",   "checked": true },
        ///         {"label": "Prevention",       "value": "prevention",      "checked": false },
        ///         { "label": "Supportive Care", "value": "supportive_care", "checked": false },
        ///         { "label": "Treatment",       "value": "treatment",       "checked": true }
        ///     ]
        /// }
        /// </code>
        ///
        /// and <c>criteria</c>  is an instance of <c>SearchCriteria</c>
        ///
        /// Calling
        /// 
        /// <c>GetLabelIfSelected(rawCriteria, criteria, "trialTypes", "Trial Types", "label", "checked")</c>
        ///
        /// would add a single entry  to <c>crieria</c> with the label "Tryia Types" and a value of "Basic Science, Treatment".
        ///
        /// The "Prevention" and "Supportive Care" elements are ignored because their respective "checked" properties are false.
        /// 
        /// </example>
        /// </summary>
        /// <param name="rawCriteria">A JSON object containing the search criteria.</param>
        /// <param name="criteria">An instance of <see cref="SearchCriteria"/> to store the criteria.</param>
        /// <param name="parentNode">The name of the property containing the array.</param>
        /// <param name="criterionLabel">The label to use with the criteria.</param>
        /// <param name="labelProperty">The name of the property to retrieve.</param>
        /// <param name="selectionProperty">The property to check in order to determine whether the array element is selected.</param>
        private static void GetSelectedCriteria(JToken rawCriteria, SearchCriteria criteria, string parentNode, string criterionLabel, string labelProperty, string selectionProperty)
        {
            JToken jv = rawCriteria[parentNode];
            if (jv != null && jv.Type == JTokenType.Array)
            {
                // For each list element which is an object,
                // If both the label property and selectionProperty exist AND
                //      the selection property is (boolean, and set to true)
                //      or the selection property is (string and set to "true" or "yes")
                // Then retrieve the label property.
                IEnumerable<string> values =
                    ((JArray)jv)
                        .Where(el => el.Type == JTokenType.Object
                            && el[labelProperty] != null && el[labelProperty].Type == JTokenType.String
                            && el[selectionProperty] != null
                            && (
                                (el[selectionProperty].Type == JTokenType.Boolean && el[selectionProperty].Value<bool>())
                                || (el[selectionProperty].Type == JTokenType.String
                                        && (
                                            StringComparer.CurrentCultureIgnoreCase.Equals(el[selectionProperty].Value<string>(), "true")
                                            || StringComparer.CurrentCultureIgnoreCase.Equals(el[selectionProperty].Value<string>(), "yes")
                                        )
                                    )
                            )
                        )
                        .Select(el => el[labelProperty].Value<string>());

                if (values.Count() > 0)
                {
                    string value = String.Join(", ", values);
                    value = HttpUtility.HtmlEncode(value);
                    criteria.Add(criterionLabel, value);
                }
            }
        }

    }
}
