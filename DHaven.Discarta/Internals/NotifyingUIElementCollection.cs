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

            RaiseCollectionChanged(NotifyCollectionChangedAction.Reset, oldChildren);
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
