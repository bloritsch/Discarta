namespace DHaven.DisCarta.PreProcessor
{
    using System;
    using Internal;
    using Model;
    using Projections;

    public class RasterInfoViewModel : ViewModel<RasterInfo>
    {
        public IProjection Projection { get; set; }

        public double? TilesAccross => Model?.ImageSize.Width / Projection.TileSize.Width;

        public double? TilesDown => Model?.ImageSize.Height / Projection.TileSize.Height;

        public double? MaxZoomLevel => TilesAccross.HasValue ? Math.Round(Math.Log(TilesAccross.Value, 2)) : (double?)null;

        #region Overrides of ViewModel<RasterInfo>

        protected override void OnModelChanged()
        {
            if (Model == null)
            {
                Projection = null;
                return;
            }

            // To make the compiler happy, one of the options has to be cast to IProjection.
            Projection = Model.Projection.Contains("Mercator")
                ? (IProjection)new PseudoMercatorProjection()
                : new EquirectangularProjection();
        }

        #endregion
    }
}
