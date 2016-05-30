using DHaven.DisCarta.Internals;
using System;
using System.Collections.Generic;
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
