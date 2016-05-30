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
using System.Windows.Media;

namespace DHaven.DisCarta
{
    public class Map : Panel
    {
        public static readonly DependencyProperty ProjectionProperty = DependencyProperty.RegisterAttached("Projection", typeof(IProjection), typeof(Map), new FrameworkPropertyMetadata(new PseudoMercatorProjection(), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty VisualExtentProperty = DependencyProperty.RegisterAttached("VisualExtentProperty", typeof(VisualExtent), typeof(Map), new FrameworkPropertyMetadata(new VisualExtent(), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        private TranslateTransform mapOffset = new TranslateTransform();

        public Map()
        {
            LayoutTransform = mapOffset;
        }

        public IProjection Projection
        {
            get { return GetProjection(this); }
            set { SetProjection(this, value); }
        }

        public VisualExtent VisualExtent
        {
            get { return GetVisualExtent(this); }
            set { SetVisualExtent(this, value); }
        }

        public static IProjection GetProjection(DependencyObject dependencyObject)
        {
            return dependencyObject.GetValue(ProjectionProperty) as IProjection;
        }

        public static void SetProjection(DependencyObject dependencyObject, IProjection value)
        {
            dependencyObject.SetValue(ProjectionProperty, value);
        }

        public static VisualExtent GetVisualExtent(DependencyObject dependencyObject)
        {
            return dependencyObject.GetValue(VisualExtentProperty) as VisualExtent;
        }

        public static void SetVisualExtent(DependencyObject dependencyObject, VisualExtent value)
        {
            dependencyObject.SetValue(VisualExtentProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size tempSize = base.MeasureOverride(availableSize);

            if(Projection == null || VisualExtent == null)
            {
                return tempSize;
            }

            Size mapSize = Projection.FullMapSizeFor(VisualExtent.ZoomLevel);

            foreach(UIElement child in InternalChildren)
            {
                Size desiredSize = child is MapLayer ? mapSize : new Size();
                child.Measure(mapSize);
            }

            return mapSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Size tempSize = base.ArrangeOverride(finalSize);

            Rect viewRect = Projection.ToRect(VisualExtent.Extent, VisualExtent);
            mapOffset.X = viewRect.Size.Width < finalSize.Width ? (finalSize.Width - viewRect.Width) / 2 : -viewRect.X;
            mapOffset.Y = viewRect.Size.Height < finalSize.Height ? (finalSize.Height - viewRect.Height) / 2 : -viewRect.Y;

            if (Projection == null || VisualExtent == null)
            {
                return tempSize;
            }

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

            child.SetBinding(VisualExtentProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath(VisualExtentProperty),
                Mode = BindingMode.TwoWay
            });
        }

        private void UnbindLayer(MapLayer child)
        {
            BindingOperations.ClearBinding(child, ProjectionProperty);
            BindingOperations.ClearBinding(child, VisualExtentProperty);
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
