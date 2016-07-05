#region Copyright 2016 D-Haven.org

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

namespace DHaven.DisCarta
{
    using System;
    using Internals;

    /// <summary>
    /// Represents the change in lat/lon between two points.
    /// </summary>
    public struct GeoVector : IEquatable<GeoVector>
    {
        /// <summary>
        /// An empty GeoVector, which is different than one with 0 value.
        /// </summary>
        public static readonly GeoVector Empty = new GeoVector
        {
            DeltaLatitude = double.NaN,
            DeltaLongitude = double.NaN,
        };

        /// <summary>
        /// Create a GeoVector using the provided values.
        /// </summary>
        /// <param name="deltaLat">the change in latitude</param>
        /// <param name="deltaLon">the change in longitude</param>
        public GeoVector(double deltaLat, double deltaLon)
        {
            DeltaLatitude = deltaLat;
            DeltaLongitude = deltaLon;
        }

        /// <summary>
        /// Gets or sets the change in Latitude.
        /// </summary>
        public double DeltaLatitude { get; set; }

        /// <summary>
        /// Gets or sets the change in Longitude.
        /// </summary>
        public double DeltaLongitude { get; set; }

        /// <summary>
        /// Determines if this GeoVector is Empty (i.e. the same as GeoVector.Empty)
        /// </summary>
        public bool IsEmpty => double.IsNaN(DeltaLatitude) || double.IsNaN(DeltaLongitude);

        /// <summary>
        ///     Gets the magnitude of the vector (Pythagoras' theorum).
        /// </summary>
        public double Magnitude => Math.Sqrt(DeltaLatitude * DeltaLatitude + DeltaLongitude * DeltaLongitude);

        /// <summary>
        ///     Gets the angle in degrees created by the vector.
        /// </summary>
        public double Angle => ArgumentUtils.ToDegrees(Math.Atan(DeltaLatitude / DeltaLongitude));

        /// <summary>
        /// Determine if two GeoVectors are equivalent.  We compare down to 5 decimal places (~approximately 1m precision).
        /// </summary>
        /// <param name="first">the GeoVector being compared against</param>
        /// <param name="second">the GeoVector being compared with</param>
        /// <returns>true if the two vectors are equivalent</returns>
        public static bool operator ==(GeoVector first, GeoVector second)
        {
            return first.Equals(second);
        }

        /// <summary>
        /// Determine if two GeoVectors are not equivalent.
        /// </summary>
        /// <param name="first">the GeoVector being compared against</param>
        /// <param name="second">the GeoVector being compared with</param>
        /// <returns>false if the two vectors are equivalent</returns>
        public static bool operator !=(GeoVector first, GeoVector second)
        {
            return !first.Equals(second);
        }

        /// <summary>
        /// Determine if a GeoVectors is equivalent with this one.  We compare down to 5 decimal places (~approximately 1m precision).
        /// </summary>
        /// <param name="other">the GeoVector being compared with</param>
        /// <returns>true if the two vectors are equivalent</returns>
        public bool Equals(GeoVector other)
        {
            return DeltaLatitude.IsSameAs(other.DeltaLatitude, ArgumentUtils.DegreePrecision)
                   && DeltaLongitude.IsSameAs(other.DeltaLongitude, ArgumentUtils.DegreePrecision);
        }

        /// <summary>
        /// Determine if the object is equivalent with this GeoVector.  We compare down to 5 decimal places (~approximately 1m precision).
        /// </summary>
        /// <param name="obj">the object being compared</param>
        /// <returns>true if the two vectors are equivalent</returns>
        public override bool Equals(object obj)
        {
            return obj is GeoVector && Equals((GeoVector) obj);
        }

        /// <summary>
        /// Calculate the hashcode from the current values in this GeoVector.
        /// </summary>
        /// <returns>a hashcode</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + DeltaLatitude.GetHashCode();
                hash = hash * 23 + DeltaLongitude.GetHashCode();

                return hash;
            }
        }

        /// <summary>
        /// Represents the GeoVector as a string.
        /// </summary>
        /// <returns>the string representation</returns>
        public override string ToString()
        {
            return $"[ΔLat: {DeltaLatitude}, ΔLon: {DeltaLongitude}]";
        }
    }
}