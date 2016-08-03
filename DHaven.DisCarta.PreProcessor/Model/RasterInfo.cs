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

namespace DHaven.DisCarta.PreProcessor.Model
{
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows;
    using OSGeo.GDAL;

    /// <summary>
    /// Represents the important metadata for any given image.  This lets us plan ahead before
    /// committing to the process of warping and tiling.  It also allows us to plan the image
    /// for the tile layers.
    /// </summary>
    public class RasterInfo
    {
        /// <summary>
        ///     Internal constructor, prevents other classes from instantiating it directly
        /// </summary>
        private RasterInfo(string fullPath)
        {
            FullPath = fullPath;
            FileName = Path.GetFileName(FullPath);
            Directory = Path.GetDirectoryName(FullPath);
        }

        /// <summary>
        /// Gets the FullPath to the file that this metadata belongs to.
        /// </summary>
        public string FullPath { get; }

        public string FileName { get; private set; }

        public string Directory { get; private set; }

        /// <summary>
        /// Gets the projection used in the raster file so we can determine if it needs warping.
        /// </summary>
        public string Projection { get; private set; }

        /// <summary>
        /// Gets the raw raster image size in pixels.  NOTE: we can read in the important parts
        /// as needed.
        /// </summary>
        public Size ImageSize { get; private set; }

        /// <summary>
        /// Gets the number of bands/color channels in the image.
        /// </summary>
        public int NumberOfBands { get; private set; }

        /// <summary>
        /// Gets the area on the map that this file belongs to.
        /// </summary>
        public GeoArea MapArea { get; private set; }

        /// <summary>
        /// Asynchronously reads the metadata.
        /// </summary>
        /// <param name="fullPath">the file to read metadata from</param>
        /// <returns>the RasterInfo for that file</returns>
        public static Task<RasterInfo> CreateFromMapFileAsync(string fullPath)
        {
            return Task.Run(() => CreateFromMapFile(fullPath));
        }

        /// <summary>
        /// Reads the metadata from a file.  NOTE: this is a synchronous call.
        /// </summary>
        /// <param name="fullPath">the file to read the metadata from</param>
        /// <returns>the RasterInfo for that file</returns>
        public static RasterInfo CreateFromMapFile(string fullPath)
        {
            var rasterInfo = new RasterInfo(fullPath);

            using (var dataset = Gdal.OpenShared(fullPath, Access.GA_ReadOnly))
            {
                rasterInfo.ImageSize = new Size(dataset.RasterXSize, dataset.RasterYSize);
                rasterInfo.NumberOfBands = dataset.RasterCount;
                rasterInfo.Projection = dataset.GetProjection();
                rasterInfo.MapArea = CalculateMapArea(dataset);
            }

            Preprocessor pre = new Preprocessor();
            pre.ProjectAndTile(rasterInfo, 0, null);
            pre.ProjectAndTile(rasterInfo, 1, null);

            return rasterInfo;
        }

        /// <summary>
        /// The geographic bounds are calculated from the Dataset's Geo Transform.
        /// </summary>
        /// <param name="dataset">the Dataset for the iamge</param>
        /// <returns>the GeoArea for the image</returns>
        private static GeoArea CalculateMapArea(Dataset dataset)
        {
            var geoTransform = new double[6];
            dataset.GetGeoTransform(geoTransform);

            var topLeft = new GeoPoint(geoTransform[3], geoTransform[0]);
            var bottomRight = new GeoPoint(
                topLeft.Latitude + geoTransform[5] * dataset.RasterYSize,
                topLeft.Longitude + geoTransform[1] * dataset.RasterXSize);

            return new GeoArea(topLeft, bottomRight);
        }
    }
}