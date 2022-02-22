using System;
using System.Collections.Generic;
using System.Linq;

namespace CancerGov.CTS.Print.Models
{
    /// <summary>
    /// Container object for parsed location criteria.
    /// </summary>
    public class LocationCriteria
    {
        /// <summary>
        /// Constructor. Use <see cref="LocationCriteriaFactory.Create(Newtonsoft.Json.Linq.JObject)"/> instead.
        /// </summary>
        internal LocationCriteria() { }

        /// <summary>
        /// What sort of location criteria are being used?
        /// </summary>
        public LocationType LocationType { get; internal set; } = LocationType.All;

        /// <summary>
        /// Limit to trials at a Veterans Administration facility
        /// </summary>
        public bool IsVAOnly { get; internal set; } = false;

        /// <summary>
        /// Number of miles from the zip code specified by <see cref="GeoLocation"/>.
        /// </summary>
        public int ZipRadius { get; internal set; } = 100;

        /// <summary>
        /// Geographic center for a ZIP code search.
        /// </summary>
        public GeoLocation GeoLocation { get; internal set; } = null;

        /// <summary>
        /// Country to search in.
        /// </summary>
        public string Country { get; internal set; }

        /// <summary>
        /// List of states to search in.
        /// </summary>
        public IEnumerable<string> States { get; internal set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Specific city to search in.
        /// </summary>
        public string City { get; internal set; } = String.Empty;

        /// <summary>
        /// Helper method: Was a country specified?
        /// </summary>
        public bool HasCountry
            => !String.IsNullOrWhiteSpace(Country);

        /// <summary>
        /// Helper method: Was a city specified?
        /// </summary>
        public bool HasCity
            => !String.IsNullOrWhiteSpace(City);

        /// <summary>
        /// Helper method: Were any states specified?
        /// </summary>
        public bool HasState
            => States.Count() > 0 && States.Any(name => !String.IsNullOrWhiteSpace(name));

    }

    /// <summary>
    /// What sort of location restrictsions should be used?
    /// </summary>
    public enum LocationType
    {
        /// <summary>
        /// No limits
        /// </summary>
        All = 0,

        /// <summary>
        /// Search within a given ZIP code.
        /// </summary>
        Zip = 1,

        /// <summary>
        /// Search within a Country, State and/or City.
        /// </summary>
        CountryCityState = 2,

        /// <summary>
        /// Search for trials at a hospital
        /// </summary>
        Hospital = 3,

        /// <summary>
        /// Search for trials at the NIH Clinical Center.
        /// </summary>
        AtNIH = 4,
    }

}
