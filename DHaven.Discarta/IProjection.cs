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
        /// Calculate the full map size for the zoom level requested.
        /// </summary>
        /// <param name="zoomLevel">calculates the full map size for the zoom level requested</param>
        /// <returns>the size in display units</returns>
        Size FullMapSizeFor(int zoomLevel);

        /// <summary>
        /// Convert a GeoPoint to a screen Point.
        /// </summary>
        /// <param name="point">the GeoPoint to convert</param>
        /// <param name="mapView">the current map view</param>
        /// <returns>the screen coordinate</returns>
        Point ToPoint(GeoPoint point, VisualExtent mapView);

        /// <summary>
        /// Convert a GeoArea to a placement Rect.
        /// </summary>
        /// <param name="extent">the GeoArea to convert</param>
        /// <param name="mapView">the current map view</param>
        /// <returns>the placement rect</returns>
        Rect ToRect(GeoArea extent, VisualExtent mapView);

        /// <summary>
        /// Convert a screen Point to a GeoPoint.
        /// </summary>
        /// <param name="point">the screen point to convert</param>
        /// <param name="mapView">the current map view</param>
        /// <returns>the corresponding GeoPoint</returns>
        GeoPoint ToGeoPoint(Point point, VisualExtent mapView);

        /// <summary>
        /// Convert a placement rect to a GeoArea.
        /// </summary>
        /// <param name="rect">the placement rect to convert</param>
        /// <param name="mapView">the current map view</param>
        /// <returns>the corresponding GeoArea</returns>
        GeoArea ToGeoArea(Rect rect, VisualExtent mapView);
    }
}
