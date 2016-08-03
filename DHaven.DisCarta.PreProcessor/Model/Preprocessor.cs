using System.Threading.Tasks;

namespace DHaven.DisCarta.PreProcessor.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Security.AccessControl;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Xml.Schema;
    using Internals;
    using OSGeo.GDAL;
    using Projections;

    /// <summary>
    /// Control class for the Preprocessor application.  Manages metadata and work queue.
    /// </summary>
    public class Preprocessor : BasePropertyChanged
    {
        private static IProjection[] SupportedProjections = {new EquirectangularProjection(), new PseudoMercatorProjection()};

        public ObservableCollection<RasterInfo> PotentialTiles { get; } = new ObservableCollection<RasterInfo>();

        public async Task<RasterInfo> LoadMetadata(string path)
        {
            var info = await RasterInfo.CreateFromMapFileAsync(path);

            if (!PotentialTiles.Contains(info))
            {
                PotentialTiles.Add(info);
            }

            return info;
        }

        public void ProjectAndTile(RasterInfo originFile, int zoomLevel, IProgress<Status> progress )
        {
            var status = new Status
            {
                Total = SupportedProjections.Length,
                Current = 0
            };

            foreach (var projection in SupportedProjections)
            {
                Size fullMapSize = projection.FullMapSizeFor(zoomLevel);
                var tilesWide = fullMapSize.Width / projection.TileSize.Width;
                var tilesTall = fullMapSize.Height / projection.TileSize.Height;

                status.Current++;
                status.Total += tilesWide * tilesTall;
                status.Message = $"Projecting {projection.Name} at Zoom Level {zoomLevel}";
                progress?.Report(status);

                string baseDir = Path.Combine(projection.GetType().Name, $"{zoomLevel}");
                if (!Directory.Exists(baseDir))
                {
                    Directory.CreateDirectory(baseDir);
                }

                using (var source = Gdal.OpenShared(originFile.FullPath, Access.GA_ReadOnly))
                using (var destination = Gdal.AutoCreateWarpedVRT(source, source.GetProjection(), projection.Wkt,
                        ResampleAlg.GRA_NearestNeighbour, .0125))
                {
                    var sourceInfo = DetermineRenderInformation(destination);

                    var ratioX = destination.RasterXSize / fullMapSize.Width;
                    var ratioY = destination.RasterYSize / fullMapSize.Height;

                    var destWidth = (int) projection.TileSize.Width;
                    var destHeight = (int) projection.TileSize.Height;
                    var sourceWidth = (int) (destWidth * ratioX);
                    var sourceHeight = (int) (destHeight * ratioY);

                    var buffer = new byte[destWidth * destHeight * sourceInfo.ChannelCount];
                    var stride = destWidth * sourceInfo.ChannelCount;

                    for (int row = 0; row < tilesWide; row++)
                    for (int column = 0; column < tilesTall; column++)
                    {
                        int x = column * sourceWidth;
                        int y = row * sourceHeight;
                        destination.ReadRaster(x, y, sourceWidth, sourceHeight, buffer, destWidth,
                            destHeight, sourceInfo.ChannelCount, sourceInfo.BandMap, sourceInfo.PixelSpace, stride, 1);

                        var bitmap = BitmapSource.Create(destWidth, destHeight, 96, 96, sourceInfo.PixelFormat,
                            sourceInfo.Colors, buffer, stride);

                        using (var stream = File.Create(Path.Combine(baseDir, $"{row}-{column}.png")))
                        {
                            var encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(bitmap));
                            encoder.Save(stream);
                        }

                        status.Current++;
                        progress?.Report(status);
                    }
                }
            }

            status.Current = status.Total;
            status.Message = string.Empty;
            progress?.Report(status);
        }

        private RenderInformation DetermineRenderInformation(Dataset dataset)
        {
            bool hasAlpha = false;
            bool isIndexed = false;
            int channelSize = -1;
            ColorTable colorTable = null;

            RenderInformation info = new RenderInformation
            {
                BandMap = new[] {1, 1, 1, 1},
                ChannelCount = dataset.RasterCount
            };

            foreach (int i in Enumerable.Range(0, info.ChannelCount))
            {
                int bandNumber = i + 1;

                using (Band band = dataset.GetRasterBand(bandNumber))
                {
                    channelSize = Gdal.GetDataTypeSize(band.DataType);

                    switch (band.GetRasterColorInterpretation())
                    {
                        case ColorInterp.GCI_RedBand:
                            info.BandMap[2] = bandNumber;
                            break;

                        case ColorInterp.GCI_GreenBand:
                            info.BandMap[1] = bandNumber;
                            break;

                        case ColorInterp.GCI_BlueBand:
                            info.BandMap[0] = bandNumber;
                            break;

                        case ColorInterp.GCI_AlphaBand:
                            hasAlpha = true;
                            info.BandMap[3] = bandNumber;
                            break;

                        case ColorInterp.GCI_PaletteIndex:
                            colorTable = band.GetRasterColorTable();
                            isIndexed = true;
                            info.BandMap[0] = bandNumber;
                            break;

                        case ColorInterp.GCI_GrayIndex:
                            isIndexed = true;
                            info.BandMap[0] = bandNumber;
                            break;

                        default:
                            if (i < 4 && info.BandMap[i] == 0)
                            {
                                info.ChannelCount = Math.Min(info.ChannelCount, bandNumber - 1);
                                info.BandMap[i] = bandNumber;
                            }

                            break;
                    }
                }
            }

            if (isIndexed)
            {
                info.PixelFormat = PixelFormats.Indexed8;
                info.PixelSpace = 1;

                if (colorTable == null)
                {
                    info.Colors = BitmapPalettes.Gray256Transparent;
                }
                else
                {
                    var colors = new List<Color>();

                    for (var i = 0; i < colorTable.GetCount(); i++)
                    {
                        var color = colorTable.GetColorEntry(i);

                        colors.Add(Color.FromArgb((byte)color.c4, (byte)color.c1, (byte)color.c2, (byte)color.c3));
                    }

                    info.Colors = new BitmapPalette(colors);
                }
            }
            else
            {
                int multiplier = channelSize / 8;
                if (info.ChannelCount == 1)
                {
                    info.PixelFormat = channelSize > 8 ? PixelFormats.Gray16 : PixelFormats.Gray8;
                    info.PixelSpace = multiplier;
                }
                else
                {
                    if (hasAlpha)
                    {
                        info.PixelFormat = channelSize > 8 ? PixelFormats.Rgba64 : PixelFormats.Bgra32;
                    }
                    else
                    {
                        info.PixelFormat = channelSize > 8 ? PixelFormats.Rgb48 : PixelFormats.Bgr24;
                    }

                    info.PixelSpace = multiplier * info.ChannelCount;
                }
            }

            return info;
        }

        private struct RenderInformation
        {
            public int ChannelCount;
            public int[] BandMap;
            public PixelFormat PixelFormat;
            public int PixelSpace;
            public BitmapPalette Colors;
        }
    }
}
