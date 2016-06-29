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

    public struct GeoArea : IEquatable<GeoArea>
    {
        public static readonly GeoArea Empty = new GeoArea(GeoPoint.Empty, GeoVector.Empty);
        private GeoPoint northWest;
        private GeoVector size;

        public GeoArea(double north, double east, double south, double west) : this()
        {
            NorthWest = new GeoPoint(north, west);
            Size = new GeoVector(north - south, east - west);
        }

        public GeoArea(GeoPoint northWestIn, GeoVector dimensionsIn) : this()
        {
            nameof(northWestIn).ThrowIfNull(northWestIn);

            NorthWest = northWestIn;
            Size = dimensionsIn;
        }

        public GeoArea(params GeoPoint[] pointsInExtent) : this()
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
                    west = Math.Min(west, point.Longitude);
                }

                NorthWest = new GeoPoint(north, west);
                Size = new GeoVector(north - south, east - west);
            }
        }

        /// <summary>
        ///     Gets or sets the anchor position (top left, or north west).
        /// </summary>
        public GeoPoint NorthWest
        {
            get { return northWest; }
            set { northWest = value; }
        }

        /// <summary>
        ///     Gets or sets the dimensions of this geo area.
        /// </summary>
        public GeoVector Size
        {
            get { return size; }
            set { size = value; }
        }

        public bool IsEmpty => NorthWest.IsEmpty || Size.IsEmpty;

        public GeoPoint NorthEast => new GeoPoint(NorthWest.Latitude, NorthWest.Longitude + Size.DeltaLongitude);

        public GeoPoint SouthWest => new GeoPoint(NorthWest.Latitude - Size.DeltaLatitude, NorthWest.Longitude);

        public GeoPoint SouthEast
            => new GeoPoint(NorthWest.Latitude - Size.DeltaLatitude, NorthWest.Longitude + Size.DeltaLongitude);

        public GeoPoint Center => new GeoPoint(NorthWest.Latitude - Size.DeltaLatitude / 2,
            NorthWest.Longitude + Size.DeltaLongitude / 2);

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
            return obj is GeoArea && Equals((GeoArea) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + NorthWest.GetHashCode();
                hash = hash * 23 + Size.GetHashCode();

                return hash;
            }
        }

        public void Expand(GeoVector dimensions)
        {
            // Split the difference so that the center doesn't change.
            northWest.Latitude -= dimensions.DeltaLatitude / 2;
            northWest.Longitude -= dimensions.DeltaLongitude / 2;
            size.DeltaLatitude += dimensions.DeltaLatitude / 2;
            size.DeltaLongitude += dimensions.DeltaLongitude / 2;
        }

        public static GeoArea Intersection(GeoArea first, GeoArea second)
        {
            return new GeoArea(
                Math.Min(first.NorthEast.Latitude, second.NorthEast.Latitude),
                Math.Min(first.NorthEast.Longitude, second.NorthEast.Longitude),
                Math.Max(first.SouthWest.Latitude, second.SouthWest.Latitude),
                Math.Max(first.SouthWest.Longitude, second.SouthWest.Longitude));
        }

        public override string ToString()
        {
            return $"{{{NorthWest}, {SouthEast}}}";
        }
    }
}