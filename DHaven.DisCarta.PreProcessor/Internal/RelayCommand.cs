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
    using System;
    using System.Windows.Input;

    /// <summary>
    /// Standard delegate ICommand interface.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Func<object, bool> canExecute;
        private readonly Action<object> execute;

        /// <summary>
        /// Create a RelayCommand with parameterless functions.
        /// </summary>
        /// <param name="executeIn">the Action to invoke on Execute()</param>
        /// <param name="canExecuteIn">the Func to invoke on CanExecute()</param>
        public RelayCommand(Action executeIn, Func<bool> canExecuteIn = null)
            : this(param => executeIn(), param => canExecuteIn?.Invoke() ?? true)
        {
            if (executeIn == null)
            {
                throw new ArgumentNullException(nameof(executeIn));
            }
        }

        /// <summary>
        /// Create a RelayCommand with object parameter functions.
        /// </summary>
        /// <param name="executeIn">the Action to invoke on Execute()</param>
        /// <param name="canExecuteIn">the Func to invoke on CanExecute()</param>
        public RelayCommand(Action<object> executeIn, Func<object, bool> canExecuteIn = null)
        {
            if (executeIn == null)
            {
                throw new ArgumentNullException(nameof(executeIn));
            }

            execute = executeIn;
            canExecute = canExecuteIn ?? (param => true);
        }

        public void RaisCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #region Implementation of ICommand

        /// <summary>Defines the method that determines whether the command can execute in its current state.</summary>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        public bool CanExecute(object parameter)
        {
            return canExecute(parameter);
        }

        /// <summary>Defines the method to be called when the command is invoked.</summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        public void Execute(object parameter)
        {
            execute(parameter);
        }

        /// <summary>
        /// The CanExecuteChanged event.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        #endregion
    }
}