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

namespace DHaven.DisCarta.PreProcessor.Internal
{
    using Internals;

    /// <summary>
    ///     Base view model class.
    /// </summary>
    /// <typeparam name="T">the type of your model, must be a class</typeparam>
    public abstract class ViewModel<T> : BasePropertyChanged
        where T : class
    {
        private T model;

        /// <summary>
        /// Create a ViewModel without an initial model.
        /// </summary>
        protected ViewModel() : this(null) {}

        /// <summary>
        /// Create a ViewModel with an initial model.
        /// </summary>
        /// <param name="modelIn"></param>
        protected ViewModel(T modelIn)
        {
            Model = modelIn;
        }

        /// <summary>
        /// Gets and sets the ViewModel's model.
        /// </summary>
        public T Model
        {
            get { return model; }
            set
            {
                if (ReferenceEquals(model, value))
                {
                    return;
                }

                model = value;
                RaisePropertyChanged(nameof(Model));
            }
        }
    }
}