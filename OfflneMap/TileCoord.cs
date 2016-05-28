using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHaven.DisCarta
{
    /// <summary>
    /// Represents a tile coordinate for a specific zoom level.
    /// </summary>
    public struct TileCoord
    {
        public int ZoomLevel { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
