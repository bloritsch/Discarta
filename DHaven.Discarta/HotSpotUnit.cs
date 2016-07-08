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
    /// <summary>
    ///     Determines how HotSpot calculates it's offset.
    /// </summary>
    public enum HotSpotUnit
    {
        /// <summary>
        ///     Technically display units, but keeping parity with GridLength
        /// </summary>
        Pixel,

        /// <summary>
        ///     The value is a percent (value must be between 0 and 1)
        /// </summary>
        Percent
    }
}