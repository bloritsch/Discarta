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
using System.Windows;
using System.Windows.Controls;

namespace DHaven.DisCarta.Internals
{
    /// <summary>
    /// Provides a way for maps and map layers to know when child objects are added.
    /// </summary>
    public class NotifyingUIElementCollection : UIElementCollection, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public NotifyingUIElementCollection(UIElement visualParent, FrameworkElement logicalParent) : base(visualParent, logicalParent)
        {
        }

        public override int Add(UIElement element)
        {
            int value = base.Add(element);
            RaiseCollectionChanged(NotifyCollectionChangedAction.Add, element);

            return value;
        }

        public override void Clear()
        {
            UIElement[] oldChildren = this.Cast<UIElement>().ToArray();
            base.Clear();

            RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, oldChildren);
        }

        public override void Insert(int index, UIElement element)
        {
            base.Insert(index, element);

            RaiseCollectionChanged(NotifyCollectionChangedAction.Add, element);
        }

        public override void Remove(UIElement element)
        {
            base.Remove(element);

            RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, element);
        }

        public override void RemoveAt(int index)
        {
            UIElement element = this[index];
            base.RemoveAt(index);

            RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, element);
        }

        public override void RemoveRange(int index, int count)
        {
            UIElement[] removed = this.Cast<UIElement>().Skip(index).Take(count).ToArray();
            base.RemoveRange(index, count);

            RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, removed);
        }

        private void RaiseCollectionChanged(NotifyCollectionChangedAction action, params UIElement[] affectedElements)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, affectedElements));
        }
    }
}
