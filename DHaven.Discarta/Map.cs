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

using DHaven.DisCarta.Internals;
using DHaven.DisCarta.Projections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System;
using System.Windows.Media;
using System.Windows.Input;

namespace DHaven.DisCarta
{
    public class Map : Panel, IScrollInfo
    {
        public const int MaxZoomLevel = 19; // total of 20 zoom levels from 0-19.
        private const double LineSize = 96 / 2.54; // 1 cm in DPU
        public static readonly DependencyProperty ProjectionProperty = DependencyProperty.RegisterAttached("Projection", typeof(IProjection), typeof(Map), new FrameworkPropertyMetadata(new EquirectangularProjection(), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty ExtentProperty = DependencyProperty.RegisterAttached("ExtentProperty", typeof(Extent), typeof(Map), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange, VisualExtentChanged));

        private Vector viewOffset;
        private Size scrollExtent;
        private Size viewPort;
        private bool loading = true;

        public Map()
        {
            Loaded += MapLoaded;
            ClipToBounds = true;
        }

        private void MapLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MapLoaded;
            loading = false;

            // Size the map so it is big enough to cover the visible area

            int desiredZoomLevel = -1;
            Size mapSize = Size.Empty;

            while(mapSize.Width < ActualWidth || mapSize.Height < ActualHeight)
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
            SetVerticalOffset((ViewportHeight- ExtentHeight) / 2);

            if (ScrollOwner != null)
            {
                ScrollOwner.InvalidateScrollInfo();
            }
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
            get { return scrollExtent.Width; }
        }

        public double ExtentHeight
        {
            get { return scrollExtent.Height; }
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
            get { return viewOffset.X; }
        }

        public double VerticalOffset
        {
            get { return viewOffset.Y; }
        }

        public ScrollViewer ScrollOwner { get; set; }

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

            Size extent = Projection.FullMapSizeFor(Extent.ZoomLevel);
            bool extentOrViewChanged = false;
            if (extent != scrollExtent)
            {
                scrollExtent = extent;
                extentOrViewChanged = true;
            }

            if (availableSize != viewPort)
            {
                viewPort = availableSize;
                extentOrViewChanged = true;
            }

            if (extentOrViewChanged && ScrollOwner != null)
            {
                ScrollOwner.InvalidateScrollInfo();
            }

            foreach (UIElement child in InternalChildren)
            {
                Size desiredSize = child is MapLayer ? scrollExtent : new Size();
                child.Measure(scrollExtent);
            }

            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (loading || Projection == null || Extent == null)
            {
                return finalSize;
            }

            if (finalSize != viewPort)
            {
                viewPort = finalSize;

                if (ScrollOwner != null)
                {
                    ScrollOwner.InvalidateScrollInfo();
                }
            }

            Rect viewRect = Projection.ToRect(Extent.Area, Extent);
            viewRect.X = viewOffset.X;
            viewRect.Y = viewOffset.Y;

            foreach (UIElement child  in InternalChildren)
            {
                Rect placement = child is MapLayer ? viewRect : new Rect();
                child.Arrange(placement);
            }

            return viewPort;
        }

        protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
        {
            NotifyingUIElementCollection collection = new NotifyingUIElementCollection(this, logicalParent);
            collection.CollectionChanged += ChildrenCollectionChanged;

            return collection;
        }

        private void BindLayer(MapLayer child)
        {
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
            BindingOperations.ClearBinding(child, ProjectionProperty);
            BindingOperations.ClearBinding(child, ExtentProperty);
        }

        private static void VisualExtentChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            Map map = d as Map;

            if(map == null)
            {
                return;
            }

            Extent oldExtent = args.OldValue as Extent;
            if (oldExtent != null)
            {
                oldExtent.PropertyChanged -= map.VisualExtentValuesChanged;
            }

            Extent newExtent = args.NewValue as Extent;
            if (newExtent != null)
            {
                newExtent.PropertyChanged += map.VisualExtentValuesChanged;
            }
        }

        private void VisualExtentValuesChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == string.Empty || e.PropertyName == nameof(Extent.ZoomLevel))
            {
                // Measurements only change when the zoom level changes.  Screen size affects the visual port,
                // and GeoArea affects the geographic area under the map
                InvalidateMeasure();
            }

            InvalidateArrange();
        }

        private void ChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.OldItems != null)
            {
                foreach(MapLayer layer in e.OldItems)
                {
                    UnbindLayer(layer);
                }
            }

            if(e.NewItems != null)
            {
                foreach(MapLayer layer in e.NewItems)
                {
                    BindLayer(layer);
                }
            }
        }

        public void LineUp()
        {
            SetVerticalOffset(VerticalOffset + LineSize);
        }

        public void LineDown()
        {
            SetVerticalOffset(VerticalOffset - LineSize);
        }

        public void LineLeft()
        {
            SetHorizontalOffset(HorizontalOffset - LineSize);
        }

        public void LineRight()
        {
            SetHorizontalOffset(HorizontalOffset - LineSize);
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
            Point mousePosition = Mouse.GetPosition(this);
            // TODO: alter extent to use mouse position for zoom anchor
            Extent.ZoomLevel = Math.Min(MaxZoomLevel, Extent.ZoomLevel + 1);
        }

        public void MouseWheelDown()
        {
            Point mousePosition = Mouse.GetPosition(this);
            // TODO: alter extent to use mouse position for zoom anchor
            Extent.ZoomLevel = Math.Max(0, Extent.ZoomLevel - 1);
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
            if (offset != viewOffset.X)
            {
                viewOffset.X = offset;
                CanHorizontallyScroll = ExtentWidth > ViewportWidth;
                InvalidateArrange();
            }
        }

        public void SetVerticalOffset(double offset)
        {
            offset = -Math.Max(0, Math.Min(offset, ExtentHeight - ViewportHeight));
            if (offset != viewOffset.Y)
            {
                viewOffset.Y = offset;
                CanVerticallyScroll = ExtentHeight > ViewportHeight;
                InvalidateArrange();
            }
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            // Called to make a visual available, but that has to be handled in the MapLayer area.
            return Rect.Empty;
        }
    }
}
