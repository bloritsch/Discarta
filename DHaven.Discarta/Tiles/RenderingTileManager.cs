using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DHaven.DisCarta.Tiles
{
    class RenderingTileManager : ITileManager
    {
        public IEnumerable<Task<Drawing>> GetTilesForArea(IProjection projection, Extent mapArea)
        {
            Rect fullMapSize = new Rect(projection.FullMapSizeFor(mapArea.ZoomLevel));
            Rect mapRect = projection.ToRect(mapArea);

            List<Task<Drawing>> listOfTiles = new List<Task<Drawing>>();

            for(double x = 0; x < fullMapSize.Width; x += projection.TileSize.Width)
            for(double y = 0; y < fullMapSize.Height; y += projection.TileSize.Height)
            {
                Rect currentTile = new Rect(x, y, projection.TileSize.Width, projection.TileSize.Height);

                if (currentTile.IntersectsWith(mapRect))
                {
                    listOfTiles.Add(Task.Run(()=>RenderTile(projection, currentTile, mapArea)));
                }
            }

            return listOfTiles;
        }

        private Drawing RenderTile(IProjection projection, Rect currentTile, Extent mapArea)
        {
            GeoArea tileArea = projection.ToGeoArea(currentTile, mapArea);

            DrawingGroup drawing = new DrawingGroup();
            Geo.SetArea(drawing, tileArea);

            Pen linePen = new Pen(Brushes.DarkGray, 1);
            linePen.Freeze();

            using (DrawingContext context = drawing.Open())
            {
                context.PushClip(new RectangleGeometry(currentTile));

                for (int lat = -90; lat <= 90; lat += 30)
                {
                    Point start = projection.ToPoint(new GeoPoint(lat, 185), mapArea);
                    Point end = projection.ToPoint(new GeoPoint(lat, -180), mapArea);

                    context.DrawLine(linePen, start, end);
                }

                for (int lon = -180; lon <= 180; lon += 30)
                {
                    Point start = projection.ToPoint(new GeoPoint(-90, lon), mapArea);
                    Point end = projection.ToPoint(new GeoPoint(90, lon), mapArea);

                    context.DrawLine(linePen, start, end);
                }

                context.Pop();
            }

            drawing.Freeze();
            return drawing;
        }
    }
}
