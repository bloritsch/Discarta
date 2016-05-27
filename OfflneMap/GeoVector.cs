using DHaven.OfflneMap.Internals;
using System;

namespace DHaven.OfflneMap
{
    public struct GeoVector : IEquatable<GeoVector>
    {
        public static readonly GeoVector Empty = new GeoVector
        {
            DeltaLatitude = double.NaN ,
            DeltaLongitude = double.NaN,
        };

        public GeoVector(double deltaLat, double deltaLon)
        {
            DeltaLatitude = deltaLat;
            DeltaLongitude = deltaLon;
        }

        public double DeltaLatitude;
        public double DeltaLongitude;

        public bool IsEmpty { get { return double.IsNaN(DeltaLatitude) || double.IsNaN(DeltaLongitude); } }

        /// <summary>
        /// Gets the magnitude of the vector (Pythagoras' theorum).
        /// </summary>
        public double Magnitude
        {
            get { return Math.Sqrt((DeltaLatitude * DeltaLatitude) + (DeltaLongitude * DeltaLongitude)); }
        }

        /// <summary>
        /// Gets the angle in degrees created by the vector.
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
            return obj is GeoVector && Equals((GeoVector)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
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
