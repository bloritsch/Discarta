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
using DHaven.DisCarta.Internals;
using System;
using System.Windows;

namespace DHaven.DisCarta.Projections
{
    public class PseudoMercatorProjection : IProjection
    {
        private const double K = 128 / Math.PI;

        public string Name { get { return "WGS 84 / Pseudo - Mercator"; } }

        public string WKT
        {
            get
            {
                return @"PROJCS[""WGS 84 / Pseudo - Mercator"",
    GEOGCS[""WGS 84"",
        DATUM[""WGS_1984"",
            SPHEROID[""WGS 84"", 6378137, 298.257223563,
                AUTHORITY[""EPSG"", ""7030""]],
            AUTHORITY[""EPSG"", ""6326""]],
        PRIMEM[""Greenwich"", 0,
            AUTHORITY[""EPSG"", ""8901""]],
        UNIT[""degree"", 0.0174532925199433,
            AUTHORITY[""EPSG"", ""9122""]],
        AUTHORITY[""EPSG"", ""4326""]],
    PROJECTION[""Mercator_1SP""],
    PARAMETER[""central_meridian"", 0],
    PARAMETER[""scale_factor"", 1],
    PARAMETER[""false_easting"", 0],
    PARAMETER[""false_northing"", 0],
    UNIT[""metre"", 1,
        AUTHORITY[""EPSG"", ""9001""]],
    AXIS[""X"", EAST],
    AXIS[""Y"", NORTH],
    EXTENSION[""PROJ4"", ""+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +wktext  +no_defs""],
    AUTHORITY[""EPSG"", ""3857""]]";
            }
        }

        public GeoArea World { get { return new GeoArea(85.051129, 180, -85.051129, -180); } }

        public Size TileSize { get { return new Size(256, 256); } }

        public Size FullMapSizeFor(int zoomLevel)
        {
            double side = TileSize.Width * Math.Pow(2, zoomLevel);
            return new Size(side, side);
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

        public Rect ToRect(Extent mapView)
        {
            return new Rect(ToPoint(mapView.Area.NorthWest, mapView), ToPoint(mapView.Area.SouthEast, mapView));
        }

        public Rect ToRect(GeoArea extent, Extent mapView)
        {
            return new Rect(ToPoint(extent.NorthWest, mapView), ToPoint(extent.SouthEast, mapView));
        }

        private double ToX(double longitude, Extent mapView)
        {
            // TODO: adjust for screen size!!!
            // Does not consider extent and screen size as a view into the map at
            // whatever the zoom size is.  It projects the point on the absolute
            // coordinate from 0,0 being NorthWest.
            double zoomFactor = K * Math.Pow(2, mapView.ZoomLevel);
            return zoomFactor * (ArgumentUtils.ToRadians(longitude) + Math.PI);
        }

        private double ToY(double latitude, Extent mapView)
        {
            // TODO: adjust for screen size!!!
            // Does not consider extent and screen size as a view into the map at
            // whatever the zoom size is.  It projects the point on the absolute
            // coordinate from 0,0 being NorthWest.
            double zoomFactor = K * Math.Pow(2, mapView.ZoomLevel);
            return zoomFactor * (Math.PI - Math.Log(Math.Tan(Math.PI / 4 + ArgumentUtils.ToRadians(latitude) / 2)));
        }

        private double ToLon(double x, Extent mapView)
        {
            // TODO: adjust for screen size!!!
            double zoomFactor = K * Math.Pow(2, mapView.ZoomLevel);
            return ArgumentUtils.ToDegrees(x / zoomFactor - Math.PI);
        }

        private double ToLat(double y, Extent mapView)
        {
            // TODO: adjust for screen size!!!
            double zoomFactor = K * Math.Pow(2, mapView.ZoomLevel);
            return ArgumentUtils.ToDegrees(2 * (Math.Atan(Math.Exp(Math.PI - y / zoomFactor)) - Math.PI / 4));
        }
    }
}
