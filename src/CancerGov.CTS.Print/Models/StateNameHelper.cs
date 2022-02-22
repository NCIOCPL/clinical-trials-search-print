namespace CancerGov.CTS.Print.Models
{
    /// <summary>
    /// Helper class for working with state names.
    /// </summary>
    public static class StateNameHelper
    {
        /// <summary>
        /// Checks abbreviation and returns full state/provice/territory name if there is a match.
        /// TODO: Clean up or move into a flat file or config
        /// </summary>
        /// <param name="code">The two letter code for the state.</param>
        /// <returns>The state's name, or the original code if no match is found.</returns>
        public static string GetStateName(string code)
        {
            switch (code?.ToUpperInvariant())
            {
                case "AB": return "Alberta";
                case "AK": return "Alaska";
                case "AL": return "Alabama";
                case "AR": return "Arkansas";
                case "AS": return "American Samoa";
                case "AZ": return "Arizona";
                case "BC": return "British Columbia";
                case "CA": return "California";
                case "CO": return "Colorado";
                case "CT": return "Connecticut";
                case "DC": return "District of Columbia";
                case "DE": return "Delaware";
                case "FL": return "Florida";
                case "GA": return "Georgia";
                case "GU": return "Guam";
                case "HI": return "Hawaii";
                case "IA": return "Iowa";
                case "ID": return "Idaho";
                case "IL": return "Illinois";
                case "IN": return "Indiana";
                case "KS": return "Kansas";
                case "KY": return "Kentucky";
                case "LA": return "Louisiana";
                case "MA": return "Massachusetts";
                case "MB": return "Manitoba";
                case "MD": return "Maryland";
                case "ME": return "Maine";
                case "MI": return "Michigan";
                case "MN": return "Minnesota";
                case "MO": return "Missouri";
                case "MP": return "Northern Mariana Islands";
                case "MS": return "Mississippi";
                case "MT": return "Montana";
                case "NB": return "New Brunswick";
                case "NC": return "North Carolina";
                case "ND": return "North Dakota";
                case "NE": return "Nebraska";
                case "NH": return "New Hampshire";
                case "NJ": return "New Jersey";
                case "NL": return "Newfoundland and Labrador";
                case "NM": return "New Mexico";
                case "NS": return "Nova Scotia";
                case "NV": return "Nevada";
                case "NY": return "New York";
                case "OH": return "Ohio";
                case "OK": return "Oklahoma";
                case "ON": return "Ontario";
                case "OR": return "Oregon";
                case "PA": return "Pennsylvania";
                case "PE": return "Prince Edward Island";
                case "PR": return "Puerto Rico";
                case "QC": return "Quebec";
                case "RI": return "Rhode Island";
                case "SC": return "South Carolina";
                case "SD": return "South Dakota";
                case "SK": return "Saskatchewan";
                case "TN": return "Tennessee";
                case "TX": return "Texas";
                case "UM": return "U.S. Minor Outlying Islands";
                case "UT": return "Utah";
                case "VA": return "Virginia";
                case "VI": return "U.S. Virgin Islands";
                case "VT": return "Vermont";
                case "WA": return "Washington";
                case "WI": return "Wisconsin";
                case "WV": return "West Virginia";
                case "WY": return "Wyoming";
            }
            return code;
        }

    }
}
