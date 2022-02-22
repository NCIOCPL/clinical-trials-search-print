using System;

namespace CancerGov.CTS.Print.Models
{
    /// <summary>
    /// Class represents an ElasticSearch geo_code, or more simply
    /// a latitude and longitude of a location.
    /// </summary>
    public class GeoLocation
    {
        /// <summary>
        /// Create a new instance of a GeoLocation
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        public GeoLocation(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }

        /// <summary>
        /// Gets the latitude of this GeoLocation
        /// </summary>
        public double Lat { get; set; }

        /// <summary>
        /// Gets the longitude of this GeoLocation
        /// </summary>
        public double Lon { get; set; }

        /// <summary>
        /// Gets the distance between this location and another in miles.  Following the Haversine formula
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public double DistanceBetween(GeoLocation other)
        {
            double resultDistance = 0.0;
            double avgRadiusOfEarth = 3960; //Radius of the earth differ, I'm taking the average.

            //Haversine formula
            //distance = R * 2 * aTan2 ( square root of A, square root of 1 - A )
            //                   where A = sinus squared (difference in latitude / 2) + (cosine of latitude 1 * cosine of latitude 2 * sinus squared (difference in longitude / 2))
            //                   and R = the circumference of the earth

            double differenceInLat = DegreeToRadian(this.Lat - other.Lat);
            double differenceInLong = DegreeToRadian(this.Lon - other.Lon);
            double aInnerFormula = Math.Cos(DegreeToRadian(this.Lat)) * Math.Cos(DegreeToRadian(other.Lat)) * Math.Sin(differenceInLong / 2) * Math.Sin(differenceInLong / 2);
            double aFormula = (Math.Sin((differenceInLat) / 2) * Math.Sin((differenceInLat) / 2)) + (aInnerFormula);
            resultDistance = avgRadiusOfEarth * 2 * Math.Atan2(Math.Sqrt(aFormula), Math.Sqrt(1 - aFormula));

            return resultDistance;
        }

        /// <summary>
        /// Converts a Degree to Radians
        /// </summary>
        /// <param name="val">The value in degrees</param>
        /// <returns>The value in radians</returns>
        public double DegreeToRadian(double val)
        {
            return (Math.PI / 180) * val;
        }


        public override string ToString()
        {
            return String.Format("Lat: {0}, Lon: {1}", Lat, Lon);
        }
    }
}
