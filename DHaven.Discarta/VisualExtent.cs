using DHaven.DisCarta.Internals;
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
