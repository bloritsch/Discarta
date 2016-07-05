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
    ///     Represents a rectangular geographic area on a map.
    /// </summary>
    public struct GeoArea : IEquatable<GeoArea>
    {
        /// <summary>
        ///     Represents an empty GeoArea.
        /// </summary>
        public static readonly GeoArea Empty = new GeoArea(GeoPoint.Empty, GeoVector.Empty);

        private GeoPoint northWest;
        private GeoVector size;

        /// <summary>
        ///     Create a GeoArea using the bounding lat/lon values
        /// </summary>
        /// <param name="north">the northern boundary (max Lat)</param>
        /// <param name="east">the eastern boundary (max Lon)</param>
        /// <param name="south">the southern boundary (min Lat)</param>
        /// <param name="west">the western boundary (min Lon)</param>
        public GeoArea(double north, double east, double south, double west)
            : this(new GeoPoint(north, west), new GeoPoint(south, east)) {}

        /// <summary>
        ///     Create a GeoArea using the northwest point and a GeoVector
        ///     for the size.
        /// </summary>
        /// <param name="northWestIn">the northwest point</param>
        /// <param name="dimensionsIn">the size of the geo area</param>
        public GeoArea(GeoPoint northWestIn, GeoVector dimensionsIn) : this()
        {
            nameof(northWestIn).ThrowIfNull(northWestIn);

            NorthWest = northWestIn;
            Size = dimensionsIn;
        }

        /// <summary>
        ///     Create a bounding GeoArea from an array of points.
        /// </summary>
        /// <param name="pointsInExtent">the points within the area</param>
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

        /// <summary>
        ///     Gets whether the GeoArea is empty.
        /// </summary>
        public bool IsEmpty => NorthWest.IsEmpty || Size.IsEmpty;

        /// <summary>
        ///     Gets the north east point.
        /// </summary>
        public GeoPoint NorthEast => new GeoPoint(NorthWest.Latitude, NorthWest.Longitude + Size.DeltaLongitude);

        /// <summary>
        ///     Gets the south west point.
        /// </summary>
        public GeoPoint SouthWest => new GeoPoint(NorthWest.Latitude - Size.DeltaLatitude, NorthWest.Longitude);

        /// <summary>
        ///     Gets the south east point.
        /// </summary>
        public GeoPoint SouthEast
            => new GeoPoint(NorthWest.Latitude - Size.DeltaLatitude, NorthWest.Longitude + Size.DeltaLongitude);

        /// <summary>
        ///     Gets the center point of the geo area.
        /// </summary>
        public GeoPoint Center => new GeoPoint(NorthWest.Latitude - Size.DeltaLatitude / 2,
            NorthWest.Longitude + Size.DeltaLongitude / 2);

        /// <summary>
        ///     Determines if two GeoAreas are equivalent.
        /// </summary>
        /// <param name="first">the primary GeoArea</param>
        /// <param name="second">the secondary GeoArea</param>
        /// <returns>true if they are equivalent</returns>
        public static bool operator ==(GeoArea first, GeoArea second)
        {
            return first.Equals(second);
        }

        /// <summary>
        ///     Determines if two GeoAreas are not equivalent.
        /// </summary>
        /// <param name="first">the primary GeoArea</param>
        /// <param name="second">the secondary GeoArea</param>
        /// <returns>false if they are equivalent</returns>
        public static bool operator !=(GeoArea first, GeoArea second)
        {
            return !first.Equals(second);
        }

        /// <summary>
        ///     Determines if another GeoArea is equivalent to this one.
        /// </summary>
        /// <param name="other">the other GeoArea</param>
        /// <returns>true if they are equivalent</returns>
        public bool Equals(GeoArea other)
        {
            return NorthWest == other.NorthWest && Size == other.Size;
        }

        /// <summary>
        ///     Determines if this GeoArea is equivalent to the provided object.
        /// </summary>
        /// <param name="obj">the object</param>
        /// <returns>true if the object is a GeoArea and is equivalent to this one</returns>
        public override bool Equals(object obj)
        {
            return obj is GeoArea && Equals((GeoArea) obj);
        }

        /// <summary>
        ///     Calculate a hashcode for this GeoArea.
        /// </summary>
        /// <returns>the hash code</returns>
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

        /// <summary>
        ///     Expand this GeoArea by the amount of the GeoVector.
        ///     NOTE: increases the size of the GeoArea equally around the center.
        /// </summary>
        /// <param name="dimensions">the GeoVector representing the delta</param>
        public void Expand(GeoVector dimensions)
        {
            // Split the difference so that the center doesn't change.
            northWest.Latitude -= dimensions.DeltaLatitude / 2;
            northWest.Longitude -= dimensions.DeltaLongitude / 2;
            size.DeltaLatitude += dimensions.DeltaLatitude / 2;
            size.DeltaLongitude += dimensions.DeltaLongitude / 2;
        }

        /// <summary>
        ///     Calculate the intersection of the first and second GeoArea.
        /// </summary>
        /// <param name="first">the primary GeoArea</param>
        /// <param name="second">the secondary GeoArea</param>
        /// <returns>the GeoArea that represents the intersection between them</returns>
        public static GeoArea Intersection(GeoArea first, GeoArea second)
        {
            return new GeoArea(
                Math.Min(first.NorthEast.Latitude, second.NorthEast.Latitude),
                Math.Min(first.NorthEast.Longitude, second.NorthEast.Longitude),
                Math.Max(first.SouthWest.Latitude, second.SouthWest.Latitude),
                Math.Max(first.SouthWest.Longitude, second.SouthWest.Longitude));
        }

        /// <summary>
        ///     Represents the GeoArea as a string.
        /// </summary>
        /// <returns>{Northeast, SouthEast}</returns>
        public override string ToString()
        {
            return $"{{{NorthWest}, {SouthEast}}}";
        }
    }
}