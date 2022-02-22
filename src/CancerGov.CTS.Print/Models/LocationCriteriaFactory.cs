using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;

namespace CancerGov.CTS.Print.Models
{
    /// <summary>
    /// Wrapper to centralize creation and initialization of a LocationCriteria object.
    /// </summary>
    public static class LocationCriteriaFactory
    {
        // Deliberately well out of range for latitude and longitude.
        const double COORDINATE_NOT_SET = 5000.0;
        const double EPSILON = 0.1;

        /// <summary>
        /// Factory method for creating LocationCriteria.
        /// </summary>
        /// <param name="searchCriteria">JSON document containing the search criteria.</param>
        /// <returns>A LocationCriteria object.</returns>
        public static LocationCriteria Create(JObject searchCriteria)
        {
            LocationCriteria metadata = new LocationCriteria();

            if (searchCriteria != null)
            {
                string location
                    = GetValue(searchCriteria, "location", JTokenType.String, "search-location-all")
                        .Trim().ToLowerInvariant();

                switch (location)
                {
                    case "search-location-zip":
                        metadata.LocationType = LocationType.Zip;
                        GetZipData(searchCriteria, ref metadata);
                        break;

                    case "search-location-country":
                        metadata.LocationType = LocationType.CountryCityState;
                        GetCountryData(searchCriteria, ref metadata);
                        break;

                    case "search-location-hospital":
                        metadata.LocationType = LocationType.Hospital;
                        break;

                    case "search-location-nih":
                        metadata.LocationType = LocationType.AtNIH;
                        break;

                    case "search-location-all":
                    default:
                        metadata.LocationType = LocationType.All;
                        break;
                }

                GetVAOnlyFlag(searchCriteria, ref metadata);

            }

            return metadata;
        }

        /// <summary>
        /// Encapsulates initialization logic for ZIP code-based searches.
        /// </summary>
        /// <param name="searchCriteria">JSON document containing the search criteria.</param>
        /// <param name="metadata">Reference to the location criteria object which is being built.</param>
        private static void GetZipData(JObject searchCriteria, ref LocationCriteria metadata)
        {
            int zipRadius = GetValue(searchCriteria, "zipRadius", JTokenType.Integer, 100);
            GeoLocation zipGeo = null;

            JObject jgeo = (JObject)searchCriteria["zipCoords"];
            if(jgeo != null)
            {
                bool OutOfRange(double coord) => COORDINATE_NOT_SET - Math.Abs(coord) < EPSILON;

                double lat = GetValue(jgeo, "lat", JTokenType.Float, COORDINATE_NOT_SET);
                double lon = GetValue(jgeo, "long", JTokenType.Float, COORDINATE_NOT_SET);

                if(!OutOfRange(lat) && !OutOfRange(lon))
                {
                    zipGeo = new GeoLocation(lat, lon);
                }

            }

            metadata.ZipRadius = zipRadius;
            metadata.GeoLocation = zipGeo;
        }

        /// <summary>
        /// Encapsulates initialization logic for Country based searches.
        /// </summary>
        /// <param name="searchCriteria">JSON document containing the search criteria.</param>
        /// <param name="metadata">Reference to the location criteria object which is being built.</param>
        private static void GetCountryData(JObject searchCriteria, ref LocationCriteria metadata)
        {
            string country = GetValue(searchCriteria, "country", JTokenType.String, String.Empty);
            string city = GetValue(searchCriteria, "city", JTokenType.String, String.Empty);

            IEnumerable<string> states = Enumerable.Empty<string>();
            JToken jstates = searchCriteria["states"];
            if(jstates != null && jstates.Type == JTokenType.Array )
            {
                states =
                    from JToken el in jstates
                    where el.Type == JTokenType.String
                    select el.Value<string>();
            }

            metadata.Country = country;
            metadata.City = city;
            metadata.States = states;
        }

        /// <summary>
        /// Encapsulates initialization logic for the Veterans Administration flag.
        /// </summary>
        /// <param name="searchCriteria">JSON document containing the search criteria.</param>
        /// <param name="metadata">Reference to the location criteria object which is being built.</param>
        private static void GetVAOnlyFlag(JObject searchCriteria, ref LocationCriteria metadata)
        {
            bool vaOnly = GetValue(searchCriteria, "vaOnly", JTokenType.Boolean, false);
            metadata.IsVAOnly = vaOnly;
        }

        /// <summary>
        /// Retrieves a named value from a JSON structure, or return a default if the value is either
        /// not defined or not formatted in the expected manner.
        /// </summary>
        /// <typeparam name="T">The expected C# system type of the value.</typeparam>
        /// <param name="jobject">The JSON structure to retrieve from.</param>
        /// <param name="property">The name of the property containing the value.</param>
        /// <param name="type">The JTokenType of the value.</param>
        /// <param name="defaultValue">What to return if the value can't be retrieved.</param>
        /// <returns>The value or defaultValue</returns>
        private static T GetValue<T>(JObject jobject, string property, JTokenType type, T defaultValue)
        {
            T retVal = defaultValue;

            JToken jv = jobject[property];

            if(jv != null &&
                (jv.Type == JTokenType.String || jv.Type == type))
            {
                // The value can always be retrieved as a string, regardless of JTokenType.
                string strValue = jobject.Value<string>(property);

                // Props to https://stackoverflow.com/q/2961656/282194
                // for the generic converter.
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null && converter.IsValid(strValue))
                {
                    retVal = (T)converter.ConvertFromString(strValue);
                }
                else
                    retVal = defaultValue;
            }

            return retVal;
        }
    }
}
