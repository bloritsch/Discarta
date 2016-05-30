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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
