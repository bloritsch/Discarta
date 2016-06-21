using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DHaven.DisCarta.Tiles
{
    /// <summary>
    /// The tile manager is the interface to get the collection of drawings used
    /// to draw the map background.
    /// </summary>
    public interface ITileManager
    {
        /// <summary>
        /// Get all the tiles for the requested area.  The set of tiles returned
        /// are designed to be placed according to the GeoArea that is attached to them.
        /// All drawings returned are frozen.
        /// </summary>
        /// <param name="projection">The projection to use for the tile rendering</param>
        /// <param name="mapArea">the map area and zoom level to use to render the tiles</param>
        /// <returns>a collection of awaitable tasks to place on screen.</returns>
        IEnumerable<Task<Drawing>> GetTilesForArea(IProjection projection, Extent mapArea);
    }
}
