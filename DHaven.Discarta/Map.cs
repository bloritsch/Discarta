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

namespace DHaven.DisCarta
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using Internals;
    using Projections;
    using Tiles;

    public class Map : ScrollablePanel
    {
        public const int MaxZoomLevel = 19; // total of 20 zoom levels from 0-19.

        public static readonly DependencyProperty ProjectionProperty = DependencyProperty.RegisterAttached(
            "Projection", typeof(IProjection), typeof(Map),
            new FrameworkPropertyMetadata(new EquirectangularProjection(),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty ExtentProperty = DependencyProperty.RegisterAttached(
            "ExtentProperty", typeof(Extent), typeof(Map),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
                VisualExtentChanged));

        internal static readonly DependencyProperty OffsetProperty = DependencyProperty.RegisterAttached(
            "Offset", typeof(Vector), typeof(Map),
            new FrameworkPropertyMetadata(default(Vector), FrameworkPropertyMetadataOptions.AffectsArrange));

        private readonly ITileManager tileManager;
        private bool loading = true;

        public Map()
        {
            tileManager = new RenderingTileManager();
            Loaded += MapLoaded;
            ClipToBounds = true;
        }

        public IProjection Projection
        {
            get { return GetProjection(this); }
            set { SetProjection(this, value); }
        }

        public Extent Extent
        {
            get { return GetExtent(this); }
            set { SetExtent(this, value); }
        }

        public static IProjection GetProjection(DependencyObject dependencyObject)
        {
            return dependencyObject.GetValue(ProjectionProperty) as IProjection;
        }

        public static void SetProjection(DependencyObject dependencyObject, IProjection value)
        {
            dependencyObject.SetValue(ProjectionProperty, value);
        }

        public static Extent GetExtent(DependencyObject dependencyObject)
        {
            return dependencyObject.GetValue(ExtentProperty) as Extent;
        }

        public static void SetExtent(DependencyObject dependencyObject, Extent value)
        {
            dependencyObject.SetValue(ExtentProperty, value);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (loading || Projection == null || Extent == null || Extent.Area.IsEmpty)
            {
                return finalSize;
            }

            var offset = PanelExtent.TopLeft - ViewPort.TopLeft;

            foreach (UIElement child  in InternalChildren)
            {
                var placement = PanelExtent;

                if (!(child is MapLayer))
                {
                    placement = Projection.ToRect(Geo.GetArea(child), Extent);
                }

                placement.X += offset.X;
                placement.Y += offset.Y;

                child.Arrange(placement);
            }

            return ViewPort.Size;
        }

        protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
        {
            var collection = new NotifyingUIElementCollection(this, logicalParent);
            collection.CollectionChanged += ChildrenCollectionChanged;

            return collection;
        }

        private void BindLayer(MapLayer child)
        {
            if (child == null)
            {
                return;
            }

            child.SetBinding(ProjectionProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath(ProjectionProperty),
                Mode = BindingMode.TwoWay
            });

            child.SetBinding(ExtentProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath(ExtentProperty),
                Mode = BindingMode.TwoWay
            });
        }

        #region Overrides of ScrollablePanel

        /// <summary>
        /// Override this to perform work if the view port chnages size, etc.
        /// </summary>
        protected override void OnViewPortChanged()
        {
            base.OnViewPortChanged();

            if (Projection != null && Extent != null)
            {
                Extent.Area = Projection.ToGeoArea(ViewPort, Extent);
            }
        }

        #endregion

        private void UnbindLayer(MapLayer child)
        {
            if (child == null)
            {
                return;
            }

            BindingOperations.ClearBinding(child, ProjectionProperty);
            BindingOperations.ClearBinding(child, ExtentProperty);
        }

        private static void VisualExtentChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var map = d as Map;

            if (map == null)
            {
                return;
            }

            var oldExtent = args.OldValue as Extent;
            if (oldExtent != null)
            {
                oldExtent.PropertyChanged -= map.VisualExtentValuesChanged;
            }

            var newExtent = args.NewValue as Extent;
            if (newExtent != null)
            {
                newExtent.PropertyChanged += map.VisualExtentValuesChanged;
            }
        }

        private async void VisualExtentValuesChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == string.Empty || e.PropertyName == nameof(Extent.ZoomLevel))
            {
                PanelExtent = new Rect(Projection.FullMapSizeFor(Extent.ZoomLevel));

                // Measurements only change when the zoom level changes.  Screen size affects the visual port,
                // and GeoArea affects the geographic area under the map
                InvalidateMeasure();
            }

            ScrollOwner?.InvalidateScrollInfo();
            InvalidateArrange();

            foreach (var child in InternalChildren.OfType<UIElement>().Where(c => !(c is MapLayer)).ToList())
            {
                Children.Remove(child);
            }

            var tiles = tileManager.GetTilesForArea(Projection, Extent).ToList();
            while (tiles.Any())
            {
                await Task.WhenAny(tiles);

                foreach (var finishedDrawing in tiles.Where(t => t.IsCompleted).ToList())
                {
                    var tileContent = await finishedDrawing;
                    var visual = new Rectangle();
                    Geo.SetArea(visual, Geo.GetArea(tileContent));

                    var brush = new DrawingBrush
                    {
                        Drawing = tileContent,
                        Stretch = Stretch.Fill
                    };

                    brush.Freeze();
                    visual.Fill = brush;
                    SetZIndex(visual, int.MaxValue);

                    Children.Add(visual);
                    tiles.Remove(finishedDrawing);
                }
            }
        }

        private void ChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (UIElement child in e.OldItems)
                {
                    UnbindLayer(child as MapLayer);
                }
            }

            if (e.NewItems != null)
            {
                foreach (UIElement child in e.NewItems)
                {
                    BindLayer(child as MapLayer);
                }
            }
        }

        private void MapLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MapLoaded;
            loading = false;

            // Size the map so it is big enough to cover the visible area

            ViewPort.Size = new Size(ActualWidth, ActualHeight);
            var desiredZoomLevel = -1;

            while (ExtentWidth < ViewportWidth || ExtentHeight < ViewportHeight)
            {
                desiredZoomLevel++;
                PanelExtent.Size = Projection.FullMapSizeFor(desiredZoomLevel);
            }

            Extent = new Extent
            {
                Area = Projection.World,
                ZoomLevel = desiredZoomLevel
            };

            // for now let's always center it (view port support is comming)
            SetHorizontalOffset((ExtentWidth - ViewportWidth) / 2);
            SetVerticalOffset((ExtentHeight - ViewportHeight) / 2);

            ScrollOwner?.InvalidateScrollInfo();
            VisualExtentValuesChanged(this, new PropertyChangedEventArgs(string.Empty));
        }

        #region Implementations

        public override Rect MakeVisible(Visual visual, Rect rectangle)
        {
            // We'll just assume the source Rect is correct for now.
            // When we actually get to placing things on screen
            // we'll fix this.
            rectangle.Intersect(PanelExtent);

            return rectangle;
        }

        public override void MouseWheelDown()
        {
            ChangeZoom(Math.Max(0, Extent.ZoomLevel - 1));
        }

        public override void MouseWheelLeft()
        {
            MouseWheelDown();
        }

        public override void MouseWheelRight()
        {
            MouseWheelUp();
        }

        public override void MouseWheelUp()
        {
            ChangeZoom(Math.Min(MaxZoomLevel, Extent.ZoomLevel + 1));
        }

        private void ChangeZoom(int newZoomLevel)
        {
            var currentCenterPoint = new Point
            {
                X = ViewPort.X + ViewportWidth / 2,
                Y = ViewPort.Y + ViewportHeight / 2
            };

            var proportion = currentCenterPoint.X / ExtentWidth;

            // TODO: alter extent to use mouse position for zoom anchor
            Extent.ZoomLevel = newZoomLevel;

            // The PanelExtent is now updated.
            SetHorizontalOffset((currentCenterPoint.X * proportion) - ViewPort.X);
            SetVerticalOffset((currentCenterPoint.Y * proportion) - ViewPort.Y);
        }

        #endregion
    }
}