using DHaven.DisCarta.Internals;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DHaven.DisCarta
{
    public class MapLayer : Panel
    {
        public IProjection Projection
        {
            get { return Map.GetProjection(this); }
            set { Map.SetProjection(this, value); }
        }

        public VisualExtent VisualExtent
        {
            get { return Map.GetVisualExtent(this); }
            set { Map.SetVisualExtent(this, value); }
        }

        protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
        {
            NotifyingUIElementCollection collection = new NotifyingUIElementCollection(this, logicalParent);
            collection.CollectionChanged += ChildrenCollectionChanged;

            return collection;
        }

        private void ChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (MapLayer layer in e.OldItems)
                {
                    UnbindChild(layer);
                }
            }

            if (e.NewItems != null)
            {
                foreach (MapLayer layer in e.NewItems)
                {
                    BindChild(layer);
                }
            }
        }

        private void BindChild(MapLayer layer)
        {
            throw new NotImplementedException();
        }

        private void UnbindChild(MapLayer layer)
        {
            throw new NotImplementedException();
        }
    }
}
