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
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Security;

    /// <summary>
    /// Converts strings and values to and from a HotSpot struct.
    /// </summary>
    public class HotSpotConverter : TypeConverter
    {
        private static readonly Tuple<string, HotSpotUnit>[] UnitTuples =
        {
            Tuple.Create("px", HotSpotUnit.Pixel),
            Tuple.Create("%", HotSpotUnit.Percent)
        };

        private static readonly Tuple<string, double>[] PixelUniTuples =
        {
            Tuple.Create("in", 96.0),
            Tuple.Create("cm", 96.0 / 2.54),
            Tuple.Create("pt", 96.0 / 72.0)
        };

        /// <summary>
        ///     Parses a HotSpot from a string given the CultureInfo.
        /// </summary>
        /// <param name="stringValue">String to parse from. </param>
        /// <param name="cultureInfo">Culture Info.</param>
        /// <returns>Newly created HotSpot instance.</returns>
        /// <remarks>
        ///     Formats:
        ///     "[value][unit]"
        ///     [value] is a double
        ///     [unit] is a string in HotSpot connected to a HotSpotUnit
        ///     "[value]"
        ///     As above, but the HotSpotUnit is assumed to be HostSpotUnit.Pixel
        /// </remarks>
        private static HotSpot FromString(string stringValue, CultureInfo cultureInfo)
        {
            double value;
            HotSpotUnit unit;
            FromString(stringValue, cultureInfo,
                out value, out unit);

            return new HotSpot(value, unit);
        }

        private static void FromString(string stringValue, CultureInfo cultureInfo, out double value,
                                       out HotSpotUnit unit)
        {
            var goodString = stringValue.Trim().ToLowerInvariant();

            unit = HotSpotUnit.Pixel;

            var strLen = goodString.Length;
            var strLenUnit = 0;
            var unitFactor = 1.0;
            var matchedType = false;

            //  this is where we would handle trailing whitespace on the input string.
            //  peel [unit] off the end of the string
            foreach (var tuple in UnitTuples)
            {
                //  Note: this is NOT a culture specific comparison.
                //  this is by design: we want the same unit string table to work across all cultures. 
                if (!goodString.EndsWith(tuple.Item1, StringComparison.Ordinal))
                {
                    continue;
                }

                strLenUnit = tuple.Item1.Length;
                unit = tuple.Item2;
                matchedType = true;
                break;
            }

            //  we couldn't match a real unit from HotSpotUnit. 
            //  try again with a converter-only unit (a pixel equivalent).
            if (matchedType)
            {
                foreach (var tuple in PixelUniTuples)
                {
                    //  Note: this is NOT a culture specific comparison. 
                    //  this is by design: we want the same unit string table to work across all cultures.
                    if (!goodString.EndsWith(tuple.Item1, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    strLenUnit = tuple.Item1.Length;
                    unitFactor = tuple.Item2;
                    break;
                }
            }

            //  this is where we would handle leading whitespace on the input string. 
            //  this is also where we would handle whitespace between [value] and [unit]. 
            //  check if we don't have a [value].  This is acceptable for certain UnitTypes.

            var valueString = goodString.Substring(0, strLen - strLenUnit);
            value = Convert.ToDouble(valueString, cultureInfo) * unitFactor;
        }

        #region Overrides of TypeConverter

        /// <summary>
        ///     Checks whether or not this class can convert from a given type.
        /// </summary>
        /// <param name="context">
        ///     The ITypeDescriptorContext
        ///     for this call.
        /// </param>
        /// <param name="sourceType">The Type being queried for support.</param>
        /// <returns>
        ///     <c>true</c> if thie converter can convert from the provided type,
        ///     <c>false</c> otherwise.
        /// </returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            // We can only handle strings, integral and floating types
            var tc = Type.GetTypeCode(sourceType);
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (tc)
            {
                case TypeCode.String:
                case TypeCode.Decimal:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        ///     Checks whether or not this class can convert to a given type.
        /// </summary>
        /// <param name="context">
        ///     The ITypeDescriptorContext
        ///     for this call.
        /// </param>
        /// <param name="destinationType">The Type being queried for support.</param>
        /// <returns>
        ///     <c>true</c> if this converter can convert to the provided type,
        ///     <c>false</c> otherwise.
        /// </returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(InstanceDescriptor)
                   || destinationType == typeof(string);
        }

        /// <summary>
        ///     Attempts to convert to a HotSpot from the given object.
        /// </summary>
        /// <param name="context">The ITypeDescriptorContext for this call.</param>
        /// <param name="cultureInfo">The CultureInfo which is respected when converting.</param>
        /// <param name="source">The object to convert to a HotSpot.</param>
        /// <returns>The HotSpot instance which was constructed.</returns>
        /// <exception cref="ArgumentNullException">
        ///     An ArgumentNullException is thrown if the example object is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     An ArgumentException is thrown if the example object is not null
        ///     and is not a valid type which can be converted to a HotSpot.
        /// </exception>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo cultureInfo, object source)
        {
            if (source == null)
            {
                throw GetConvertFromException(null);
            }

            var valueString = source as string;
            if (valueString != null)
            {
                return FromString(valueString, cultureInfo);
            }

            //  conversion from numeric type, presumed to be explicit
            var value = Convert.ToDouble(source, cultureInfo);

            return new HotSpot(value);
        }

        /// <summary>
        ///     Attempts to convert a HotSPot instance to the given type.
        /// </summary>
        /// <param name="typeDescriptorContext">The ITypeDescriptorContext for this call.</param>
        /// <param name="cultureInfo">The CultureInfo which is respected when converting. </param>
        /// <param name="value">The HotSPot to convert.</param>
        /// <param name="destinationType">The type to which to convert the HotSPot instance.</param>
        /// <returns>
        ///     The object which was constructed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     An ArgumentNullException is thrown if the example object is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     An ArgumentException is thrown if the object is not null and is not a HotSPot,
        ///     or if the destinationType isn't one of the valid destination types.
        /// </exception>
        /// <securitynote>
        ///     Critical: calls InstanceDescriptor ctor which LinkDemands
        ///     PublicOK: can only make an InstanceDescriptor for HotSPot, not an arbitrary class
        /// </securitynote>
        [SecurityCritical]
        public override object ConvertTo(
            ITypeDescriptorContext typeDescriptorContext,
            CultureInfo cultureInfo,
            object value,
            Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException(nameof(destinationType));
            }

            if (!(value is HotSpot))
            {
                throw GetConvertToException(value, destinationType);
            }

            var hotSpot = (HotSpot) value;

            if (destinationType == typeof(string))
            {
                return hotSpot.ToString(cultureInfo);
            }

            if (destinationType != typeof(InstanceDescriptor))
            {
                throw GetConvertToException(value, destinationType);
            }

            var constructorInfo = typeof(HotSpot).GetConstructor(new[] {typeof(double), typeof(HotSpotUnit)});
            return new InstanceDescriptor(constructorInfo, new object[] {hotSpot.Value, hotSpot.HotSpotUnit});
        }

        #endregion
    }
}