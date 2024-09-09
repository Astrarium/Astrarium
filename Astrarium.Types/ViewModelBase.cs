using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Astrarium.Types
{
    /// <summary>
    /// Base class for all ViewModels.
    /// </summary>
    public abstract class ViewModelBase : PropertyChangedBase
    {
        /// <summary>
        /// Raises when the window or dialog associated with current ViewModel is going to be closed.
        /// </summary>
        public event Action<bool?> Closing;

        /// <summary>
        /// Closes the window associated with current ViewModel.
        /// </summary>
        public virtual void Close()
        {
            Close(null);
        }

        /// <summary>
        /// Sets the dialog result value and closes the dialog associated with the current ViewModel.
        /// </summary>
        /// <param name="dialogResult">Dialog result value to be set.</param>
        public void Close(bool? dialogResult)
        {
            Closing?.Invoke(dialogResult);
        }

        /// <summary>
        /// Called when associated dialog or window is activated
        /// </summary>
        public virtual void OnActivated() { }
    }
}
