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
            var tileArea = projection.ToGeoArea(currentTile, mapArea);

            var drawing = new DrawingGroup();
            Geo.SetArea(drawing, tileArea);

            var linePen = new Pen(Brushes.Black, 1);
            linePen.Freeze();

            using (var context = drawing.Open())
            {
                context.PushClip(new RectangleGeometry(currentTile));

                for (var lat = -90; lat <= 90; lat += 30)
                {
                    var start = projection.ToPoint(new GeoPoint(lat, 180), mapArea);
                    var end = projection.ToPoint(new GeoPoint(lat, -180), mapArea);

                    context.DrawLine(linePen, start, end);
                }

                for (var lon = -180; lon <= 180; lon += 30)
                {
                    var start = projection.ToPoint(new GeoPoint(-90, lon), mapArea);
                    var end = projection.ToPoint(new GeoPoint(90, lon), mapArea);

                    context.DrawLine(linePen, start, end);
                }

                context.Pop();
            }

            drawing.Freeze();
            return drawing;
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