using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DHaven.DisCarta.Projections
{
    /// <summary>
    /// Equirectangular projection would map 1-1 if the size of the map
    /// were the same size as the screen (w: 360, h: 180).  So the lat/lon
    /// are simply scaled according to the size of the extent.  Our screen
    /// size and extent size must be proportional.
    /// </summary>
    public class EquirectangularProjection : IProjection
    {
        public string Name { get { return "WGS 84 / World Equidistant Cylindrical"; } }

        public Size TileSize { get { return new Size(512, 512); } }

        public string WKT
        {
            get
            {
                return @"PROJCS[""WGS 84 / World Equidistant Cylindrical"",
    GEOGCS[""WGS 84"",
        DATUM[""WGS_1984"",
            SPHEROID[""WGS 84"", 6378137, 298.257223563,
                AUTHORITY[""EPSG"", ""7030""]],
            AUTHORITY[""EPSG"", ""6326""]],
        PRIMEM[""Greenwich"", 0,
            AUTHORITY[""EPSG"", ""8901""]],
        UNIT[""degree"", 0.01745329251994328,
            AUTHORITY[""EPSG"", ""9122""]],
        AUTHORITY[""EPSG"", ""4326""],
        AXIS[""Latitude"", NORTH],
        AXIS[""Longitude"", EAST]],
    UNIT[""metre"", 1,
        AUTHORITY[""EPSG"", ""9001""]]]";
            }
        }

        public GeoArea World { get { return new GeoArea(90, -180, -90, 180); } }

        public Size FullMapSizeFor(int zoomLevel)
        {
            double width = 2 * TileSize.Width * Math.Pow(2, zoomLevel);
            double height = TileSize.Height * Math.Pow(2, zoomLevel);
            return new Size(width, height);
        }

        public GeoArea ToGeoArea(Rect rect, VisualExtent mapView)
        {
            return new GeoArea(ToGeoPoint(rect.TopLeft, mapView), ToGeoPoint(rect.BottomRight, mapView));
        }

        public GeoPoint ToGeoPoint(Point point, VisualExtent mapView)
        {
            return new GeoPoint
            {
                Latitude = ToLat(point.Y, mapView),
                Longitude = ToLon(point.X, mapView)
            };
        }

        public Point ToPoint(GeoPoint point, VisualExtent mapView)
        {
            return new Point
            {
                X = ToX(point.Longitude, mapView),
                Y = ToY(point.Latitude, mapView)
            };
        }

        public Rect ToRect(GeoArea extent, VisualExtent mapView)
        {
            return new Rect(ToPoint(extent.NorthWest, mapView), ToPoint(extent.SouthEast, mapView));
        }

        private double ToX(double longitude, VisualExtent mapView)
        {
            return longitude * (mapView.Screen.Width / mapView.Extent.Size.DeltaLongitude);
        }

        private double ToY(double latitude, VisualExtent mapView)
        {
            return latitude * (mapView.Screen.Height / mapView.Extent.Size.DeltaLatitude);
        }

        private double ToLon(double x, VisualExtent mapView)
        {
            return x * (mapView.Extent.Size.DeltaLongitude / mapView.Screen.Width);
        }

        private double ToLat(double y, VisualExtent mapView)
        {
            return y * (mapView.Extent.Size.DeltaLatitude / mapView.Screen.Height);
        }
    }
}
