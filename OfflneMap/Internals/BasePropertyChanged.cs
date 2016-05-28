using System;
using System.ComponentModel;

namespace DHaven.DisCarta.Internals
{
    /// <summary>
    /// Base class used to simplify INotifyPropertyChanged eventing.
    /// </summary>
    public class BasePropertyChanged : INotifyPropertyChanged
    {
        /// <summary>
        /// Invoked when a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sends the "All Properties Changed" event.  In WPF, you can
        /// send the ProeprtyChangedEventArgs with string.Empty and all
        /// bound properties will be updated.
        /// </summary>
        protected void RaiseAllPropertiesChanged()
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(string.Empty));
            }
        }

        /// <summary>
        /// Sends the property change event for a single property
        /// </summary>
        /// <param name="property"></param>
        protected void RaisePropertyChanged(string property)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
