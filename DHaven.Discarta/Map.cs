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
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using Internals;
    using Projections;
    using Tiles;

    public class Map : Panel, IScrollInfo
    {
        public const int MaxZoomLevel = 19; // total of 20 zoom levels from 0-19.
        private const double LineSize = 96 / 2.54; // 1 cm in DPU

        public static readonly DependencyProperty ProjectionProperty = DependencyProperty.RegisterAttached(
            "Projection", typeof(IProjection), typeof(Map),
            new FrameworkPropertyMetadata(new EquirectangularProjection(),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty ExtentProperty = DependencyProperty.RegisterAttached(
            "ExtentProperty", typeof(Extent), typeof(Map),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
                VisualExtentChanged));

        private ScrollViewer owner;
        private Rect mapRect;
        private Rect viewPort;
        private bool loading = true;
        private readonly ITileManager tileManager;

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

        public bool CanVerticallyScroll { get; set; }

        public bool CanHorizontallyScroll { get; set; }

        public double ExtentWidth
        {
            get { return mapRect.Width; }
        }

        public double ExtentHeight
        {
            get { return mapRect.Height; }
        }

        public double ViewportWidth
        {
            get { return viewPort.Width; }
        }

        public double ViewportHeight
        {
            get { return viewPort.Height; }
        }

        public double HorizontalOffset
        {
            get { return -viewPort.X; }
        }

        public double VerticalOffset
        {
            get { return -viewPort.Y; }
        }

        public ScrollViewer ScrollOwner
        {
            get { return owner; }
            set
            {
                if (!ReferenceEquals(owner, value))
                {
                    owner = value;

                    owner.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    owner.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                }
            }
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

        protected override Size MeasureOverride(Size availableSize)
        {
            if (loading || Projection == null || Extent == null)
            {
                return availableSize;
            }

            var extent = Projection.FullMapSizeFor(Extent.ZoomLevel);
            var extentOrViewChanged = false;
            if (extent != mapRect.Size)
            {
                mapRect = new Rect(extent);
                extentOrViewChanged = true;
            }

            if (availableSize != viewPort.Size)
            {
                viewPort.Size = availableSize;
                extentOrViewChanged = true;
            }

            if (extentOrViewChanged && ScrollOwner != null)
            {
                CanHorizontallyScroll = ExtentWidth > ViewportWidth;
                CanVerticallyScroll = ExtentHeight > ViewportHeight;

                ScrollOwner.InvalidateScrollInfo();
            }

            foreach (UIElement child in InternalChildren)
            {
                var desiredSize = child is MapLayer ? mapRect.Size : new Size();
                child.Measure(mapRect.Size);
            }

            return mapRect.Size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (loading || Projection == null || Extent == null)
            {
                return finalSize;
            }

            if (finalSize != viewPort.Size)
            {
                viewPort.Size = finalSize;

                if (ScrollOwner != null)
                {
                    ScrollOwner.InvalidateScrollInfo();
                }
            }

            var viewRect = Projection.ToRect(Extent.Area, Extent);
            viewRect.X = viewPort.X;
            viewRect.Y = viewPort.Y;

            foreach (UIElement child  in InternalChildren)
            {
                var placement = viewRect;

                if (!(child is MapLayer))
                {
                    placement = Projection.ToRect(Geo.GetArea(child), Extent);
                    //placement.X += viewPort.X;
                    //placement.Y += viewPort.Y;
                }

                child.Arrange(placement);
            }

            return viewPort.Size;
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

        private void VisualExtentValuesChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == string.Empty || e.PropertyName == nameof(Extent.ZoomLevel))
            {
                mapRect = new Rect(Projection.FullMapSizeFor(Extent.ZoomLevel));
                // Measurements only change when the zoom level changes.  Screen size affects the visual port,
                // and GeoArea affects the geographic area under the map
                InvalidateMeasure();
            }

            InvalidateArrange();
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

        public void LineUp()
        {
            SetVerticalOffset(VerticalOffset - LineSize);
        }

        public void LineDown()
        {
            SetVerticalOffset(VerticalOffset + LineSize);
        }

        public void LineLeft()
        {
            SetHorizontalOffset(HorizontalOffset - LineSize);
        }

        public void LineRight()
        {
            SetHorizontalOffset(HorizontalOffset + LineSize);
        }

        public void PageUp()
        {
            SetVerticalOffset(VerticalOffset - ViewportHeight);
        }

        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + ViewportHeight);
        }

        public void PageLeft()
        {
            SetHorizontalOffset(HorizontalOffset - ViewportWidth);
        }

        public void PageRight()
        {
            SetHorizontalOffset(HorizontalOffset + ViewportWidth);
        }

        public void MouseWheelUp()
        {
            var mousePosition = Mouse.GetPosition(this);
            // TODO: alter extent to use mouse position for zoom anchor
            Extent.ZoomLevel = Math.Min(MaxZoomLevel, Extent.ZoomLevel + 1);

            SetHorizontalOffset(HorizontalOffset);
            SetVerticalOffset(VerticalOffset);
        }

        public void MouseWheelDown()
        {
            var mousePosition = Mouse.GetPosition(this);
            // TODO: alter extent to use mouse position for zoom anchor
            Extent.ZoomLevel = Math.Max(0, Extent.ZoomLevel - 1);

            SetHorizontalOffset(HorizontalOffset);
            SetVerticalOffset(VerticalOffset);
        }

        public void MouseWheelLeft()
        {
            MouseWheelDown();
        }

        public void MouseWheelRight()
        {
            MouseWheelUp();
        }

        public void SetHorizontalOffset(double offset)
        {
            offset = -Math.Max(0, Math.Min(offset, ExtentWidth - ViewportWidth));
            if (offset != viewPort.X)
            {
                viewPort.X = offset;
                InvalidateArrange();
            }
        }

        public void SetVerticalOffset(double offset)
        {
            offset = -Math.Max(0, Math.Min(offset, ExtentHeight - ViewportHeight));
            if (offset != viewPort.Y)
            {
                viewPort.Y = offset;
                InvalidateArrange();
            }
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            // We'll just assume the source Rect is correct for now.
            // When we actually get to placing things on screen
            // we'll fix this.
            rectangle.Intersect(mapRect);

            return rectangle;
        }


        private async void MapLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MapLoaded;
            loading = false;

            // Size the map so it is big enough to cover the visible area

            var desiredZoomLevel = -1;
            var mapSize = Size.Empty;

            while (mapSize.Width < ActualWidth || mapSize.Height < ActualHeight)
            {
                desiredZoomLevel++;
                mapSize = Projection.FullMapSizeFor(desiredZoomLevel);
            }

            Extent = new Extent
            {
                Area = Projection.World,
                ZoomLevel = desiredZoomLevel
            };

            // for now let's always center it (view port support is comming)
            SetHorizontalOffset((ViewportWidth - ExtentWidth) / 2);
            SetVerticalOffset((ViewportHeight - ExtentHeight) / 2);

            if (ScrollOwner != null)
            {
                ScrollOwner.InvalidateScrollInfo();
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
                    SetZIndex(visual, int.MinValue);

                    Children.Add(visual);
                    tiles.Remove(finishedDrawing);
                }
            }
        }
    }
}