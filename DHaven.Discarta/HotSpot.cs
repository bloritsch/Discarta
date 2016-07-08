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
    using System.ComponentModel;
    using System.Globalization;
    using Internals;

    /// <summary>
    /// HotSpot is used for icons plotted on the map.  The HotSpot represents
    /// the point within the icon that matches the GeoPoint where it is plotted.
    /// The default value is 50%, which works for the majority of icons.  However,
    /// Pins and Markers tend to have the bottom center be the hot spot.
    /// </summary>
    [TypeConverter(typeof(HotSpotConverter))]
    public struct HotSpot : IEquatable<HotSpot>
    {
        /// <summary>
        /// Create a HotSpot with the provided value and HotSpotUnit.  The value
        /// is either representing
        /// </summary>
        /// <param name="value">the value</param>
        /// <param name="type">the unit type (defaults to HotSpotUnit.Pixel)</param>
        public HotSpot(double value, HotSpotUnit type = HotSpotUnit.Pixel)
        {
            Value = value;
            HotSpotUnit = type;

            if (IsProportional && !Value.IsInRange(0, 1, 0.01))
            {
                throw new ArgumentException($"{Value} must be between 0 and 1", nameof(value));
            }

            if (IsAbsolute && Value < 0)
            {
                throw new ArgumentException($"{Value} must be greater than or equal to 0", nameof(value));
            }
        }

        /// <summary>
        /// Gets whether this HotSpot represents display units.
        /// </summary>
        public bool IsAbsolute => HotSpotUnit == HotSpotUnit.Pixel;

        /// <summary>
        /// Gets whether this HotSpot represents a percentage.
        /// </summary>
        public bool IsProportional => HotSpotUnit == HotSpotUnit.Percent;

        /// <summary>
        /// Gets or sets the HotSpotUnit.
        /// </summary>
        public HotSpotUnit HotSpotUnit { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Gets an instance of the HotSpot object that represents the center of a dimension (50%).
        /// </summary>
        public static HotSpot Center { get; } = new HotSpot(0.5, HotSpotUnit.Percent);

        /// <summary>
        /// Calculates the offset from the top left corner using the provided dimension.
        /// If the HotSpot is absolute, we simply return value, otherwise we calculate the
        /// offset as a percentage of the supplied dimension.
        /// </summary>
        /// <param name="dimension">the height or width of the element</param>
        /// <returns>the adjusted offset</returns>
        public double Apply(double dimension)
        {
            return IsAbsolute ? Value : dimension * Value;
        }

        #region Implementation of IEquatable<HotSpot>

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(HotSpot other)
        {
            var precision = IsAbsolute ? 1 : .01;

            return Value.IsSameAs(other.Value, precision) && HotSpotUnit == other.HotSpotUnit;
        }

        #endregion

        #region Overrides of ValueType

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + Value.GetHashCode();
                hash = hash * 23 + HotSpotUnit.GetHashCode();

                return hash;
            }
        }

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <returns>
        ///     true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise,
        ///     false.
        /// </returns>
        /// <param name="obj">The object to compare with the current instance. </param>
        public override bool Equals(object obj)
        {
            return obj is HotSpot && Equals((HotSpot) obj);
        }

        /// <summary>Returns the fully qualified type name of this instance.</summary>
        /// <returns>A <see cref="T:System.String" /> containing a fully qualified type name.</returns>
        public override string ToString()
        {
            return ToString(CultureInfo.CurrentCulture);
        }

        public string ToString(IFormatProvider provider)
        {
            var format = IsAbsolute ? "F0" : "P0";
            var formattedValue = Value.ToString(format, provider);

            return IsAbsolute ? formattedValue + " px" : formattedValue;
        }

        #endregion
    }
}