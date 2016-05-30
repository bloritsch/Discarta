using DHaven.DisCarta.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DHaven.DisCarta
{
    /// <summary>
    /// Represents the current view of a map or map layer.
    /// </summary>
    public class VisualExtent : BasePropertyChanged
    {
        private GeoArea extent;
        private int zoomLevel;
        private Size screen;

        /// <summary>
        /// Gets or sets the geographic extent of the map.
        /// </summary>
        public GeoArea Extent
        {
            get { return extent; }
            set
            {
                if(extent != value)
                {
                    extent = value;
                    RaisePropertyChanged(nameof(Extent));
                }
            }
        }

        /// <summary>
        /// Gets or sets the zoom level displayed.
        /// </summary>
        public int ZoomLevel
        {
            get { return zoomLevel; }
            set
            {
                if(zoomLevel != value)
                {
                    zoomLevel = value;
                    RaisePropertyChanged(nameof(ZoomLevel));
                }
            }
        }

        /// <summary>
        /// Gets or sets the screen size of the map in display units.
        /// </summary>
        public Size Screen
        {
            get { return screen; }
            set
            {
                if (screen != value)
                {
                    screen = value;
                    RaisePropertyChanged(nameof(Screen));
                }
            }
        }
    }
}
