#region Copyright 2016 D-Haven.org
/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion
using System;
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

        public GeoArea ToGeoArea(Rect rect, Extent mapView)
        {
            return new GeoArea(ToGeoPoint(rect.TopLeft, mapView), ToGeoPoint(rect.BottomRight, mapView));
        }

        public GeoPoint ToGeoPoint(Point point, Extent mapView)
        {
            return new GeoPoint
            {
                Latitude = ToLat(point.Y, mapView),
                Longitude = ToLon(point.X, mapView)
            };
        }

        public Point ToPoint(GeoPoint point, Extent mapView)
        {
            return new Point
            {
                X = ToX(point.Longitude, mapView),
                Y = ToY(point.Latitude, mapView)
            };
        }

        public Rect ToRect(GeoArea extent, Extent mapView)
        {
            return new Rect(ToPoint(extent.NorthWest, mapView), ToPoint(extent.SouthEast, mapView));
        }

        private double ToX(double longitude, Extent mapView)
        {
            return longitude * (mapView.Screen.Width / mapView.Area.Size.DeltaLongitude);
        }

        private double ToY(double latitude, Extent mapView)
        {
            return latitude * (mapView.Screen.Height / mapView.Area.Size.DeltaLatitude);
        }

        private double ToLon(double x, Extent mapView)
        {
            return x * (mapView.Area.Size.DeltaLongitude / mapView.Screen.Width);
        }

        private double ToLat(double y, Extent mapView)
        {
            return y * (mapView.Area.Size.DeltaLatitude / mapView.Screen.Height);
        }
    }
}
