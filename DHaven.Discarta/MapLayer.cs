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
    using System.Collections.Specialized;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using Internals;

    /// <summary>
    ///     Will be taking lessons from here:
    ///     https://blogs.msdn.microsoft.com/dancre/2006/02/06/implementing-a-virtualized-panel-in-wpf-avalon/
    ///     https://blogs.msdn.microsoft.com/dancre/2006/02/13/implementing-a-virtualizingpanel-part-2-iitemcontainergenerator/
    ///     https://blogs.msdn.microsoft.com/dancre/2006/02/15/implementing-a-virtualizingpanel-part-3-measurecore/
    ///     https://blogs.msdn.microsoft.com/dancre/2006/02/17/implementing-a-virtualizingpanel-part-4-the-goods/
    /// </summary>
    public class MapLayer : VirtualizingPanel
    {
        public IProjection Projection
        {
            get { return Map.GetProjection(this); }
            set { Map.SetProjection(this, value); }
        }

        public Extent VisualExtent
        {
            get { return Map.GetExtent(this); }
            set { Map.SetExtent(this, value); }
        }

        protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
        {
            var collection = new NotifyingUIElementCollection(this, logicalParent);
            collection.CollectionChanged += ChildrenCollectionChanged;

            return collection;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (Projection == null || VisualExtent == null)
            {
                return new Size();
            }

            // Virtualizing Panel Responsibility: visual data I need
            // Ensure the IItemContainerGenerator is initialized (not initialized until InternalChildren is accessed
            // Not sure if still the case..... test
            var children = InternalChildren;
            var generator = ItemContainerGenerator;

            var desiredSize = Projection.FullMapSizeFor(VisualExtent.ZoomLevel);

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(desiredSize);
            }

            // Virtualizing Panel Responsibility: visual data to get rid of

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var renderSize = base.ArrangeOverride(finalSize);

            foreach (UIElement child in InternalChildren)
            {
                var isPlaced = PlaceIfArea(child);

                if (!isPlaced)
                {
                    isPlaced = PlaceIfPoint(child);
                }

                if (!isPlaced)
                {
                    // If this isn't a map element, hide it.
                    child.Arrange(Rect.Empty);
                }
            }

            return renderSize;
        }

        private bool PlaceIfArea(UIElement element)
        {
            var elementExtent = GeoArea.Intersection(Geo.GetArea(element), Projection.World);
            var isArea = !elementExtent.IsEmpty;

            if (isArea)
            {
                var destRect = Projection.ToRect(elementExtent, VisualExtent);
                element.Arrange(destRect);
            }

            return isArea;
        }

        private bool PlaceIfPoint(UIElement element)
        {
            var elementLocation = Geo.GetLocation(element);
            var isPoint = !elementLocation.IsEmpty;

            if (isPoint)
            {
                var hotSpotPoint = Projection.ToPoint(elementLocation, VisualExtent);
                var pointSize = element.DesiredSize;

                // Until I add HotSpot support, center over the point
                var destRect = new Rect(hotSpotPoint, pointSize);
                destRect.X -= pointSize.Width / 2;
                destRect.Y -= pointSize.Height / 2;

                element.Arrange(destRect);
            }

            return isPoint;
        }

        private void ChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (UIElement child in e.OldItems)
                {
                    UnbindChild(child);
                }
            }

            if (e.NewItems != null)
            {
                foreach (UIElement child in e.NewItems)
                {
                    BindChild(child);
                }
            }
        }

        private void BindChild(UIElement child)
        {
            // Stuff needed to manage the child will be bound here
            BindingOperations.SetBinding(child, Map.ProjectionProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath(Map.ProjectionProperty),
                Mode = BindingMode.OneWay
            });
        }

        private void UnbindChild(UIElement child)
        {
            // Stuff needed to manage the child will be unbound here
            BindingOperations.ClearBinding(child, Map.ProjectionProperty);
        }
    }
}