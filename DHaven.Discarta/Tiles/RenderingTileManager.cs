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
    using System.Windows;
    using System.Windows.Media;

    internal class RenderingTileManager : ITileManager
    {
        private Drawing RenderTile(IProjection projection, Rect currentTile, Extent mapArea)
        {
            var degrees = CalculateDegreeLines(mapArea.ZoomLevel);
            var tileArea = projection.ToGeoArea(currentTile, mapArea);

            var drawing = new DrawingGroup();
            Geo.SetArea(drawing, tileArea);

            var linePen = new Pen(Brushes.Red, 1);
            linePen.Freeze();

            using (var context = drawing.Open())
            {
                context.PushClip(new RectangleGeometry(currentTile));

                var outerRect = projection.ToRect(projection.World, mapArea);
                context.DrawRectangle(null, linePen, outerRect);

                for (var lat = 0.0; lat <= projection.World.NorthEast.Latitude; lat += degrees)
                {
                    var start = projection.ToPoint(new GeoPoint(lat, projection.World.NorthEast.Longitude), mapArea);
                    var end = projection.ToPoint(new GeoPoint(lat, projection.World.NorthWest.Longitude), mapArea);

                    context.DrawLine(linePen, start, end);

                    if (lat <= 0)
                    {
                        continue;
                    }

                    start = projection.ToPoint(new GeoPoint(-lat, projection.World.NorthEast.Longitude), mapArea);
                    end = projection.ToPoint(new GeoPoint(-lat, projection.World.NorthWest.Longitude), mapArea);

                    context.DrawLine(linePen, start, end);
                }

                for (var lon = 0.0; lon <= projection.World.NorthEast.Longitude; lon += degrees)
                {
                    var start = projection.ToPoint(new GeoPoint(projection.World.NorthEast.Latitude, lon), mapArea);
                    var end = projection.ToPoint(new GeoPoint(projection.World.SouthEast.Latitude, lon), mapArea);

                    context.DrawLine(linePen, start, end);

                    if (lon <= 0)
                    {
                        continue;
                    }

                    start = projection.ToPoint(new GeoPoint(projection.World.NorthEast.Latitude, -lon), mapArea);
                    end = projection.ToPoint(new GeoPoint(projection.World.SouthEast.Latitude, -lon), mapArea);

                    context.DrawLine(linePen, start, end);
                }

                context.Pop();
            }

            drawing.Freeze();
            return drawing;
        }

        private static double CalculateDegreeLines(int zoomLevel)
        {
            if (zoomLevel <= 1)
            {
                return 50;
            }

            if (zoomLevel <= 3)
            {
                return 30;
            }

            if (zoomLevel <= 6)
            {
                return 20;
            }

            return 10;
        }

        #region Implementations

        public IEnumerable<Task<Drawing>> GetTilesForArea(IProjection projection, Extent mapArea)
        {
            var fullMapSize = new Rect(projection.FullMapSizeFor(mapArea.ZoomLevel));
            var mapRect = projection.ToRect(mapArea);

            var listOfTiles = new List<Task<Drawing>>();

            for (double x = 0; x < fullMapSize.Width; x += projection.TileSize.Width)
                for (double y = 0; y < fullMapSize.Height; y += projection.TileSize.Height)
                {
                    var currentTile = new Rect(x, y, projection.TileSize.Width, projection.TileSize.Height);

                    if (currentTile.IntersectsWith(mapRect))
                    {
                        listOfTiles.Add(Task.Run(() => RenderTile(projection, currentTile, mapArea)));
                    }
                }

            return listOfTiles;
        }

        #endregion
    }
}