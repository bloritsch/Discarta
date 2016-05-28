using DHaven.DisCarta.Internals;
using System;

namespace DHaven.DisCarta
{
    public struct GeoArea : IEquatable<GeoArea>
    {
        public static readonly GeoArea Empty = new GeoArea(GeoPoint.Empty, GeoVector.Empty);

        public GeoArea(double north, double east, double south, double west)
        {
            NorthWest = new GeoPoint(north, west);
            Size = new GeoVector(north - south, east - west);
        }

        public GeoArea(GeoPoint northWestIn, GeoVector dimensionsIn)
        {
            nameof(northWestIn).ThrowIfNull(northWestIn);

            NorthWest = northWestIn;
            Size = dimensionsIn;
        }

        public GeoArea(params GeoPoint[] pointsInExtent)
        {
            if (pointsInExtent.Length == 0)
            {
                NorthWest = new GeoPoint();
                Size = new GeoVector();
            }
            else
            {
                double north = -90;
                double south = 90;
                double east = -180;
                double west = 180;

                foreach (var point in pointsInExtent)
                {
                    north = Math.Max(north, point.Latitude);
                    south = Math.Min(south, point.Latitude);
                    east = Math.Max(east, point.Longitude);
                    west = Math.Min(east, point.Longitude);
                }

                NorthWest = new GeoPoint(north, west);
                Size = new GeoVector(north - south, east - west);
            }
        }

        public GeoPoint NorthWest;
        public GeoVector Size;

        public bool IsEmpty { get { return NorthWest.IsEmpty || Size.IsEmpty; } }

        public GeoPoint NorthEast
        {
            get { return new GeoPoint(NorthWest.Latitude, NorthWest.Longitude + Size.DeltaLongitude); }
        }

        public GeoPoint SouthWest
        {
            get { return new GeoPoint(NorthWest.Latitude + Size.DeltaLatitude, NorthWest.Longitude); }
        }

        public GeoPoint SouthEast
        {
            get { return new GeoPoint(NorthWest.Latitude + Size.DeltaLatitude, NorthWest.Longitude + Size.DeltaLongitude); }
        }

        public GeoPoint Center
        {
            get { return new GeoPoint(NorthWest.Latitude + (Size.DeltaLatitude / 2), NorthWest.Longitude + (Size.DeltaLongitude / 2)); }
        }

        public static bool operator ==(GeoArea first, GeoArea second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(GeoArea first, GeoArea second)
        {
            return !first.Equals(second);
        }

        public bool Equals(GeoArea other)
        {
            return NorthWest == other.NorthWest && Size == other.Size;
        }

        public override bool Equals(object obj)
        {
            return obj is GeoArea && Equals((GeoArea)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + NorthWest.GetHashCode();
                hash = hash * 23 + Size.GetHashCode();

                return hash;
            }
        }

        public void Expand(GeoVector dimensions)
        {
            // Split the difference so that the center doesn't change.
            NorthWest.Latitude -= dimensions.DeltaLatitude / 2;
            NorthWest.Longitude -= dimensions.DeltaLongitude / 2;
            Size.DeltaLatitude += dimensions.DeltaLatitude / 2;
            Size.DeltaLongitude += dimensions.DeltaLongitude / 2;
        }

        public override string ToString()
        {
            return string.Format("{{{0}, {1}}}", NorthWest, SouthEast);
        }
    }
}
