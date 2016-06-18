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

namespace DHaven.DisCarta
{
    public class Map : Panel
    {
        public static readonly DependencyProperty ProjectionProperty = DependencyProperty.RegisterAttached("Projection", typeof(IProjection), typeof(Map), new FrameworkPropertyMetadata(new EquirectangularProjection(), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty ExtentProperty = DependencyProperty.RegisterAttached("ExtentProperty", typeof(Extent), typeof(Map), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange, VisualExtentChanged));

        private bool loading = true;

        public Map()
        {
            Loaded += MapLoaded;
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

        protected override Size MeasureOverride(Size availableSize)
        {
            Size tempSize = base.MeasureOverride(availableSize);

            if (loading || Projection == null || Extent == null)
            {
                return tempSize;
            }

            Size mapSize = Projection.FullMapSizeFor(Extent.ZoomLevel);

            foreach (UIElement child in InternalChildren)
            {
                Size desiredSize = child is MapLayer ? mapSize : new Size();
                child.Measure(mapSize);
            }

            return mapSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Size tempSize = base.ArrangeOverride(finalSize);

            if (loading || Projection == null || Extent == null)
            {
                return tempSize;
            }

            Rect viewRect = Projection.ToRect(Extent.Area, Extent);
            //viewRect.X = viewRect.Size.Width < ActualWidth ? (ActualWidth - viewRect.Width) / 2 : viewRect.X;
            //viewRect.Y = viewRect.Size.Height < ActualHeight ? (ActualHeight - viewRect.Height) / 2 : viewRect.Y;

            // for now let's always center it (view port support is comming)
            viewRect.X = (ActualWidth - viewRect.Width) / 2;
            viewRect.Y = (ActualHeight - viewRect.Height) / 2;

            foreach (UIElement child  in InternalChildren)
            {
                Rect placement = child is MapLayer ? viewRect : new Rect();
                child.Arrange(placement);
            }

            return finalSize;
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
    }
}
