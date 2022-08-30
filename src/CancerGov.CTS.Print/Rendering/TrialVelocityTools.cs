using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

using Newtonsoft.Json.Linq;

using CancerGov.CTS.Print.Models;

namespace CancerGov.CTS.Print.Rendering
{

    /// <summary> 
    /// This class houses tools that can be used in the velocity templates for Results and View
    /// </summary>
    public class TrialVelocityTools
    {
        const string PRIMARY_PURPOSE_OTHER_TEXT = "Not provided by clinicaltrials.gov";

        /// <summary>
        /// Sorts the sites based on country (United States first, Canada second, Other in alphabetic order),
        /// followed by state or province in alphabetic order.
        /// </summary>
        /// <param name="sites"></param>
        /// <returns></returns>
        public JArray GetSortedSites(JArray sites)
        {
            var usaSites = sites.Where(s => s.Value<string>("org_country") == "United States").OrderBy(s => s.Value<string>("org_state_or_province")).ThenBy(s => s.Value<string>("org_city")).ThenBy(s => s.Value<string>("org_name"));
            var canadaSites = sites.Where(s => s.Value<string>("org_country") == "Canada").OrderBy(s => s.Value<string>("org_state_or_province")).ThenBy(s => s.Value<string>("org_city")).ThenBy(s => s.Value<string>("org_name"));
            var otherSites = sites.Where(s => s.Value<string>("org_country") != "United States" && s.Value<string>("org_country") != "Canada").OrderBy(s => s.Value<string>("org_country")).ThenBy(s => s.Value<string>("org_city")).ThenBy(s => s.Value<string>("org_name"));

            return new JArray(usaSites.Union(canadaSites).Union(otherSites));
        }

        /// <summary>
        /// Groups the sites by USA/Canada/Other, Then by state/Province/Country, then by city
        /// </summary>
        /// <returns></returns> 
        public OrderedDictionary GetGroupedSites(JArray sites)
        {
            OrderedDictionary locations = new OrderedDictionary();

            var usaSites = sites.Where(s => s.Value<string>("org_country") == "United States");
            var canadaSites = sites.Where(s => s.Value<string>("org_country") == "Canada");
            var otherSites = sites.Where(s => s.Value<string>("org_country") != "United States" && s.Value<string>("org_country") != "Canada");

            if (usaSites.Count() > 0)
            {
                //Group by state, then city
                OrderedDictionary states = new OrderedDictionary();

                //Group and loop over states
                foreach (IGrouping<string, JToken> group in usaSites.GroupBy(s => s.Value<string>("org_state_or_province")))
                {
                    states.Add(group.Key, new OrderedDictionary());

                    //Now do the same for cities
                    foreach (IGrouping<string, JToken> cityGroup in group.GroupBy(s => s.Value<string>("org_city")))
                    {
                        ((OrderedDictionary)states[group.Key]).Add(cityGroup.Key, cityGroup.AsEnumerable());
                    }
                }

                locations.Add("U.S.A.", states);
            }

            if (canadaSites.Count() > 0)
            {
                //Group by state, then city
                OrderedDictionary states = new OrderedDictionary();

                //Group and loop over states
                foreach (IGrouping<string, JToken> group in canadaSites.GroupBy(s => s.Value<string>("org_state_or_province")))
                {
                    states.Add(group.Key, new OrderedDictionary());

                    //Now do the same for cities
                    foreach (IGrouping<string, JToken> cityGroup in group.GroupBy(s => s.Value<string>("org_city")))
                    {
                        ((OrderedDictionary)states[group.Key]).Add(cityGroup.Key, cityGroup.AsEnumerable());
                    }
                }

                locations.Add("Canada", states);
            }

            if (otherSites.Count() > 0)
            {
                //Group by state, then city
                OrderedDictionary countries = new OrderedDictionary();

                //Group and loop over states
                foreach (IGrouping<string, JToken> group in otherSites.GroupBy(s => s.Value<string>("org_country")))
                {
                    countries.Add(group.Key, new OrderedDictionary());

                    //Now do the same for cities
                    foreach (IGrouping<string, JToken> cityGroup in group.GroupBy(s => s.Value<string>("org_city")))
                    {
                        ((OrderedDictionary)countries[group.Key]).Add(cityGroup.Key, cityGroup.AsEnumerable());
                    }
                }

                locations.Add("Other", countries);
            }

            return locations;
        }


