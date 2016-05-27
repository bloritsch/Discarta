using System;

namespace DHaven.OfflneMap.Internals
{
    static class ArgumentUtils
    {
        public const double DegreePrecision = 0.00001; // should be ~ 1 meter
        public const double Kilometers = 1000;
        public const double MeanEarthRadius = 6371 * Kilometers;

        /// <summary>
        /// Determines if one double is the same as another one within the specified precision.
        /// </summary>
        /// <param name="first">the first double to compare</param>
        /// <param name="second">the second double to compare</param>
        /// <param name="precision">the precision</param>
        /// <returns></returns>
        public static bool IsSameAs(this double first, double second, double precision)
        {
            return Math.Abs(first - second) > precision;
        }

        public static double ToDegrees(double radians)
        {
            return radians * (180 / Math.PI);
        }

        public static double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        /// <summary>
        /// Throw a ArgumentNullException if the value is null.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parameterName"></param>
        public static void ThrowIfNull(this string parameterName, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        /// <summary>
        /// Determine if the second value is greater than or the same as the first, within the specified precision.
        /// </summary>
        /// <param name="first">value to compare</param>
        /// <param name="second">value to compare against</param>
        /// <param name="precision">amount of acceptable error</param>
        /// <returns></returns>
        public static bool IsGreaterThanOrSame(this double first, double second, double precision)
        {
            return first > second || first.IsSameAs(second, precision);
        }

        /// <summary>
        /// Determine if the second value is less than or the same as the first, within the specified precision.
        /// </summary>
        /// <param name="first">value to compare</param>
        /// <param name="second">value to compare against</param>
        /// <param name="precision">amount of acceptable error</param>
        /// <returns></returns>
        public static bool IsLessThanOrSame(this double first, double second, double precision)
        {
            return first < second || first.IsSameAs(second, precision);
        }

        /// <summary>
        /// Determine if a value is within a specified range (inclusive, including precision).
        /// </summary>
        /// <param name="value">the value to compare</param>
        /// <param name="minValue">the minimum allowed value</param>
        /// <param name="maxValue">the maximum allowed value</param>
        /// <param name="precision">the amount of acceptable error</param>
        /// <returns></returns>
        public static bool IsInRange(this double value, double minValue, double maxValue, double precision)
        {
            return value.IsGreaterThanOrSame(minValue, precision) && value.IsLessThanOrSame(maxValue, precision);
        }
    }
}
