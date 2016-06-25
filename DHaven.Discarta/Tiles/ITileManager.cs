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

namespace DHaven.DisCarta.Tiles
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows.Media;

    /// <summary>
    ///     The tile manager is the interface to get the collection of drawings used
    ///     to draw the map background.
    /// </summary>
    public interface ITileManager
    {
        /// <summary>
        ///     Get all the tiles for the requested area.  The set of tiles returned
        ///     are designed to be placed according to the GeoArea that is attached to them.
        ///     All drawings returned are frozen.
        /// </summary>
        /// <param name="projection">The projection to use for the tile rendering</param>
        /// <param name="mapArea">the map area and zoom level to use to render the tiles</param>
        /// <returns>a collection of awaitable tasks to place on screen.</returns>
        IEnumerable<Task<Drawing>> GetTilesForArea(IProjection projection, Extent mapArea);
    }
}