        /// <summary>
        /// Gets and formats the Trial Objectives and Outline
        /// </summary>
        /// <param name="trial"></param>
        /// <returns>String - detailed description</returns>
        public string GetPrettyDescription(JObject trial)
        {
            String rtn = "<p class='ctrp'>" + HttpUtility.HtmlEncode(trial.Value<string>("detail_description")) + "</p>";
            return rtn.Replace("\r\n", "</p><p class='ctrp'>");
        }

        /// <summary>
        /// Capitalizes first letter and removes underscores
        /// </summary>
        // Formerly GetFormattedString
        public string GetNormalizedString(String str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                str = char.ToString(str[0]).ToUpper() + str.Substring(1).ToLower();
                str = str.Replace("_", " ");
            }
            return str;
        }

        /// <summary>
        /// Gets the list of secondary IDs as comma separated string
        /// </summary>
        /// <param name="trial"></param>
        /// <returns>A string containing (if present) the ctep_id, dcp_id, ccr_id, and nci_id followed by any ID strings
        /// in the other_ids node.</returns>
        public string GetSecondaryIDsString(JObject trial)
        {
            // Gather all the secondary IDs. These simply return null if not present.
            string[] fixedIDs = { trial.Value<string>("ctep_id"), trial.Value<string>("dcp_id"), trial.Value<string>("ccr_id"), trial.Value<string>("nci_id") };

            IEnumerable<string> otherIDs;

            if (trial["other_ids"] != null)
                otherIDs = trial["other_ids"].Select(obj => obj.Value<string>("value"));
            else
                otherIDs = new string[0];

            // Combine the two lists, and remove, blanks, duplicates, and copies of the primary and NCT IDs.
            IEnumerable<string> returnIDs = fixedIDs.Union(otherIDs)
                .Where(value => !String.IsNullOrWhiteSpace(value) && value != trial["protocol_id"]?.Value<string>() && value != trial["nct_id"]?.Value<string>())
                .Distinct();

            if (returnIDs.Count() > 0)
                return String.Join(", ", returnIDs);
            else
                return String.Empty;
        }

        /// <summary>
        /// Gets the Phase number from a trial and formats it for presentation.
        /// </summary>
        /// <param name="trial">JOBject containing a clinical trial.</param>
        /// <returns>String contining the trial's phase.</returns>
        public string GetPhase(JObject trial)
        {
            string phase = trial.Value<string>("phase") ?? string.Empty;
            if (!String.IsNullOrWhiteSpace(phase))
            {
                phase = "Phase " + phase.Replace("_", "/");
            }
            return phase;
        }

        /// <summary>
        /// Gets the Primary Purpose and formats text
        /// </summary>
        /// <param name="trial"></param>
        /// <returns>String - purpose</returns>
        public string GetTrialType(JObject trial)
        {
            /// TODO: Verify if we need to add other_text and additioncal_qualifier_code to this text
            string purpose = trial.Value<string>("primary_purpose") ?? String.Empty;

            if (!String.IsNullOrEmpty(purpose))
            {
                purpose = char.ToString(purpose[0]).ToUpper() + purpose.Substring(1).ToLower();
                purpose = purpose.Replace("_", " ");
            }

            if (purpose.ToLower() == "other")
                purpose = PRIMARY_PURPOSE_OTHER_TEXT;

            return purpose;
        }

        /// <summary>
        /// Checks abbreviation and returns full state/provice/territory name if there is a match.
        /// TODO: Clean up or move into a flat file or config.
        /// (This is still used by velocity template.)
        /// </summary>
        /// <param name="code">The two letter code for the state.</param>
        /// <returns>The state's name, or the original code if no match is found.</returns>
        [Obsolete("Replace with StateNameHelper")]
        public string GetStateName(string code)
        {
            return StateNameHelper.GetStateName(code);
        }

    }
}
