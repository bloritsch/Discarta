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

    public struct GeoVector : IEquatable<GeoVector>
    {
        public static readonly GeoVector Empty = new GeoVector
        {
            DeltaLatitude = double.NaN,
            DeltaLongitude = double.NaN,
        };

        public GeoVector(double deltaLat, double deltaLon)
        {
            DeltaLatitude = deltaLat;
            DeltaLongitude = deltaLon;
        }

        public double DeltaLatitude { get; set; }
        public double DeltaLongitude { get; set; }

        public bool IsEmpty
        {
            get { return double.IsNaN(DeltaLatitude) || double.IsNaN(DeltaLongitude); }
        }

        /// <summary>
        ///     Gets the magnitude of the vector (Pythagoras' theorum).
        /// </summary>
        public double Magnitude
        {
            get { return Math.Sqrt((DeltaLatitude * DeltaLatitude) + (DeltaLongitude * DeltaLongitude)); }
        }

        /// <summary>
        ///     Gets the angle in degrees created by the vector.
        /// </summary>
        public double Angle
        {
            get { return ArgumentUtils.ToDegrees(Math.Atan(DeltaLatitude / DeltaLongitude)); }
        }

        public static bool operator ==(GeoVector first, GeoVector second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(GeoVector first, GeoVector second)
        {
            return !first.Equals(second);
        }

        public bool Equals(GeoVector other)
        {
            return ArgumentUtils.IsSameAs(DeltaLatitude, other.DeltaLatitude, ArgumentUtils.DegreePrecision)
                   && ArgumentUtils.IsSameAs(DeltaLongitude, other.DeltaLongitude, ArgumentUtils.DegreePrecision);
        }

        public override bool Equals(object obj)
        {
            return obj is GeoVector && Equals((GeoVector) obj);
        }

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

        public override string ToString()
        {
            return string.Format("[ΔLat: {0}, ΔLon: {1}]", DeltaLatitude, DeltaLongitude);
        }
    }
}