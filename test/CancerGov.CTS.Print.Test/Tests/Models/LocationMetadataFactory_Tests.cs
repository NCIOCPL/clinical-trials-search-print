using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;
using Xunit;

using CancerGov.CTS.Print.Models;

namespace CancerGov.CTS.Print.Test.Models
{
    public class LocationMetadataFactory_Tests
    {
        /// <summary>
        /// Verify no criteria entries are generated when there are no criteria are specified.
        /// </summary>
        [Fact]
        public void NoSearchCriteria()
        {
            LocationCriteria metadata = LocationCriteriaFactory.Create(null);

            Assert.NotNull(metadata);
            Assert.Equal(LocationType.All, metadata.LocationType);
        }

        /// <summary>
        /// Verify garbage inputs don't result in criteria entries.
        /// </summary>
        [Fact]
        public void InvalidInput()
        {
            JObject badObject = JObject.Parse(@"
                {
                    ""property-name"": ""some value""
                }
            ");

            LocationCriteria metadata = LocationCriteriaFactory.Create(badObject);

            Assert.NotNull(metadata);
            Assert.Equal(LocationType.All, metadata.LocationType);
        }

        /// <summary>
        /// Verify search-location-all causes other location criteria to be ignored.
        /// </summary>
        [Fact]
        public void Location_All()
        {
            JObject criteria = JObject.Parse(@"
                {
	                ""zipCoords"": { ""lat"": """", ""long"": """" },
	                ""zipRadius"": ""100"",
	                ""country"": ""United States"",
	                ""states"": [],
	                ""city"": """",
                    ""location"": ""search-location-all"",
                    ""vaOnly"": false
                }
            ");

            LocationCriteria metadata = LocationCriteriaFactory.Create(criteria);

            Assert.NotNull(metadata);
            Assert.Equal(LocationType.All, metadata.LocationType);
            Assert.False(metadata.HasCountry);
            Assert.False(metadata.HasState);
            Assert.False(metadata.HasCity);
        }

        /// <summary>
        /// Verify invalid location type causes location criteria to be ignored.
        /// </summary>
        [Fact]
        public void Location_Invalid()
        {
            JObject criteria = JObject.Parse(@"
                {
	                ""zipCoords"": { ""lat"": """", ""long"": """" },
	                ""zipRadius"": ""100"",
	                ""country"": ""United States"",
	                ""states"": [],
	                ""city"": """",
                    ""location"": ""chicken"",
                    ""vaOnly"": false
                }
            ");

            LocationCriteria metadata = LocationCriteriaFactory.Create(criteria);

            Assert.NotNull(metadata);
            Assert.Equal(LocationType.All, metadata.LocationType);
        }

        /// <summary>
        /// Verify missing location type causes location criteria to be ignored.
        /// </summary>
        [Fact]
        public void Location_Missing()
        {
            JObject criteria = JObject.Parse(@"
                {
	                ""zipCoords"": { ""lat"": """", ""long"": """" },
	                ""zipRadius"": ""100"",
	                ""country"": ""United States"",
	                ""states"": [],
	                ""city"": """",
                    ""vaOnly"": false
                }
            ");

            LocationCriteria metadata = LocationCriteriaFactory.Create(criteria);

            Assert.NotNull(metadata);
            Assert.Equal(LocationType.All, metadata.LocationType);
        }

        /// <summary>
        /// Verify handling of search-location-zip.
        /// </summary>
        [Fact]
        public void Location_Zip()
        {
            JObject criteria = JObject.Parse(@"
                {
	                ""zipCoords"": { ""lat"": ""39.0003"", ""long"": ""-77.1056"" },
	                ""zipRadius"": ""250"",
	                ""country"": ""United States"",
	                ""states"": [],
	                ""city"": """",
                    ""location"": ""search-location-zip"",
                    ""vaOnly"": true
                }
            ");

            LocationCriteria metadata = LocationCriteriaFactory.Create(criteria);

            Assert.NotNull(metadata);
            // Wny no check for the ZIP? Because we don't actually need it.
            Assert.Equal(LocationType.Zip, metadata.LocationType);
            Assert.Equal(250, metadata.ZipRadius);
            Assert.NotNull(metadata.GeoLocation);
            Assert.Equal(39.0003, metadata.GeoLocation.Lat, 4);
            Assert.Equal(-77.1056, metadata.GeoLocation.Lon, 4);
        }

        /// <summary>
        /// Verify handling of a missing geolocation.
        /// </summary>
        [Fact]
        public void Location_Zip_NoGeo()
        {
            JObject criteria = JObject.Parse(@"
                {
	                ""zipRadius"": ""250"",
	                ""country"": ""United States"",
	                ""states"": [],
	                ""city"": """",
                    ""location"": ""search-location-zip"",
                    ""vaOnly"": true
                }
            ");

            LocationCriteria metadata = LocationCriteriaFactory.Create(criteria);

            Assert.NotNull(metadata);
            // Wny no check for the ZIP? Because we don't actually need it.
            Assert.Equal(LocationType.Zip, metadata.LocationType);
            Assert.Equal(250, metadata.ZipRadius);
            Assert.Null(metadata.GeoLocation);
        }

        /// <summary>
        /// Verify handling of a non-numeric zipRadius
        /// </summary>
        [Fact]
        public void Location_BadZip()
        {
            JObject criteria = JObject.Parse(@"
                {
	                ""zipCoords"": { ""lat"": ""39.0003"", ""long"": ""-77.1056"" },
	                ""zipRadius"": ""chicken"",
	                ""country"": ""United States"",
	                ""states"": [],
	                ""city"": """",
                    ""location"": ""search-location-zip"",
                    ""vaOnly"": true
                }
            ");

            LocationCriteria metadata = LocationCriteriaFactory.Create(criteria);

            Assert.NotNull(metadata);
            Assert.Equal(LocationType.Zip, metadata.LocationType);
            Assert.Equal(100, metadata.ZipRadius);
            Assert.NotNull(metadata.GeoLocation);
            Assert.Equal(39.0003, metadata.GeoLocation.Lat, 4);
            Assert.Equal(-77.1056, metadata.GeoLocation.Lon, 4);
        }

        /// <summary>
        /// Verify handling of search-location-country.
        /// </summary>
        [Fact]
        public void Location_Country()
        {
            JObject criteria = JObject.Parse(@"
                {
	                ""zipCoords"": { ""lat"": """", ""long"": """" },
	                ""zipRadius"": ""100"",
	                ""country"": ""United States"",
	                ""states"": [""DC"",""IL"",""NC""],
	                ""city"": ""Washington"",
                    ""location"": ""search-location-country"",
                    ""vaOnly"": false
                }
            ");

            LocationCriteria metadata = LocationCriteriaFactory.Create(criteria);

            Assert.NotNull(metadata);
            Assert.Equal(LocationType.CountryCityState, metadata.LocationType);
            Assert.Equal("United States", metadata.Country);
            Assert.Equal(new string[] { "DC", "IL", "NC" }, metadata.States);
            Assert.Equal("Washington", metadata.City);
            Assert.True(metadata.HasCountry);
            Assert.True(metadata.HasState);
            Assert.True(metadata.HasCity);

            Assert.False(metadata.IsVAOnly);
        }

        /// <summary>
        /// Verify handling when search-location-country has no state or city.
        /// </summary>
        [Fact]
        public void Location_Country_Only()
        {
            JObject criteria = JObject.Parse(@"
                {
	                ""zipCoords"": { ""lat"": """", ""long"": """" },
	                ""zipRadius"": ""100"",
	                ""country"": ""United States"",
                    ""location"": ""search-location-country"",
                    ""vaOnly"": false
                }
            ");

            LocationCriteria metadata = LocationCriteriaFactory.Create(criteria);

            Assert.NotNull(metadata);
            Assert.Equal(LocationType.CountryCityState, metadata.LocationType);
            Assert.Equal("United States", metadata.Country);
            Assert.Equal(new string[0], metadata.States);
            Assert.Equal(String.Empty, metadata.City);
            Assert.True(metadata.HasCountry);
            Assert.False(metadata.HasState);
            Assert.False(metadata.HasCity);

            Assert.False(metadata.IsVAOnly);
        }

        /// <summary>
        /// Verify search-location-country for a non-string country.
        /// </summary>
        [Fact]
        public void Location_BadCountry()
        {
            JObject criteria = JObject.Parse(@"
                {
	                ""zipCoords"": { ""lat"": """", ""long"": """" },
	                ""zipRadius"": ""100"",
	                ""country"": 55,
	                ""states"": ""Not an array!"",
	                ""city"": 22,
                    ""location"": ""search-location-country"",
                    ""vaOnly"": false
                }
            ");

            LocationCriteria metadata = LocationCriteriaFactory.Create(criteria);

            Assert.NotNull(metadata);
            Assert.Equal(LocationType.CountryCityState, metadata.LocationType);
            Assert.Equal(String.Empty, metadata.Country);
            Assert.Empty(metadata.States);
            Assert.Equal(String.Empty, metadata.City);
            Assert.False(metadata.HasCountry);
            Assert.False(metadata.HasState);
            Assert.False(metadata.HasCity);

            Assert.False(metadata.IsVAOnly);
        }

        /// <summary>
        /// Verify handling for search-location-hospital.
        /// </summary>
        [Fact]
        public void Location_Hospital()
        {
            JObject criteria = JObject.Parse(@"
                {
	                ""zipCoords"": { ""lat"": """", ""long"": """" },
	                ""zipRadius"": ""100"",
	                ""country"": ""United States"",
	                ""states"": [],
	                ""city"": """",
                    ""location"": ""search-location-hospital"",
                    ""vaOnly"": false
                }
            ");

            LocationCriteria metadata = LocationCriteriaFactory.Create(criteria);

            Assert.NotNull(metadata);
            Assert.Equal(LocationType.Hospital, metadata.LocationType);
        }

        /// <summary>
        /// Verify handling search-location-nih.
        /// </summary>
        [Fact]
        public void Location_AtNIH()
        {
            JObject criteria = JObject.Parse(@"
                {
	                ""zipCoords"": { ""lat"": """", ""long"": """" },
	                ""zipRadius"": ""100"",
	                ""country"": ""United States"",
	                ""states"": [],
	                ""city"": """",
                    ""location"": ""search-location-nih"",
                    ""vaOnly"": false
                }
            ");

            LocationCriteria metadata = LocationCriteriaFactory.Create(criteria);

            Assert.NotNull(metadata);
            Assert.Equal(LocationType.AtNIH, metadata.LocationType);
        }

        /// <summary>
        /// Verify detection of Veterans Administration only flag.
        /// </summary>
        [Fact]
        public void Location_VAOnly()
        {
            JObject criteria = JObject.Parse(@"
                {
	                ""zipCoords"": { ""lat"": """", ""long"": """" },
	                ""zipRadius"": ""100"",
	                ""country"": ""United States"",
	                ""states"": [],
	                ""city"": """",
                    ""location"": ""search-location-all"",
                    ""vaOnly"": true
                }
            ");

            LocationCriteria metadata = LocationCriteriaFactory.Create(criteria);

            Assert.NotNull(metadata);
            Assert.Equal(LocationType.All, metadata.LocationType);
        }

        /// <summary>
        /// Verify handling of non-boolean vaOnly value.
        /// </summary>
        [Fact]
        public void Location_BadVAOnly()
        {
            JObject criteria = JObject.Parse(@"
                {
	                ""zipCoords"": { ""lat"": """", ""long"": """" },
	                ""zipRadius"": ""100"",
	                ""country"": ""United States"",
	                ""states"": [],
	                ""city"": """",
                    ""location"": ""search-location-all"",
                    ""vaOnly"": ""chicken""
                }
            ");

            LocationCriteria metadata = LocationCriteriaFactory.Create(criteria);

            Assert.NotNull(metadata);
            Assert.Equal(LocationType.All, metadata.LocationType);
        }

    }
}
