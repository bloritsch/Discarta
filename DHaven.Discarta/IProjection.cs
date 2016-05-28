using System.Collections.Generic;
using System.Windows;

namespace DHaven.DisCarta
{
    /// <summary>
    /// Represents a standard map projection used to convert geographic points
    /// on to a flat screen.
    /// </summary>
    public interface IProjection
    {
        /// <summary>
        /// The name of the projection.  Used for debugging, can be Well Known Text, a reference number, etc.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The Well Known Text definition.  Used by GDAL, and many other spatial system.
        /// </summary>
        string WKT { get; }

        GeoArea World { get; }

        /// <summary>
        /// Gets the standard tile size for this projection
        /// </summary>
        Size TileSize { get; }

        /// <summary>
        /// Convert a GeoPoint to a screen Point.
        /// </summary>
        /// <param name="point">the GeoPoint to convert</param>
        /// <param name="mapView">the current map view</param>
        /// <returns>the screen coordinate</returns>
        Point ToPoint(GeoPoint point, MapView mapView);

        /// <summary>
        /// Convert a GeoArea to a placement Rect.
        /// </summary>
        /// <param name="extent">the GeoArea to convert</param>
        /// <param name="mapView">the current map view</param>
        /// <returns>the placement rect</returns>
        Rect ToRect(GeoArea extent, MapView mapView);

        /// <summary>
        /// Convert a screen Point to a GeoPoint.
        /// </summary>
        /// <param name="point">the screen point to convert</param>
        /// <param name="mapView">the current map view</param>
        /// <returns>the corresponding GeoPoint</returns>
        GeoPoint ToGeoPoint(Point point, MapView mapView);

        /// <summary>
        /// Convert a placement rect to a GeoArea.
        /// </summary>
        /// <param name="rect">the placement rect to convert</param>
        /// <param name="mapView">the current map view</param>
        /// <returns>the corresponding GeoArea</returns>
        GeoArea ToGeoArea(Rect rect, MapView mapView);

        /// <summary>
        /// Used to retrieve map tiles from the file system or a remote
        /// map server.  Gets all tiles that are completely or partially
        /// contained in this map view.
        /// </summary>
        /// <param name="mapView">the current map view</param>
        /// <returns>the set of tiles that are displayed on screen</returns>
        ICollection<TileCoord> GetAffectedTiles(MapView mapView);
    }
}
