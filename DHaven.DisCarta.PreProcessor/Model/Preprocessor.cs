using System.Threading.Tasks;

namespace DHaven.DisCarta.PreProcessor.Model
{
    using System.Collections.ObjectModel;
    using Internals;

    /// <summary>
    /// Control class for the Preprocessor application.  Manages metadata and work queue.
    /// </summary>
    public class Preprocessor : BasePropertyChanged
    {
        public ObservableCollection<RasterInfo> PotentialTiles { get; } = new ObservableCollection<RasterInfo>();

        public async Task<RasterInfo> LoadMetadata(string path)
        {
            RasterInfo info = await RasterInfo.CreateFromMapFileAsync(path);

            if (!PotentialTiles.Contains(info))
            {
                PotentialTiles.Add(info);
            }

            return info;
        }
    }
}
