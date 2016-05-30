#region Copyright 2016 D-Haven.org
/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion
using DHaven.DisCarta.Internals;
using System;

namespace DHaven.DisCarta
{
    /// <summary>
    /// Represents a point on the map in degrees lat/lon and meters above see level.
    /// Uses WGS84 spheroid for modeling.
    /// </summary>
    public struct GeoPoint : IEquatable<GeoPoint>
    {
        public static readonly GeoPoint Empty = new GeoPoint(double.NaN, double.NaN);

        /// <summary>
        /// Create a GeoPoint with the specified latitude, longitude, and optional altitude.
        /// </summary>
        /// <param name="latitudeIn">latitude part of the coordinate (-90 to 90)</param>
        /// <param name="longitudeIn">longitude part of the coordinate (-180 to 180)</param>
        /// <param name="altitudeIn">altitude part of the coordinate (0 is the surface of the WGS84 spheroid</param>
        public GeoPoint(double latitudeIn, double longitudeIn)
        {
            Latitude = latitudeIn;
            Longitude = longitudeIn;
        }

        /// <summary>
        /// Gets or sets the latitude in degrees for the WGS84 spheriod.
        /// </summary>
        public double Latitude;

        /// <summary>
        /// Gets or sets the longitude in degrees for the WGS84 spheriod.
        /// </summary>
        public double Longitude;

        /// <summary>
        /// Gets whether the point is Empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return double.IsNaN(Latitude) || double.IsNaN(Longitude); }
        }

        /// <summary>
        /// Gets whether the point is a valid geographic point.
        /// </summary>
        public bool IsValid
        {
            get { return Latitude.IsInRange(-90, 90, ArgumentUtils.DegreePrecision) && Longitude.IsInRange(-180, 180, ArgumentUtils.DegreePrecision); }
        }

        public static bool operator ==(GeoPoint first, GeoPoint second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(GeoPoint first, GeoPoint second)
        {
            return !first.Equals(second);
        }

        public bool Equals(GeoPoint other)
        {
            return ArgumentUtils.IsSameAs(Latitude, other.Latitude, ArgumentUtils.DegreePrecision)
                && ArgumentUtils.IsSameAs(Longitude, other.Longitude, ArgumentUtils.DegreePrecision);
        }

        public override bool Equals(object obj)
        {
            return obj is GeoPoint && Equals((GeoPoint)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Latitude.GetHashCode();
                hash = hash * 23 + Longitude.GetHashCode();

                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format("[Lat: {0}, Lon: {1}]", Latitude, Longitude);
        }

        public static GeoVector operator -(GeoPoint first, GeoPoint second)
        {
            return new GeoVector(first.Latitude - second.Latitude, first.Longitude - second.Longitude);
        }

        public static GeoPoint operator -(GeoPoint point, GeoVector vector)
        {
            return new GeoPoint(point.Latitude - vector.DeltaLatitude, point.Longitude - vector.DeltaLongitude);
        }

        public static GeoPoint operator +(GeoPoint point, GeoVector vector)
        {
            return new GeoPoint(point.Latitude + vector.DeltaLatitude, point.Longitude + vector.DeltaLongitude);
        }

        /// <summary>
        /// Calculate the distance between two geographic points, taking into consideration the earth's curvature.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static double Distance(GeoPoint first, GeoPoint second)
        {
            // Use the haversine formula to calculate distance.
            //var R = 6371e3; // metres
            //var φ1 = lat1.toRadians();
            //var φ2 = lat2.toRadians();
            //var Δφ = (lat2 - lat1).toRadians();
            //var Δλ = (lon2 - lon1).toRadians();

            //var a = Math.sin(Δφ / 2) * Math.sin(Δφ / 2) +
            //        Math.cos(φ1) * Math.cos(φ2) *
            //        Math.sin(Δλ / 2) * Math.sin(Δλ / 2);
            //var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));

            //var d = R * c;

            double lat1 = ArgumentUtils.ToRadians(first.Latitude);
            double lat2 = ArgumentUtils.ToRadians(second.Latitude);
            double deltaLat = ArgumentUtils.ToRadians(second.Latitude - first.Latitude);
            double deltaLon = ArgumentUtils.ToRadians(second.Longitude - first.Longitude);

            double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2)
                + Math.Cos(lat1) * Math.Cos(lat2)
                * Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return ArgumentUtils.MeanEarthRadius * c;
        }

        public static double Bearing(GeoPoint first, GeoPoint second)
        {
            //var y = Math.sin(λ2 - λ1) * Math.cos(φ2);
            //var x = Math.cos(φ1) * Math.sin(φ2) -
            //        Math.sin(φ1) * Math.cos(φ2) * Math.cos(λ2 - λ1);
            //var brng = Math.atan2(y, x).toDegrees();

            double deltaLat = ArgumentUtils.ToRadians(second.Latitude - first.Latitude);
            double deltaLon = ArgumentUtils.ToRadians(second.Longitude - first.Longitude);
            double lat1 = ArgumentUtils.ToRadians(first.Latitude);
            double lat2 = ArgumentUtils.ToRadians(second.Latitude);

            double y = Math.Sin(deltaLon) * Math.Cos(lat2);
            double x = Math.Cos(lat1) * Math.Sin(lat2)
                - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(deltaLat);

            return ArgumentUtils.ToDegrees(Math.Atan2(y, x));
        }
    }
}
